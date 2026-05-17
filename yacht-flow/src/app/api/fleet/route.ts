import { prisma } from "@/lib/prisma";
import { NextRequest, NextResponse } from "next/server";

export const dynamic = "force-dynamic";

export async function GET(request: NextRequest) {
  try {
    const { searchParams } = request.nextUrl;
    const location = searchParams.get("location") || undefined;
    const type = searchParams.get("type") || undefined;
    const guests = searchParams.get("guests") || undefined;
    const sort = searchParams.get("sort") || undefined;

    const where: any = {};

    if (location) {
      where.location = { contains: location, mode: "insensitive" };
    }
    if (type) {
      where.type = { contains: type, mode: "insensitive" };
    }
    if (guests && !isNaN(parseInt(guests))) {
      where.guests = { gte: parseInt(guests) };
    }

    const orderBy: any = {};
    if (sort === "price-asc") orderBy.pricePerDay = "asc";
    else if (sort === "price-desc") orderBy.pricePerDay = "desc";
    else orderBy.createdAt = "desc";

    const yachts = await prisma.yacht.findMany({
      where,
      orderBy,
    });

    return NextResponse.json(yachts);
  } catch (error: any) {
    console.error("Fleet API error:", error?.message || error);
    return NextResponse.json([], { status: 200 });
  }
}
