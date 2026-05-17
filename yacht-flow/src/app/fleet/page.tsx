import { prisma } from "@/lib/prisma";
import FleetClient from "./FleetClient";
import { Suspense } from "react";

export const dynamic = "force-dynamic";

export default async function FleetPage({
  searchParams,
}: {
  searchParams?: Promise<{ 
    location?: string;
    type?: string;
    guests?: string;
    sort?: string;
  }> | any;
}) {
  let location: string | undefined = undefined;
  let type: string | undefined = undefined;
  let guests: string | undefined = undefined;
  let sort: string | undefined = undefined;
  let yachts: any[] = [];
  
  try {
    const resolved = searchParams ? await searchParams : {};
    if (resolved) {
      location = resolved.location;
      type = resolved.type;
      guests = resolved.guests;
      sort = resolved.sort;
    }
  } catch (err) {
    console.error("Error resolving searchParams:", err);
  }
  
  try {
    const where: any = {};
    
    if (location) {
      where.location = { contains: location, mode: 'insensitive' };
    }
    if (type) {
      where.type = { contains: type, mode: 'insensitive' };
    }
    if (guests && !isNaN(parseInt(guests))) {
      where.guests = { gte: parseInt(guests) };
    }

    const orderBy: any = {};
    if (sort === "price-asc") orderBy.pricePerDay = 'asc';
    else if (sort === "price-desc") orderBy.pricePerDay = 'desc';
    else orderBy.createdAt = 'desc';

    yachts = await prisma.yacht.findMany({
      where,
      orderBy
    });
  } catch (error: any) {
    console.error("Database fetch error:", error);
  }

  return (
    <main className="min-h-screen bg-background pt-32 pb-12 px-6 max-w-7xl mx-auto">
      <Suspense fallback={<div className="min-h-screen bg-background flex items-center justify-center text-gold">Loading Collection...</div>}>
        <FleetClient yachts={yachts} />
      </Suspense>
    </main>
  );
}
