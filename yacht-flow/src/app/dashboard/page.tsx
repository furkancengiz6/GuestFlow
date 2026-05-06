import { getServerSession } from "next-auth";
import { authOptions } from "@/lib/auth";
import { prisma } from "@/lib/prisma";
import { redirect } from "next/navigation";
import GuestDashboard from "../components/GuestDashboard";
import HostDashboard from "../components/HostDashboard";

export default async function DashboardPage() {
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
          startDate: "desc",
        }
      },
      yachts: {
        include: {
          bookings: {
            include: {
              guest: true
            },
            orderBy: {
              startDate: "desc"
            }
          }
        }
      }
    }
  });

  if (!user) {
    redirect("/login");
  }

  if (user.role === "HOST" || user.role === "ADMIN") {
    return <HostDashboard user={user} />;
  }

  return <GuestDashboard user={user} />;
}
