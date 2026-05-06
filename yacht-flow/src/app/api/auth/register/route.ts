import { prisma } from "@/lib/prisma";
import { NextResponse } from "next/server";
import bcrypt from "bcryptjs";

export async function POST(req: Request) {
  try {
    const { name, email, password, role, companyName, phoneNumber } = await req.json();
    console.log("Registering user:", email, "Role:", role);

    if (!email || !password || !name) {
      return NextResponse.json({ error: "Missing required fields" }, { status: 400 });
    }

    // Check if user exists
    const existingUser = await prisma.user.findUnique({
      where: { email },
    });

    if (existingUser) {
      console.log("User already exists:", email);
      return NextResponse.json({ error: "User already exists" }, { status: 400 });
    }

    // Hash password
    console.log("Hashing password...");
    const hashedPassword = await bcrypt.hash(password, 12);

    // Create user
    console.log("Creating user in DB...");
    const user = await prisma.user.create({
      data: {
        name,
        email,
        password: hashedPassword,
        role: role || "GUEST",
        companyName: role === "HOST" ? companyName : null,
        phoneNumber,
        isApproved: role === "HOST" ? false : true,
      },
    });

    console.log("User created successfully:", user.id);
    return NextResponse.json({ 
      message: "User created successfully",
      user: { id: user.id, email: user.email, role: user.role } 
    }, { status: 201 });

  } catch (error: any) {
    console.error("DETAILED REGISTRATION ERROR:", error);
    return NextResponse.json({ error: error.message || "Internal server error" }, { status: 500 });
  }
}
