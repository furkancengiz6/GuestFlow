import { prisma } from "@/lib/prisma";
import { notFound } from "next/navigation";
import YachtDetailClient from "./YachtDetailClient";

export default async function YachtDetail({ params }: { params: Promise<{ id: string }> }) {
  const { id } = await params;
  const yacht = await prisma.yacht.findUnique({
    where: { id },
    include: {
      amenities: true,
      reviews: {
        include: { user: true },
        orderBy: { createdAt: 'desc' }
      }
    }
  });

  if (!yacht) {
    notFound();
  }

  return <YachtDetailClient yacht={yacht} />;
}
