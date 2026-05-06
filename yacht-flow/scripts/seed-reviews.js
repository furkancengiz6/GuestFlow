const { PrismaClient } = require("@prisma/client");
const prisma = new PrismaClient();

async function main() {
  const yachts = await prisma.yacht.findMany();
  const guest = await prisma.user.findUnique({ where: { email: "guest@yachtflow.com" } });

  if (!guest) {
    console.log("Guest user not found. Please login as guest first.");
    return;
  }

  const reviewTemplates = [
    { rating: 5, comment: "An absolutely breathtaking experience. The crew was impeccable and the sunset views from the deck were unforgettable." },
    { rating: 5, comment: "Pure luxury from start to finish. The AI route suggestions led us to a hidden bay that was like paradise." },
    { rating: 4, comment: "Wonderful vessel and great service. The onboard chef prepared the best seafood we've ever had." },
    { rating: 5, comment: "Exceeded all expectations. The attention to detail in every corner of the yacht is remarkable." }
  ];

  for (const yacht of yachts) {
    // Create 2 random reviews for each yacht
    const shuffled = reviewTemplates.sort(() => 0.5 - Math.random());
    const selected = shuffled.slice(0, 2);

    for (const rev of selected) {
      await prisma.review.create({
        data: {
          rating: rev.rating,
          comment: rev.comment,
          yachtId: yacht.id,
          userId: guest.id,
        }
      });
    }
  }

  console.log("Seed successful: 2 reviews added to each yacht.");
}

main()
  .catch((e) => {
    console.error(e);
    process.exit(1);
  })
  .finally(async () => {
    await prisma.$disconnect();
  });
