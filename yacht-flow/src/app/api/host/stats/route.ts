import { NextResponse } from "next/server";
import { prisma } from "@/lib/prisma";
import { getServerSession } from "next-auth";
import { authOptions } from "@/lib/auth";

export async function GET() {
  try {
    const session = await getServerSession(authOptions);

    if (!session) {
      return NextResponse.json({ error: "Unauthorized" }, { status: 401 });
    }

    const hostId = session.user.id;

    // Fetch ONLY yachts belonging to this host
    const yachts = await prisma.yacht.findMany({
      where: { hostId: hostId },
      include: {
        bookings: true,
        reviews: {
          include: { user: true },
          orderBy: { createdAt: 'desc' }
        }
      },
    });

    // Calculate real stats
    const totalRevenue = yachts.reduce((acc, y) => 
      acc + y.bookings.reduce((bAcc, b) => bAcc + (b.status === "CONFIRMED" ? b.totalPrice : 0), 0)
    , 0);

    const activeBookings = yachts.reduce((acc, y) => 
      acc + y.bookings.filter(b => b.status === "CONFIRMED" || b.status === "PENDING").length
    , 0);

    // Calculate average rating across all yachts
    const allReviews = yachts.flatMap(y => y.reviews);
    const avgRating = allReviews.length > 0 
      ? (allReviews.reduce((acc, r) => acc + r.rating, 0) / allReviews.length).toFixed(1)
      : "5.0";

    const stats = {
      totalRevenue,
      activeBookings,
      fleetCount: yachts.length,
      rating: avgRating,
    };

    return NextResponse.json({ yachts, stats });
  } catch (error: any) {
    console.error("Host Stats Error:", error);
    return NextResponse.json({ error: error.message }, { status: 500 });
  }
}
