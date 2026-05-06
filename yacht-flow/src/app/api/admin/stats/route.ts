import { NextResponse } from "next/server";
import { prisma } from "@/lib/prisma";

export async function GET() {
  try {
    const allBookings = await prisma.booking.findMany({
      include: {
        yacht: true,
      },
      orderBy: {
        createdAt: 'desc'
      }
    });

    const confirmedBookings = allBookings.filter(b => b.status === "CONFIRMED");
    const totalYachts = await prisma.yacht.count();

    const stats = {
      totalGMV: confirmedBookings.reduce((acc, b) => acc + b.totalPrice, 0),
      activeCharters: confirmedBookings.length,
      fleetCount: totalYachts,
      commission: confirmedBookings.reduce((acc, b) => acc + b.totalPrice, 0) * 0.15, // 15% platform fee
    };

    return NextResponse.json({ bookings: allBookings.slice(0, 5), stats });
  } catch (error: any) {
    console.error("Admin Stats Error:", error);
    return NextResponse.json({ error: error.message }, { status: 500 });
  }
}
