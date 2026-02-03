# GuestFlow API Integration Guide

**Base URL**: `https://api.guestflow.com/api/v1.0`  
**API Version**: `v1.0`  
**Format**: `application/json`

---

## 🔐 Authentication

GuestFlow uses JWT (JSON Web Tokens) for secure communication.

### 1. Obtain Access Token

```http
POST /auth/login
{
  "email": "staff@hotel.com",
  "password": "secure-password"
}
```

**Response**: `200 OK` with `{ "accessToken": "...", "refreshToken": "..." }`

### 2. Authorization Header

Include the access token in every subsequent request:
`Authorization: Bearer <access_token>`

---

## 👥 Essential Resource: Guests

### List Guests

`GET /guests?pageNumber=1&pageSize=20`

- **Filters**: `searchTerm`, `nationality`, `vipStatus`.
- **Sort**: `sortBy=LastName&sortOrder=asc`.

### Create Guest

`POST /guests`

```json
{
  "firstName": "Alex",
  "lastName": "Rivera",
  "email": "alex.rivera@example.com",
  "vipStatus": "Gold"
}
```

---

## 🚗 Operational Hub: Transfers

### Create Transfer

`POST /transfers`

- **Types**: `AirportToHotel`, `HotelToRestaurant`, `CityTour`.
- **Payload**: Requires `guestId`, `scheduledDate`, and `pickupLocation`.

### Update Status

`PATCH /transfers/{id}/status`

- **Statuses**: `Pending`, `Confirmed`, `InTransit`, `Completed`, `Cancelled`.

---

## 🧾 Billing & Finance

### Generate Invoice

`POST /invoices/{id}/generate-pdf`

- Generates a professional PDF voucher with QR code support.

### Post to Journal

`POST /journal/post`

- **Strict Idempotency**: Can only post an invoice once.
- **Requirement**: Total Debit must equal Total Credit.

---

## 🛡 Error Handling & Status Codes

| Code | Reason | Action |
| :--- | :--- | :--- |
| `200/201` | Success | Continue flow. |
| `400` | Validation Fail | Check `details` array in response. |
| `401` | Unauthorized | Refresh token or re-login. |
| `403` | Forbidden | Insufficient permissions for role. |
| `429` | Rate Limit | Implement exponential backoff. |
| `500` | Server Error | Contact `api-support@guestflow.com`. |

### Standard Error Object

```json
{
  "success": false,
  "error": {
    "code": "ENTITY_NOT_FOUND",
    "message": "The requested guest does not exist.",
    "correlationId": "..."
  }
}
```

---

## 📡 Real-time Updates (SignalR)

Subscribe to notifications via:
`wss://api.guestflow.com/hubs/notifications`

**Events**:

- `ReceiveTransferUpdate`: Triggered when a driver is assigned.
- `ReceiveInvoicePosted`: Triggered when an accounting entry is finalized.
