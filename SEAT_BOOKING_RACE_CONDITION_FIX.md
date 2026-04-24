# Seat Booking Race Condition Fix

## Problem Description

**Critical Bug**: Multiple users could book the same seat simultaneously, even though the seat should be locked after the first booking.

### Scenario
1. User A selects Seat 5 and clicks "Confirm Booking"
2. User B selects Seat 5 and clicks "Confirm Booking" (at almost the same time)
3. **Both bookings succeed** ❌ (Should only allow one)

### Root Cause: Race Condition

The original code had a **race condition** in the booking creation process:

```csharp
// Step 1: Check if seat is booked
var existingBookings = await _context.Bookings...
var alreadyBooked = request.SeatNumbers.Intersect(existingBookings).ToList();
if (alreadyBooked.Any()) {
    throw new Exception("Seats already booked");
}

// Step 2: Create booking
var booking = new Booking { ... };
_context.Bookings.Add(booking);

// Step 3: Save to database
await _context.SaveChangesAsync();
```

**The Problem**:
- User A: Checks seat → Not booked ✓
- User B: Checks seat → Not booked ✓ (User A hasn't saved yet!)
- User A: Saves booking → Success
- User B: Saves booking → Success ❌ (Should have failed!)

This is called a **"Time-of-Check to Time-of-Use" (TOCTOU)** race condition.

## Solution: Database Transaction with Serializable Isolation

Added a **database transaction** with **Serializable isolation level** to prevent concurrent bookings of the same seat.

### What Changed

**File**: `backend/BusBooking.Application/Bookings/Commands/CreateBooking/CreateBookingCommand.cs`

**Before** (Vulnerable to race conditions):
```csharp
// Check if seats are already booked
var existingBookings = await _context.Bookings...
var alreadyBooked = request.SeatNumbers.Intersect(existingBookings).ToList();
if (alreadyBooked.Any()) {
    throw new Exception($"Seats already booked: {string.Join(", ", alreadyBooked)}");
}

// Create booking
var booking = new Booking { ... };
_context.Bookings.Add(booking);
await _context.SaveChangesAsync(cancellationToken);
```

**After** (Protected with transaction):
```csharp
// Use a transaction with serializable isolation level to prevent race conditions
using var transaction = await _context.Database.BeginTransactionAsync(
    System.Data.IsolationLevel.Serializable, cancellationToken);

try
{
    // Check if seats are already booked
    var existingBookings = await _context.Bookings...
    var alreadyBooked = request.SeatNumbers.Intersect(existingBookings).ToList();
    if (alreadyBooked.Any()) {
        await transaction.RollbackAsync(cancellationToken);
        throw new Exception($"Seats already booked: {string.Join(", ", alreadyBooked)}");
    }

    // Create booking
    var booking = new Booking { ... };
    _context.Bookings.Add(booking);
    
    // Create booking seats
    foreach (var seatNumber in request.SeatNumbers) {
        // ... create seat bookings
    }

    await _context.SaveChangesAsync(cancellationToken);
    await transaction.CommitAsync(cancellationToken);

    return new BookingConfirmationDto { ... };
}
catch (Exception)
{
    await transaction.RollbackAsync(cancellationToken);
    throw;
}
```

## How It Works

### 1. **Serializable Isolation Level**

The `Serializable` isolation level is the highest level of transaction isolation. It ensures that:
- Transactions are executed as if they were running one after another (serially)
- No other transaction can read or modify data that this transaction is working with
- Prevents phantom reads, dirty reads, and non-repeatable reads

### 2. **Transaction Flow**

**Scenario with Fix**:
1. User A starts transaction → Locks seat data
2. User B tries to start transaction → **Waits** (blocked by User A's lock)
3. User A checks seat → Not booked ✓
4. User A creates booking → Success
5. User A commits transaction → Releases lock
6. User B's transaction starts → Checks seat → **Already booked** ✗
7. User B's transaction rolls back → Error message shown

### 3. **Rollback on Error**

If any error occurs during the booking process:
- The transaction is rolled back
- No partial data is saved
- Database remains in a consistent state

## Benefits

### ✅ Prevents Double Booking
- Only one user can book a seat at a time
- Second user gets a clear error message: "Seats already booked: 5"

### ✅ Data Consistency
- All-or-nothing approach
- Either the entire booking succeeds, or nothing is saved

### ✅ Automatic Locking
- Database handles the locking mechanism
- No need for manual lock management

### ✅ Handles Concurrent Users
- Multiple users can book different seats simultaneously
- Only blocks when trying to book the same seat

## Performance Considerations

### Potential Impact
- **Serializable isolation** can reduce concurrency
- Transactions may wait for locks to be released
- Slight increase in booking time (milliseconds)

### Mitigation
- Locks are held for a very short time (< 100ms typically)
- Only affects users booking the exact same seat
- Most bookings won't experience any delay

### Scalability
For high-traffic scenarios, consider:
1. **Optimistic Locking**: Use row versioning instead of serializable transactions
2. **Seat Reservation System**: Reserve seats for 5 minutes before final booking
3. **Queue System**: Use a message queue for booking requests
4. **Caching**: Cache booked seats with short TTL

## Testing the Fix

### Test Case 1: Sequential Bookings (Should Work)
1. User A books Seat 5 → Success ✓
2. User B tries to book Seat 5 → Error: "Seats already booked: 5" ✓

### Test Case 2: Concurrent Bookings (Should Work)
1. User A and User B both click "Confirm" for Seat 5 at the same time
2. One succeeds, one fails with error message ✓

### Test Case 3: Different Seats (Should Work)
1. User A books Seat 5 → Success ✓
2. User B books Seat 6 → Success ✓ (No conflict)

### Test Case 4: Multiple Seats (Should Work)
1. User A books Seats 5, 6, 7 → Success ✓
2. User B tries to book Seats 6, 7, 8 → Error: "Seats already booked: 6, 7" ✓

## Frontend Behavior

The frontend already has proper handling:

### 1. **Visual Indication**
```html
<button 
  class="seat-button"
  [class.booked]="seat.isBooked"
  [disabled]="seat.isBooked">
```
- Booked seats are grayed out
- Booked seats are disabled (not clickable)

### 2. **Tooltip**
```html
[matTooltip]="seat.isBooked ? 'Booked' : ..."
```
- Shows "Booked" tooltip on hover

### 3. **Click Prevention**
```typescript
toggleSeat(seat: Seat): void {
  if (seat.isBooked) {
    this.snackBar.open('This seat is already booked', 'Close', { duration: 2000 });
    return;
  }
  // ... rest of logic
}
```
- Shows error message if user somehow clicks a booked seat

### 4. **Real-time Updates**
The `loadBookedSeats()` method fetches the latest booked seats:
```typescript
loadBookedSeats(): void {
  this.bookingService.getBookedSeatsForTrip(this.tripId).subscribe({
    next: (bookedSeats: string[]) => {
      this.seats.forEach(row => {
        row.forEach(seat => {
          if (bookedSeats.includes(seat.seatNumber)) {
            seat.isBooked = true;
          }
        });
      });
    }
  });
}
```

## Deployment Steps

### 1. Build Backend
```bash
cd backend
dotnet build
```

### 2. Restart Backend
```bash
dotnet run --project BusBooking.API
```

### 3. Test
- Open two browser windows (or use incognito mode)
- Login as different users
- Try to book the same seat simultaneously
- Verify only one booking succeeds

## Alternative Solutions (Not Implemented)

### 1. **Optimistic Locking**
```csharp
// Add RowVersion to Booking entity
[Timestamp]
public byte[] RowVersion { get; set; }

// EF Core will automatically check version on save
```
**Pros**: Better performance, less blocking  
**Cons**: More complex error handling

### 2. **Pessimistic Locking**
```csharp
var booking = await _context.Bookings
    .FromSqlRaw("SELECT * FROM Bookings WHERE Id = {0} FOR UPDATE", id)
    .FirstOrDefaultAsync();
```
**Pros**: Explicit control over locks  
**Cons**: Database-specific SQL, harder to maintain

### 3. **Distributed Lock (Redis)**
```csharp
using (var redisLock = await redisLockFactory.CreateLockAsync($"seat:{tripId}:{seatNumber}", TimeSpan.FromSeconds(30)))
{
    if (redisLock != null)
    {
        // Create booking
    }
}
```
**Pros**: Works across multiple servers  
**Cons**: Requires Redis, more infrastructure

## Summary

✅ **Problem**: Race condition allowing double booking of seats  
✅ **Solution**: Database transaction with Serializable isolation level  
✅ **Result**: Only one user can book a seat at a time  
✅ **Impact**: Minimal performance impact, significant reliability improvement  

The fix ensures data integrity and prevents the critical bug where multiple users could book the same seat. The solution is simple, effective, and leverages the database's built-in transaction capabilities.
