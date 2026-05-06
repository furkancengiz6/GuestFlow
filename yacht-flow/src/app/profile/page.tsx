import { getServerSession } from "next-auth";
import { authOptions } from "@/lib/auth";
import { prisma } from "@/lib/prisma";
import { redirect } from "next/navigation";
import ProfileClient from "./ProfileClient";

export default async function ProfilePage() {
  const session = await getServerSession(authOptions);

  if (!session || !session.user) {
    redirect("/login");
  }

  const user = await prisma.user.findUnique({
    where: { email: session.user.email as string },
    include: {
      bookings: {
        include: {
          yacht: true,
        },
        orderBy: {
          createdAt: "desc",
        },
        take: 3,
      },
    },
  });

  if (!user) {
    redirect("/login");
  }

  return <ProfileClient user={user as any} />;
}
