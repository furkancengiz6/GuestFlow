import { prisma } from "@/lib/prisma";
import { NextResponse } from "next/server";
import { getServerSession } from "next-auth";
import { authOptions } from "@/lib/auth";

export async function POST(req: Request) {
  try {
    const session = await getServerSession(authOptions);

    if (!session || (session.user.role !== "HOST" && session.user.role !== "ADMIN")) {
      return NextResponse.json({ error: "Unauthorized" }, { status: 401 });
    }

    const body = await req.json();
    const { 
      name, type, location, length, guests, cabins, crew, 
      pricePerDay, description, imageUrl, amenities 
    } = body;

    // Basic Validation
    if (!name || !type || !location || !pricePerDay || !imageUrl) {
      return NextResponse.json({ error: "Missing required fields" }, { status: 400 });
    }

    const yacht = await prisma.yacht.create({
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
        amenities: {
          connectOrCreate: (amenities ? amenities.split(',').map((a: string) => a.trim()) : []).map((name: string) => ({
            where: { name },
            create: { name }
          }))
        },
        hostId: session.user.id,
      },
    });

    return NextResponse.json({ 
      message: "Vessel listed successfully",
      yacht 
    }, { status: 201 });

  } catch (error: any) {
    console.error("Yacht creation error:", error);
    return NextResponse.json({ error: "Internal server error" }, { status: 500 });
  }
}
