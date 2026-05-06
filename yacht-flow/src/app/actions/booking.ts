"use server";

import { prisma } from "@/lib/prisma";
import { revalidatePath } from "next/cache";
import { getServerSession } from "next-auth";
import { authOptions } from "@/lib/auth";

export async function createBooking(formData: FormData) {
  const session = await getServerSession(authOptions);

  if (!session?.user) {
    return { error: "Please login to book." };
  }

  const yachtId = formData.get("yachtId") as string;
  const startDateStr = formData.get("startDate") as string;
  const endDateStr = formData.get("endDate") as string;
  
  if (!yachtId || !startDateStr || !endDateStr) {
    return { error: "Missing required fields." };
  }

  const startDate = new Date(startDateStr);
  const endDate = new Date(endDateStr);
  
  // Calculate days
  const diffTime = Math.abs(endDate.getTime() - startDate.getTime());
  const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24)) || 1;

  // Fetch yacht to get price
  const yacht = await prisma.yacht.findUnique({ where: { id: yachtId } });
  if (!yacht) return { error: "Yacht not found." };

  const totalPrice = yacht.pricePerDay * diffDays;

  await prisma.booking.create({
    data: {
      yachtId,
      guestId: session.user.id,
      startDate,
      endDate,
      totalPrice,
      status: "PENDING"
    }
  });

  revalidatePath("/dashboard");
  return { success: true };
}
