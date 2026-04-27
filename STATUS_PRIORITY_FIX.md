# Bus Status Priority Fix

## Issue
When a bus was **rejected** by admin (status = 4), it was incorrectly showing BOTH "Disabled" and "Rejected" statuses because the logic was checking `!isAvailable` before checking the actual status value.

### Example of the Problem:
- Admin rejects a bus → Status = 4 (Rejected), IsAvailable = false
- Old logic checked `!isAvailable` first → Showed "Disabled by Admin"
- Then also showed "Rejected by Admin"
- Result: Confusing double status display

## Solution
Changed the logic to **prioritize the actual status value** over the `isAvailable` flag.

### New Priority Order:

1. **Status = 4 (Rejected)** → Show "Rejected by Admin" ONLY
2. **Status = 3 (Disabled)** → Show "Disabled by Admin"
3. **Status = 2 (Approved) + IsAvailable = false** → Show "Disabled by Admin"
4. **Status = 2 (Approved) + IsAvailable = true** → Show "Active"
5. **Status = 1 (Pending)** → Show "Pending Approval"

## Logic Flow

### Before (WRONG):
```typescript
if (status === 3 || !isAvailable) {
  return 'Disabled by Admin';  // ❌ Rejected buses also hit this!
}

if (status === 4) {
  return 'Rejected by Admin';  // This was checked AFTER disabled
}
```

### After (CORRECT):
```typescript
// Check rejected FIRST (highest priority)
if (status === 4) {
  return 'Rejected by Admin';  // ✅ Rejected buses stop here
}

// Then check disabled (status 3 OR approved but not available)
if (status === 3 || (status === 2 && !isAvailable)) {
  return 'Disabled by Admin';  // ✅ Only truly disabled buses
}
```

## Status Scenarios

### Scenario 1: Bus Rejected by Admin
- **Database**: Status = 4, IsAvailable = false
- **Display**: 
  - Badge: "Rejected by Admin" (red)
  - Notice: "Bus Registration Rejected"
  - Rejection Reason Box: Shows admin's reason
- **No "Disabled" status shown** ✅

### Scenario 2: Operator Disabled by Admin
- **Database**: Status = 2 (Approved), IsAvailable = false
- **Display**:
  - Badge: "Disabled by Admin" (red, pulsing)
  - Notice: "Bus Disabled by Admin"
- **No "Rejected" status shown** ✅

### Scenario 3: Bus Explicitly Disabled (Status 3)
- **Database**: Status = 3, IsAvailable = false
- **Display**:
  - Badge: "Disabled by Admin" (red, pulsing)
  - Notice: "Bus Disabled by Admin"
- **No "Rejected" status shown** ✅

### Scenario 4: Bus Pending Approval
- **Database**: Status = 1, IsAvailable = false
- **Display**:
  - Badge: "Pending Approval" (yellow)
  - Notice: "Awaiting Admin Approval"
- **No other statuses shown** ✅

### Scenario 5: Bus Active
- **Database**: Status = 2, IsAvailable = true
- **Display**:
  - Badge: "Active" (green)
  - No notice box
- **No other statuses shown** ✅

## Code Changes

### Method: `getStatusClass()`
```typescript
getStatusClass(status: number, isAvailable: boolean): string {
  // Check status first, then isAvailable
  switch (status) {
    case 1: return 'pending';
    case 2: 
      // Only check isAvailable for approved buses
      return (!isAvailable) ? 'disabled' : 'approved';
    case 3: return 'disabled';
    case 4: return 'rejected';  // ✅ Rejected gets its own class
    default: return 'unknown';
  }
}
```

### Method: `getBusStatusDisplay()`
```typescript
getBusStatusDisplay(status: number, isAvailable: boolean): string {
  // Check status first, then isAvailable
  switch (status) {
    case 1: return 'Pending Approval';
    case 2:
      // Only check isAvailable for approved buses
      return (!isAvailable) ? 'Disabled by Admin' : 'Active';
    case 3: return 'Disabled by Admin';
    case 4: return 'Rejected by Admin';  // ✅ Clear rejected label
    default: return 'Unknown';
  }
}
```

### Method: `getStatusMessage()`
```typescript
getStatusMessage(status: number, isAvailable: boolean) {
  // Rejected status - HIGHEST PRIORITY (don't check isAvailable)
  if (status === 4) {
    return {
      title: 'Bus Registration Rejected',
      message: '...',
      type: 'rejected'
    };
  }
  
  // Disabled status - Check both status 3 AND approved buses with isAvailable=false
  if (status === 3 || (status === 2 && !isAvailable)) {
    return {
      title: 'Bus Disabled by Admin',
      message: '...',
      type: 'disabled'
    };
  }
  
  // ... rest of statuses
}
```

## Key Improvements

✅ **Clear Separation**: Rejected and Disabled are now distinct
✅ **Correct Priority**: Status value checked before isAvailable flag
✅ **No Confusion**: Only ONE status shown per bus
✅ **Accurate Display**: Matches actual database state

## Testing

### Test Case 1: Reject a Bus
1. Admin rejects a bus with reason "Invalid license plate"
2. Operator views "Submission History"
3. **Expected**: 
   - Badge shows "Rejected by Admin" (red)
   - Notice shows "Bus Registration Rejected"
   - Rejection reason box shows "Invalid license plate"
   - NO "Disabled" status anywhere ✅

### Test Case 2: Disable an Operator
1. Admin disables a bus operator
2. Operator views "My Fleet Management"
3. **Expected**:
   - Badge shows "Disabled by Admin" (red, pulsing)
   - Notice shows "Bus Disabled by Admin"
   - NO "Rejected" status anywhere ✅

### Test Case 3: Pending Bus
1. Operator submits new bus
2. Operator views "Submission History"
3. **Expected**:
   - Badge shows "Pending Approval" (yellow)
   - Notice shows "Awaiting Admin Approval"
   - NO "Disabled" or "Rejected" status ✅

## Status
✅ **FIXED** - Rejected buses now show ONLY rejected status, not disabled status
