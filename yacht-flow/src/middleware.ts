import { NextResponse } from "next/server";
import type { NextRequest } from "next/server";

// Temporary Permissive Middleware to debug Vercel Config issues
export function middleware(request: NextRequest) {
  // Just let everything pass through for now to isolate the Auth Configuration issue
  return NextResponse.next();
}

export const config = {
  matcher: ["/admin/:path*", "/host/:path*", "/manage/:path*", "/dashboard/:path*"],
};
