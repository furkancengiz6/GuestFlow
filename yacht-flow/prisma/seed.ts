import { PrismaClient } from '@prisma/client'
import bcrypt from 'bcryptjs'

const prisma = new PrismaClient()

async function main() {
  // Clear existing data
  await prisma.review.deleteMany()
  await prisma.booking.deleteMany()
  await prisma.yacht.deleteMany()
  await prisma.user.deleteMany()
  await prisma.amenity.deleteMany()

  const hashedPassword = await bcrypt.hash('password123', 10)

  // Create Users
  const admin = await prisma.user.create({
    data: {
      email: 'admin@yachtflow.com',
      name: 'System Admin',
      password: hashedPassword,
      role: 'ADMIN',
      isApproved: true
    }
  });

  const guest = await prisma.user.create({
    data: {
      email: 'guest@yachtflow.com',
      name: 'Exclusive Guest',
      password: hashedPassword,
      role: 'GUEST'
    }
  });

  const host = await prisma.user.create({
    data: {
      email: 'host@yachtflow.com',
      name: 'Capt. Mehmet',
      password: hashedPassword,
      role: 'HOST',
      isApproved: true,
      companyName: 'Bodrum Luxury Charters'
    }
  });

  const yachts = [
    {
      name: "Aura of Bodrum",
      type: "Motor Yacht",
      location: "Bodrum",
      length: "42m",
      guests: 12,
      cabins: 6,
      crew: 8,
      pricePerDay: 4500,
      description: "A masterpiece of modern engineering. Experience the turquoise waters of Bodrum like never before with ultimate luxury and speed.",
      imageUrl: "/yacht-1.png",
      hostId: host.id,
      amenities: ["Jacuzzi", "Helipad", "Jet-Ski", "Cinema Room"]
    },
    {
      name: "Halikarnas Star",
      type: "Luxury Gulet",
      location: "Bodrum",
      length: "38m",
      guests: 12,
      cabins: 6,
      crew: 5,
      pricePerDay: 3200,
      description: "A traditional wooden gulet with a modern soul. Perfect for sunset cruises around the Bodrum peninsula.",
      imageUrl: "/yacht-2.png",
      hostId: host.id,
      amenities: ["Sun Deck", "Wi-Fi", "Paddleboard", "Fishing Gear"]
    },
    {
      name: "Bodrum Royal",
      type: "Motor Yacht",
      location: "Bodrum",
      length: "50m",
      guests: 14,
      cabins: 7,
      crew: 10,
      pricePerDay: 7500,
      description: "The flagship of our fleet. Offering unparalleled service and absolute privacy for the most discerning guests.",
      imageUrl: "/hero-bg.png",
      hostId: host.id,
      amenities: ["Swimming Pool", "Gym", "Private Chef", "Massage Room"]
    },
    {
      name: "Aegean Princess",
      type: "Sailing Yacht",
      location: "Bodrum",
      length: "30m",
      guests: 8,
      cabins: 4,
      crew: 4,
      pricePerDay: 2400,
      description: "Feel the wind and the sea. A classic sailing experience without compromising on modern comforts.",
      imageUrl: "/yacht-1.png",
      hostId: host.id,
      amenities: ["Snorkeling Gear", "Kayaks", "Deck Speakers", "Wine Cellar"]
    },
    {
      name: "Blue Voyage",
      type: "Luxury Gulet",
      location: "Bodrum",
      length: "35m",
      guests: 10,
      cabins: 5,
      crew: 5,
      pricePerDay: 2900,
      description: "Elegant lines and spacious decks. Designed for long, relaxing voyages through the Aegean's hidden gems.",
      imageUrl: "/yacht-2.png",
      hostId: host.id,
      amenities: ["Outdoor Cinema", "BBQ", "Sea Bobs", "Satellite TV"]
    },
    {
      name: "Starlight Bodrum",
      type: "Catamaran",
      location: "Bodrum",
      length: "25m",
      guests: 10,
      cabins: 5,
      crew: 3,
      pricePerDay: 2100,
      description: "Spacious and stable. Ideal for groups looking for a social atmosphere and easy access to shallow bays.",
      imageUrl: "/hero-bg.png",
      hostId: host.id,
      amenities: ["Trampoline", "Underwater Lights", "Water Maker", "Nespresso Machine"]
    },
    {
      name: "Yalikavak Legend",
      type: "Motor Yacht",
      location: "Bodrum",
      length: "45m",
      guests: 12,
      cabins: 6,
      crew: 9,
      pricePerDay: 5800,
      description: "Modern, chic, and powerful. Perfectly suited for the vibrant lifestyle of Yalikavak Marina.",
      imageUrl: "/yacht-1.png",
      hostId: host.id,
      amenities: ["Beach Club", "Infinity Pool", "Electric Surfboards", "DJ Booth"]
    },
    {
      name: "Paradise Bodrum",
      type: "Luxury Gulet",
      location: "Bodrum",
      length: "40m",
      guests: 12,
      cabins: 6,
      crew: 6,
      pricePerDay: 3500,
      description: "Your private floating villa. Experience the ultimate comfort with a dedicated crew catering to your every whim.",
      imageUrl: "/yacht-2.png",
      hostId: host.id,
      amenities: ["Air Conditioning", "Steam Room", "Tender Boat", "Wakeboard"]
    }
  ]

  for (const yachtData of yachts) {
    const { amenities, ...rest } = yachtData;
    await prisma.yacht.create({
      data: {
        ...rest,
        amenities: {
          connectOrCreate: amenities.map(name => ({
            where: { name },
            create: { name }
          }))
        }
      }
    })
  }

  console.log('Seed completed successfully with 8 Bodrum yachts and admin user.')
}

main()
  .catch((e) => {
    console.error(e)
    process.exit(1)
  })
  .finally(async () => {
    await prisma.$disconnect()
  })
