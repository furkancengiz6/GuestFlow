import { NextResponse } from "next/server";
import type { NextRequest } from "next/server";

// Permissive Proxy — passes all matched routes through
export function proxy(request: NextRequest) {
  return NextResponse.next();
}

export const config = {
  matcher: ["/admin/:path*", "/host/:path*", "/manage/:path*", "/dashboard/:path*"],
};
