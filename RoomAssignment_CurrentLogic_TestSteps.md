# GetCurrentRoomAssignmentAsync Logic Test Steps

## Logic Verification
The `GetCurrentRoomAssignmentAsync` method should return assignments that satisfy:
- `StartDate <= today` (assignment has started)
- `EndDate IS NULL OR EndDate >= today` (assignment is ongoing or ends in future)
- `IsDeleted = false` (not soft deleted)
- If multiple matches, return the one with most recent `StartDate`

## Manual Test Scenarios

### 1. Active Assignment (Ongoing)
**Setup:** Create assignment with StartDate in past, EndDate = null
```
POST /api/v1.0/Guests/1/room-assignments
{
  "guestId": 1,
  "roomNumber": "101",
  "startDate": "2024-01-01T10:00:00Z",  // Past date
  "endDate": null                         // Ongoing
}
```
**Expected:** GET `/api/v1.0/Guests/1/current-room` returns 200 with assignment

### 2. Active Assignment (Future End Date)
**Setup:** Create assignment with StartDate in past, EndDate in future
```
POST /api/v1.0/Guests/1/room-assignments
{
  "guestId": 1,
  "roomNumber": "102",
  "startDate": "2024-01-01T10:00:00Z",  // Past date
  "endDate": "2024-12-31T10:00:00Z"     // Future date
}
```
**Expected:** GET `/api/v1.0/Guests/1/current-room` returns 200 with assignment

### 3. Expired Assignment
**Setup:** Create assignment with EndDate in past
```
POST /api/v1.0/Guests/1/room-assignments
{
  "guestId": 1,
  "roomNumber": "103",
  "startDate": "2024-01-01T10:00:00Z",  // Past date
  "endDate": "2024-01-01T10:00:00Z"     // Past date (expired)
}
```
**Expected:** GET `/api/v1.0/Guests/1/current-room` returns 404 "Aktif oda ataması bulunamadı"

### 4. Future Assignment
**Setup:** Create assignment with StartDate in future
```
POST /api/v1.0/Guests/1/room-assignments
{
  "guestId": 1,
  "roomNumber": "104",
  "startDate": "2024-12-31T10:00:00Z",  // Future date
  "endDate": "2025-01-05T10:00:00Z"
}
```
**Expected:** GET `/api/v1.0/Guests/1/current-room` returns 404 "Aktif oda ataması bulunamadı"

### 5. Multiple Active Assignments (Most Recent Wins)
**Setup:** Create two ongoing assignments with different start dates
```
POST /api/v1.0/Guests/1/room-assignments
{
  "guestId": 1,
  "roomNumber": "105",
  "startDate": "2024-01-01T10:00:00Z",  // Older
  "endDate": null
}

POST /api/v1.0/Guests/1/room-assignments
{
  "guestId": 1,
  "roomNumber": "106",
  "startDate": "2024-01-15T10:00:00Z",  // Newer
  "endDate": null
}
```
**Expected:** GET `/api/v1.0/Guests/1/current-room` returns 200 with room "106" (most recent StartDate)

## API Test Commands

### Login First
```bash
curl -X POST http://localhost:5146/api/v1.0/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email": "ahmet@guestflow.com", "password": "Admin123!"}'
```

### Test Current Assignment
```bash
# Replace {token} with actual JWT token from login
curl -X GET http://localhost:5146/api/v1.0/Guests/1/current-room \
  -H "Authorization: Bearer {token}"
```

## Database Query Verification

The logic translates to this SQL query:
```sql
SELECT TOP 1 *
FROM RoomAssignments
WHERE GuestId = @guestId
  AND StartDate <= CAST(GETUTCDATE() AS DATE)
  AND (EndDate IS NULL OR EndDate >= CAST(GETUTCDATE() AS DATE))
  AND IsDeleted = 0
ORDER BY StartDate DESC
```

## Edge Cases Covered
- ✅ Null EndDate (ongoing assignments)
- ✅ Past EndDate (expired assignments)
- ✅ Future StartDate (upcoming assignments)
- ✅ Multiple overlapping assignments (most recent wins)
- ✅ Deleted assignments (excluded)
- ✅ UTC date handling (consistent timezone)
