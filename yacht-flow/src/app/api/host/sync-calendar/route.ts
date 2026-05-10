import { NextResponse } from "next/server";
import { getServerSession } from "next-auth";
import { authOptions } from "@/lib/auth";
import { prisma } from "@/lib/prisma";
import { syncYachtCalendar } from "@/lib/calendar";

export const dynamic = 'force-dynamic';

export async function POST(req: Request) {
  try {
    const session = await getServerSession(authOptions);

    if (!session?.user || session.user.role !== "HOST") {
      return NextResponse.json({ error: "Unauthorized" }, { status: 401 });
    }

    const { yachtId } = await req.json();

    if (!yachtId) {
      return NextResponse.json({ error: "Missing Yacht ID" }, { status: 400 });
    }

    // Verify ownership
    const yacht = await prisma.yacht.findUnique({
      where: { id: yachtId },
    });

    if (!yacht || yacht.hostId !== session.user.id) {
      return NextResponse.json({ error: "Unauthorized" }, { status: 401 });
    }

    if (!yacht.icalUrl) {
      return NextResponse.json({ error: "No external calendar URL configured" }, { status: 400 });
    }

    await syncYachtCalendar(yachtId);

    return NextResponse.json({ message: "Calendar synced successfully" });
  } catch (error: any) {
    console.error("Sync error:", error);
    return NextResponse.json({ error: "Internal server error" }, { status: 500 });
  }
}
