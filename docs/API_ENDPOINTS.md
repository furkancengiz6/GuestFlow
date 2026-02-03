# GuestFlow API Reference Index

Comprehensive list of available endpoints for the GuestFlow Tourism Operations platform.

---

## 🔐 Identity & Access

| Endpoint | Method | Description |
| :--- | :--- | :--- |
| `/auth/login` | POST | Authenticate and retrieve tokens. |
| `/auth/refresh-token` | POST | Exchange refresh token for new access token. |
| `/auth/me` | GET | Retrieve current profile information. |
| `/personnel` | GET/POST | Manage hotel staff and roles. |

---

## 👤 Guest CRM

| Endpoint | Method | Description |
| :--- | :--- | :--- |
| `/guests` | GET/POST | Search or create guest profiles. |
| `/guests/{id}` | GET/PUT/DELETE | Retrieve, update, or remove a specific guest. |
| `/guests/{id}/timeline` | GET | View all interactions and services for a guest. |
| `/guests/{id}/pref` | GET/PUT | Manage guest specific preferences. |

---

## 🚗 Logistics & Transfers

| Endpoint | Method | Description |
| :--- | :--- | :--- |
| `/transfers` | GET/POST | Manage transfer bookings. |
| `/transfers/{id}/status` | PATCH | Update operational status (e.g., Completed). |
| `/vehicles` | GET/POST | Fleet management. |
| `/airports` | GET | Reference data for airport pickup/dropoff. |

---

## 🛥️ Tours & Experiences

| Endpoint | Method | Description |
| :--- | :--- | :--- |
| `/citytours` | GET/POST | Manage urban tour offerings. |
| `/yachttours` | GET/POST | Manage maritime experience offerings. |
| `/servicepackages` | GET/POST | Bundled service itinerary management. |

---

## 🧾 Billing & Finance

| Endpoint | Method | Description |
| :--- | :--- | :--- |
| `/invoices` | GET/POST | Manage billing records. |
| `/payments` | GET/POST | Process and track guest payments. |
| `/journal/preview` | GET | Preview accounting impact before posting. |
| `/journal/post` | POST | Finalize entry to General Ledger. |
| `/currency/rates` | GET | Fetch real-time exchange rate configurations. |

---

## 📊 Analytics & Ops

| Endpoint | Method | Description |
| :--- | :--- | :--- |
| `/dashboard/overview` | GET | High-level operational stats for Concierge desks. |
| `/reports/revenue` | GET | Historical revenue and profitability analysis. |
| `/export/guests` | GET | Bulk export profiles (Excel/CSV). |
| `/health/detailed` | GET | Live system health and integration status. |

---

## 🛠 Developer Utilities

| Endpoint | Method | Description |
| :--- | :--- | :--- |
| `/files/upload` | POST | Upload documents/images to Azure Blob Storage. |
| `/localization` | GET | Retrieve translation strings and locale info. |
| `/settings` | GET/PUT | Manage system-wide feature flags and constants. |

---

**Note**: All endpoints follow the `/api/v1.0` base path.
