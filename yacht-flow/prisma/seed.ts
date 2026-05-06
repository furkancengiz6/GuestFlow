import { PrismaClient } from '@prisma/client'
const prisma = new PrismaClient()

async function main() {
  // Clear existing data
  await prisma.booking.deleteMany()
  await prisma.yacht.deleteMany()
  await prisma.user.deleteMany()

  // Create Users
  const guest = await prisma.user.create({
    data: {
      email: 'guest@yachtflow.com',
      name: 'Exclusive Guest',
      password: 'password123',
      role: 'GUEST'
    }
  });

  const host = await prisma.user.create({
    data: {
      email: 'host@yachtflow.com',
      name: 'Capt. Mehmet',
      password: 'password123',
      role: 'HOST'
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
      description: "A masterpiece of modern engineering. Experience the turquoise waters of Bodrum like never before.",
      imageUrl: "/yacht-1.png",
      hostId: host.id,
      amenities: {
        connectOrCreate: ["Jacuzzi", "Helipad", "Jet-Ski", "Cinema Room"].map(name => ({
          where: { name }, create: { name }
        }))
      }
    },
    {
      name: "Aegean Dream",
      type: "Luxury Gulet",
      location: "Göcek",
      length: "35m",
      guests: 10,
      cabins: 5,
      crew: 4,
      pricePerDay: 2800,
      description: "Classic Turkish wooden yacht with ultra-modern interiors. Perfect for family voyages in Göcek.",
      imageUrl: "/yacht-2.png",
      hostId: host.id,
      amenities: {
        connectOrCreate: ["Sun Deck", "Wi-Fi", "Paddleboard", "Fishing Gear"].map(name => ({
          where: { name }, create: { name }
        }))
      }
    },
    {
      name: "Oceanic Pearl",
      type: "Catamaran",
      location: "Marmaris",
      length: "24m",
      guests: 8,
      cabins: 4,
      crew: 3,
      pricePerDay: 1950,
      description: "Stable, spacious and fast. Explore the hidden bays of Marmaris with maximum comfort.",
      imageUrl: "/hero-bg.png",
      hostId: host.id,
      amenities: {
        connectOrCreate: ["Trampoline", "BBQ", "Snorkeling Gear", "Underwater Lights"].map(name => ({
          where: { name }, create: { name }
        }))
      }
    }
  ]

  for (const yacht of yachts) {
    await prisma.yacht.create({
      data: yacht
    })
  }

  console.log('Seed completed successfully.')
}

main()
  .catch((e) => {
    console.error(e)
    process.exit(1)
  })
  .finally(async () => {
    await prisma.$disconnect()
  })
