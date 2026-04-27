# User Panel Not Showing Buses - Diagnosis

## The Real Issue

**Having a bus with `IsAvailable=true` in the Buses table is NOT enough!**

The user panel displays **TRIPS**, not just buses. For a bus to appear, you need:

### ✅ Requirements Checklist

1. **Bus exists** in `Buses` table
2. **Bus.IsAvailable = true**
3. **Bus.Status = Approved (2)**
4. **⚠️ TRIP scheduled** for that bus in `Trips` table
5. **Trip.Status = Scheduled**
6. **Trip.DepartureDateTime >= Today**

## Why Your Bus Isn't Showing

Even though your bus has `IsAvailable=true`, it's likely missing one of these:

### Most Common Reason: No Trip Scheduled

**The operator must schedule a trip for the bus!**

Steps to schedule a trip:
1. Login as the bus operator
2. Go to "My Fleet Management" page
3. Find your approved bus
4. Click "Schedule Trip" button
5. Fill in:
   - Trip Date (today or future)
   - Source Address
   - Destination Address
   - Pickup Point
   - Drop Point
   - Departure Time
   - Arrival Time
   - Base Price per Seat
6. Click "Schedule Trip"

### Other Possible Reasons

1. **Trip is in the past**: The trip's departure date/time has already passed
2. **Trip status is not "Scheduled"**: Trip might be Completed or Cancelled
3. **Bus status is not "Approved"**: Bus might still be Pending or Rejected
4. **No pricing set**: Trip needs pricing information

## How the Search Works

```
User Panel Search
    ↓
Search Trips API (/api/buses/search-trips)
    ↓
Query Trips Table (NOT Buses Table!)
    ↓
Filter by:
  - Trip.Status = Scheduled
  - Bus.Status = Approved
  - Bus.IsAvailable = true
  - DepartureDateTime >= Today
  - (Optional) Source/Destination/Date filters
    ↓
Return matching TRIPS with bus details
```

## Verification Steps

### Step 1: Check if Bus Exists and is Available
```sql
SELECT 
    "Id",
    "BusName",
    "BusNumber",
    "Status",
    "IsAvailable"
FROM "Buses"
WHERE "IsAvailable" = true;
```

Expected: Your bus should appear with `Status = 2` (Approved) and `IsAvailable = true`

### Step 2: Check if Trips are Scheduled
```sql
SELECT 
    t."Id" as "TripId",
    b."BusName",
    b."BusNumber",
    b."IsAvailable",
    t."Status" as "TripStatus",
    t."DepartureDateTime",
    t."SourceAddress",
    t."DestinationAddress"
FROM "Trips" t
INNER JOIN "Buses" b ON t."BusId" = b."Id"
WHERE b."IsAvailable" = true
  AND b."Status" = 2
  AND t."Status" = 1
  AND t."DepartureDateTime" >= NOW()
ORDER BY t."DepartureDateTime";
```

Expected: Should show trips for your available buses

### Step 3: Check Backend Logs

When you search in the user panel, check the backend console for:
```
[DEBUG-SEARCH] ========== SEARCH TRIPS QUERY ==========
[DEBUG-SEARCH] Filters - Source: ANY, Destination: ANY, Date: TODAY+
[DEBUG-SEARCH] Initial query filters applied: TripStatus=Scheduled, BusStatus=Approved, IsAvailable=true
[DEBUG-SEARCH] ========== RESULTS ==========
[DEBUG-SEARCH] Query returned X trips
```

If it says "NO TRIPS FOUND", check the possible reasons listed.

## Solution

### If No Trips Exist:
**Operator needs to schedule trips!**

1. Login as operator
2. Go to "My Fleet Management"
3. Click "Schedule Trip" on an approved bus
4. Fill in trip details
5. Submit

### If Trips Exist but Not Showing:
Check:
1. Trip departure date is today or future
2. Trip status is "Scheduled" (not Completed/Cancelled)
3. Bus status is "Approved"
4. Bus IsAvailable is true

## Database Status Values

### Bus Status (BusStatus enum)
- 1 = Pending
- 2 = Approved ✅ (Required)
- 3 = Disabled
- 4 = Rejected

### Trip Status (TripStatus enum)
- 1 = Scheduled ✅ (Required)
- 2 = InProgress
- 3 = Completed
- 4 = Cancelled

## Quick Fix SQL (If Needed)

If you have buses but no trips, you can manually insert a test trip:

```sql
-- First, get a bus ID and route ID
SELECT b."Id" as "BusId", b."BusName", b."RouteId"
FROM "Buses" b
WHERE b."IsAvailable" = true AND b."Status" = 2
LIMIT 1;

-- Then insert a trip (replace the GUIDs with actual values from above)
INSERT INTO "Trips" (
    "Id", "BusId", "RouteId", "Status",
    "SourceAddress", "DestinationAddress",
    "PickupPoint", "DropPoint",
    "DepartureDateTime", "ArrivalDateTime",
    "CreatedAt"
)
VALUES (
    gen_random_uuid(),
    'YOUR-BUS-ID-HERE',
    'YOUR-ROUTE-ID-HERE',
    1, -- Scheduled
    'Mumbai Central',
    'Pune Station',
    'Platform 5',
    'Main Gate',
    NOW() + INTERVAL '1 day', -- Tomorrow
    NOW() + INTERVAL '1 day 4 hours', -- Tomorrow + 4 hours
    NOW()
);

-- Add pricing for the trip
INSERT INTO "TripPricing" (
    "Id", "TripId", "SeatType", "Price", "CreatedAt"
)
VALUES (
    gen_random_uuid(),
    (SELECT "Id" FROM "Trips" ORDER BY "CreatedAt" DESC LIMIT 1),
    'Standard',
    500.00,
    NOW()
);
```

## Summary

✅ **Bus with IsAvailable=true exists** → Good!
❌ **No trips scheduled for that bus** → This is why it's not showing!

**Action Required**: Operator must schedule trips through the "My Fleet Management" page.
