import { prisma } from "@/lib/prisma";
import { NextResponse } from "next/server";
import { getServerSession } from "next-auth";
import { authOptions } from "@/lib/auth";

export async function POST(req: Request) {
  try {
    const session = await getServerSession(authOptions);

    // Only Admins can approve
    if (!session || session.user.role !== "ADMIN") {
      return NextResponse.json({ error: "Unauthorized" }, { status: 401 });
    }

    const { userId } = await req.json();

    if (!userId) {
      return NextResponse.json({ error: "Missing User ID" }, { status: 400 });
    }

    const updatedUser = await prisma.user.update({
      where: { id: userId },
      data: { isApproved: true },
    });

    return NextResponse.json({ 
      message: "Host approved successfully",
      user: updatedUser 
    });

  } catch (error: any) {
    console.error("Approval error:", error);
    return NextResponse.json({ error: "Internal server error" }, { status: 500 });
  }
}
