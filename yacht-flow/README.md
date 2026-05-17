# VOY | Elite Yachting & Charters (GuestFlow) ⚓

![VOY Hero](public/hero-bg.png)

VOY is an ultra-luxury, premium yacht charter platform built to connect world-class travelers with the most exclusive yacht collection on the Turkish Riviera (Bodrum, Göcek, Marmaris). 

Designed with an immersive dark-theme aesthetic and a seamless user experience, VOY operates as a multi-role SaaS platform catering to **Guests**, **Hosts**, and **Administrators**.

---

## 🌟 Key Features

### 🤵 Guest Experience (B2C)
- **Bilingual Interface:** Real-time toggling between English (EN) and Turkish (TR).
- **Curated Fleet:** Browse luxury yachts with dynamic filters (Length, Guests, Cabins, Price).
- **Luxury Configurator:** A unique "Design Your Experience" modal to add Private Chefs, Jet-Skis, and curated dining experiences (e.g., Riviera Italian) to your voyage.
- **Seamless Booking & Checkout:** Live price calculation and Stripe integration for secure payments.
- **Live Bodum Weather Widget:** Real-time maritime weather data on the fleet page.

### ⚓ Host Experience (B2B SaaS)
- **Host Dashboard:** Secure login for yacht owners/agencies.
- **Fleet Management:** Add, edit, and manage vessel details (images, pricing, amenities).
- **Booking Analytics:** Track upcoming voyages and total revenue in a sleek, glassmorphic interface.

### 👑 Admin Experience (Platform Control)
- **Centralized Control:** Approve or reject newly registered yachts.
- **Global Analytics:** Monitor total platform commissions, active users, and all reservations across the system.

---

## 🖼️ Gallery

### The Fleet
![The Fleet](public/yacht-1.png)

### Curated Destinations
![Destinations](public/dest-bodrum.png)

### The Experience
![Experience](public/experience.png)

---

## 💻 Tech Stack

- **Framework:** Next.js 15+ (App Router, React 19)
- **Styling:** Tailwind CSS (Custom Dark Mode & Glassmorphism UI)
- **Database:** Prisma ORM with SQLite
- **Authentication:** Custom Role-Based Access Control (RBAC) with JWT & bcrypt
- **Payments:** Stripe Checkout
- **Deployment:** Vercel

---

## 🚀 Getting Started

### 1. Clone the repository
```bash
git clone https://github.com/furkancengiz6/GuestFlow.git
cd GuestFlow
```

### 2. Install Dependencies
```bash
npm install
```

### 3. Environment Variables
Create a `.env` file in the root directory:
```env
DATABASE_URL="file:./dev.db"
JWT_SECRET="your-secret-key"
```

### 4. Setup Database
```bash
npx prisma generate
npx prisma db push
```

### 5. Run the Development Server
```bash
npm run dev
```
Navigate to [http://localhost:3000](http://localhost:3000) to view the platform.

---

## 🎭 Test Accounts

You can test the platform using the following pre-configured accounts:

- **Host Account:** `host@voy.com` / `test1234`
- **Admin Account:** `admin@voy.com` / `test1234`

*(No account is required to test the Guest/Booking flow).*

---

*Designed and engineered for the ultimate maritime luxury experience.*
