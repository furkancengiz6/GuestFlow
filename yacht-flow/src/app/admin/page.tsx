import { prisma } from "@/lib/prisma";
import { getServerSession } from "next-auth";
import { authOptions } from "@/lib/auth";
import { redirect } from "next/navigation";
import AdminPageClient from "./AdminPageClient";

export default async function AdminPage() {
  const session = await getServerSession(authOptions);

  // Security: Server-side check
  if (!session || session.user.role !== "ADMIN") {
    redirect("/");
  }

  // Fetch pending hosts
  const pendingHosts = await prisma.user.findMany({
    where: {
      role: "HOST",
      isApproved: false,
    },
    orderBy: { createdAt: "desc" },
  });

  const formattedHosts = pendingHosts.map(h => ({
    id: h.id,
    name: h.name || "N/A",
    email: h.email || "N/A",
    companyName: h.companyName || "N/A",
    phoneNumber: h.phoneNumber || "N/A",
    createdAt: h.createdAt.toLocaleDateString()
  }));

  return <AdminPageClient pendingHosts={formattedHosts} />;
}
