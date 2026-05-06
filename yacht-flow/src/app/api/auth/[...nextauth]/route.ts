import NextAuth from "next-auth";
import { authOptions } from "@/lib/auth";

// Vercel Environment Variable Force-Injection Hack
if (!process.env.NEXTAUTH_URL) {
  process.env.NEXTAUTH_URL = process.env.VERCEL_URL 
    ? `https://${process.env.VERCEL_URL}` 
    : "https://yacht-flow.vercel.app";
}

if (!process.env.NEXTAUTH_SECRET) {
  process.env.NEXTAUTH_SECRET = "voy-luxury-secret-fallback-999";
}

const handler = NextAuth(authOptions);

export { handler as GET, handler as POST };
