const { PrismaClient } = require('@prisma/client');
const bcrypt = require('bcryptjs');

const prisma = new PrismaClient();

async function main() {
  const email = 'admin@yachtflow.com';
  const password = 'admin';
  const hashedPassword = await bcrypt.hash(password, 10);

  const adminUser = await prisma.user.upsert({
    where: { email: email },
    update: {
      password: hashedPassword,
      role: 'ADMIN',
      name: 'Admin User'
    },
    create: {
      email: email,
      name: 'Admin User',
      password: hashedPassword,
      role: 'ADMIN',
      isApproved: true,
    },
  });

  console.log('Admin user created or updated successfully:');
  console.log(`Email: ${adminUser.email}`);
  console.log(`Password: ${password}`);
  console.log(`Role: ${adminUser.role}`);
}

main()
  .catch((e) => {
    console.error(e);
    process.exit(1);
  })
  .finally(async () => {
    await prisma.$disconnect();
  });
