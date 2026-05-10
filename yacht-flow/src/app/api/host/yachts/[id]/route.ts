import { NextResponse } from "next/server";
import { getServerSession } from "next-auth";
import { authOptions } from "@/lib/auth";
import { prisma } from "@/lib/prisma";

export async function GET(req: Request, { params }: { params: Promise<{ id: string }> }) {
  try {
    const { id } = await params;
    const session = await getServerSession(authOptions);

    if (!session || session.user.role !== "HOST") {
      return NextResponse.json({ error: "Unauthorized" }, { status: 401 });
    }

    const yacht = await prisma.yacht.findUnique({
      where: { id },
      include: { amenities: true }
    });

    if (!yacht || yacht.hostId !== session.user.id) {
      return NextResponse.json({ error: "Not found or unauthorized" }, { status: 404 });
    }

    return NextResponse.json(yacht);
  } catch (error: any) {
    console.error("Fetch yacht error:", error);
    return NextResponse.json({ error: "Internal Server Error" }, { status: 500 });
  }
}

export async function PUT(req: Request, { params }: { params: Promise<{ id: string }> }) {
  try {
    const { id } = await params;
    const session = await getServerSession(authOptions);

    if (!session || session.user.role !== "HOST") {
      return NextResponse.json({ error: "Unauthorized" }, { status: 401 });
    }

    // Verify ownership
    const existing = await prisma.yacht.findUnique({
      where: { id }
    });

    if (!existing || existing.hostId !== session.user.id) {
      return NextResponse.json({ error: "Not found or unauthorized" }, { status: 404 });
    }

    const body = await req.json();
    const { name, type, location, length, guests, cabins, crew, pricePerDay, description, imageUrl, icalUrl, amenities } = body;

    const updatedYacht = await prisma.yacht.update({
      where: { id },
      data: {
        name,
        type,
        location,
        length,
        guests: parseInt(guests),
        cabins: parseInt(cabins),
        crew: parseInt(crew),
        pricePerDay: parseFloat(pricePerDay),
        description,
        imageUrl,
        icalUrl: icalUrl || null,
        amenities: {
          set: [], // Clear existing amenities first
          connectOrCreate: (typeof amenities === 'string' && amenities ? amenities.split(',').map((a: string) => a.trim()) : []).map((name: string) => ({
            where: { name },
            create: { name }
          }))
        },
      }
    });

    return NextResponse.json(updatedYacht);
  } catch (error: any) {
    console.error("Update yacht error:", error);
    return NextResponse.json({ error: "Internal Server Error" }, { status: 500 });
  }
}
