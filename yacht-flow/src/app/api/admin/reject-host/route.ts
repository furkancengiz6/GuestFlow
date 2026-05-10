import { prisma } from "@/lib/prisma";
import { NextResponse } from "next/server";
import { getServerSession } from "next-auth";
import { authOptions } from "@/lib/auth";

export async function POST(req: Request) {
  try {
    const session = await getServerSession(authOptions);

    // Only Admins can reject
    if (!session || session.user.role !== "ADMIN") {
      return NextResponse.json({ error: "Unauthorized" }, { status: 401 });
    }

    const { userId } = await req.json();

    if (!userId) {
      return NextResponse.json({ error: "Missing User ID" }, { status: 400 });
    }

    // In a real scenario, you might want to send an email or keep the user record with a 'REJECTED' status.
    // For now, we will delete the pending request.
    await prisma.user.delete({
      where: { id: userId },
    });

    return NextResponse.json({ 
      message: "Host application rejected and removed successfully"
    });

  } catch (error: any) {
    console.error("Rejection error:", error);
    return NextResponse.json({ error: "Internal server error" }, { status: 500 });
  }
}
