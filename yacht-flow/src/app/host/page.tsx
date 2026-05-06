"use client";

import { useState, useEffect } from "react";
import LuxuryLoading from "@/app/components/LuxuryLoading";
import HostDashboard from "@/app/components/HostDashboard";
import Link from "next/link";

export default function HostPage() {
  const [data, setData] = useState<any>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    fetch("/api/host/stats")
      .then(res => res.json())
      .then(data => setData(data))
      .catch(err => console.error(err))
      .finally(() => setLoading(false));
  }, []);

  if (loading) return <LuxuryLoading />;
  
  if (!data || !data.yachts) return (
    <div className="min-h-screen bg-background flex flex-col items-center justify-center p-6 text-center">
      <div className="text-6xl mb-8">🗝️</div>
      <h2 className="text-3xl font-serif text-white mb-4">Access Denied</h2>
      <p className="text-foreground/40 font-light mb-8 max-w-sm mx-auto uppercase tracking-widest text-[10px]">
        This area is restricted to authorized VOY. partners.
      </p>
      <Link href="/login" className="text-gold font-bold tracking-widest uppercase text-[10px] border-b border-gold/30 pb-2 hover:text-white transition-colors">
        Try Re-authenticating →
      </Link>
    </div>
  );

  // Mock a user object structure that HostDashboard expects
  const user = {
    id: "host-1",
    name: "Fleet Partner",
    yachts: data.yachts,
    rating: data.stats?.rating || "5.0"
  };

  return <HostDashboard user={user} />;
}
