"use client";

import { useState, useEffect } from "react";
import Image from "next/image";
import Link from "next/link";
import { useLanguage } from "@/locales/LanguageContext";

interface HostDashboardProps {
  user: {
    id: string;
    name: string | null;
    rating?: string;
    yachts: any[];
  };
}

export default function HostDashboard({ user }: HostDashboardProps) {
  const { t, lang } = useLanguage();
  const [activeTab, setActiveTab] = useState("fleet");
  const [mounted, setMounted] = useState(false);

  useEffect(() => {
    setMounted(true);
  }, []);

  const yachts = user.yachts || [];
  
  // Calculate stats from the user object passed from server
  const totalRevenue = yachts.reduce((acc: number, yacht: any) => {
    const yachtRevenue = yacht.bookings?.reduce((bAcc: number, b: any) => bAcc + b.totalPrice, 0) || 0;
    return acc + yachtRevenue;
  }, 0);

  const activeBookings = yachts.reduce((acc: number, yacht: any) => {
    return acc + (yacht.bookings?.filter((b: any) => b.status === "CONFIRMED").length || 0);
  }, 0);

  const stats = [
    { label: t.host.dashboard.stats.revenue, value: `€${mounted ? totalRevenue.toLocaleString(lang === 'tr' ? 'tr-TR' : 'en-US') : totalRevenue}`, icon: "💰" },
    { label: t.host.dashboard.stats.active, value: activeBookings, icon: "⛵" },
    { label: t.host.dashboard.stats.vessels, value: yachts.length, icon: "🛥️" },
    { label: t.host.dashboard.stats.rating, value: user.rating || "5.0", icon: "⭐" },
  ];

  return (
    <main className="min-h-screen bg-background pt-32 pb-24 px-6 md:px-12">
      <div className="max-w-7xl mx-auto">
        {/* Header */}
        <div className="flex flex-col md:flex-row justify-between items-start md:items-end gap-8 mb-16 animate-reveal">
          <div>
            <div className="text-gold tracking-[0.4em] uppercase text-xs mb-4 font-bold">{t.host.dashboard.tagline}</div>
            <h1 className="font-serif text-5xl md:text-7xl mb-4 italic">
              {t.host.dashboard.title.split(' ')[0]} <span className="text-gold not-italic">{t.host.dashboard.title.split(' ').slice(1).join(' ')}</span>
            </h1>
            <p className="text-foreground/50 font-light flex items-center gap-3 text-xl">
              <span className="w-8 h-[1px] bg-gold"></span>
              {t.host.dashboard.welcome.replace('{name}', user.name || "Captain")}
            </p>
          </div>
          
          <Link href="/host/add-yacht">
            <button className="px-10 py-5 bg-gold text-background rounded-full text-xs tracking-widest uppercase font-bold shadow-2xl hover:scale-105 transition-transform">
              + {t.host.dashboard.listNew}
            </button>
          </Link>
        </div>

        {/* Stats Grid */}
        <div className="grid grid-cols-1 md:grid-cols-4 gap-8 mb-24 animate-reveal stagger-1">
          {stats.map((stat, i) => (
            <div key={i} className="glass p-8 rounded-[2rem] border border-gold/10 hover:border-gold/30 transition-all duration-500">
              <div className="text-3xl mb-4">{stat.icon}</div>
              <div className="text-[10px] tracking-[0.3em] uppercase text-foreground/40 font-bold mb-2">{stat.label}</div>
              <div className="text-3xl font-serif text-white">{stat.value}</div>
            </div>
          ))}
        </div>

        {/* Tab Switcher */}
        <div className="flex gap-12 mb-12 border-b border-surface-border/30 pb-4">
          <button 
            onClick={() => setActiveTab("fleet")}
            className={`text-[10px] tracking-[0.3em] uppercase font-bold transition-all pb-4 relative ${activeTab === 'fleet' ? 'text-gold' : 'text-foreground/40 hover:text-white'}`}
          >
            {t.host.dashboard.tabs.fleet}
            {activeTab === 'fleet' && <div className="absolute bottom-0 left-0 w-full h-[1px] bg-gold"></div>}
          </button>
          <button 
            onClick={() => setActiveTab("reviews")}
            className={`text-[10px] tracking-[0.3em] uppercase font-bold transition-all pb-4 relative ${activeTab === 'reviews' ? 'text-gold' : 'text-foreground/40 hover:text-white'}`}
          >
            {t.host.dashboard.tabs.reviews}
            {activeTab === 'reviews' && <div className="absolute bottom-0 left-0 w-full h-[1px] bg-gold"></div>}
          </button>
        </div>

        {activeTab === "fleet" ? (
          <div className="animate-reveal">
            <h2 className="font-serif text-3xl mb-12 flex items-center gap-4">
              {t.host.dashboard.fleet.title.split(' ')[0]} <span className="text-gold italic">{t.host.dashboard.fleet.title.split(' ').slice(1).join(' ')}</span>
              <span className="h-[1px] flex-1 bg-surface-border/20"></span>
            </h2>
            
            <div className="grid gap-8">
              {yachts.length > 0 ? yachts.map((yacht: any) => (
                <div key={yacht.id} className="glass p-6 rounded-[3rem] flex flex-col md:flex-row gap-8 items-center group hover:border-gold/30 transition-all duration-700">
                  <div className="relative w-full md:w-72 aspect-video md:aspect-square rounded-[2.5rem] overflow-hidden">
                    <Image src={yacht.imageUrl || "/placeholder-yacht.png"} alt={yacht.name} fill className="object-cover group-hover:scale-110 transition-transform duration-1000" />
                  </div>
                  
                  <div className="flex-1 space-y-6">
                    <div className="flex justify-between items-start">
                      <div>
                        <h3 className="text-4xl font-serif text-white mb-2">{yacht.name}</h3>
                        <div className="flex items-center gap-4">
                          <span className="px-4 py-1 rounded-full text-[9px] tracking-widest uppercase font-bold bg-green-500/10 text-green-400 border border-green-500/20">
                            {t.host.dashboard.fleet.active}
                          </span>
                          <span className="text-[10px] text-foreground/40 tracking-widest uppercase">{yacht.location} • {yacht.type}</span>
                        </div>
                      </div>
                      <div className="text-right">
                        <div className="text-[9px] tracking-widest uppercase text-gold/40 font-bold mb-1">{t.host.dashboard.fleet.fleetId}</div>
                        <div className="text-sm font-mono text-foreground/20 italic">#{yacht.id.slice(-6).toUpperCase()}</div>
                      </div>
                    </div>
                    
                    <div className="grid grid-cols-3 gap-8 py-6 border-y border-surface-border/10">
                      <div>
                        <div className="text-[9px] tracking-widest uppercase text-foreground/30 mb-1">{t.host.dashboard.fleet.capacity}</div>
                        <div className="text-lg text-white">{yacht.guests} {t.host.dashboard.fleet.guests}</div>
                      </div>
                      <div>
                        <div className="text-[9px] tracking-widest uppercase text-foreground/30 mb-1">{t.host.dashboard.fleet.basePrice}</div>
                        <div className="text-lg text-white">€{yacht.pricePerDay}</div>
                      </div>
                      <div>
                        <div className="text-[9px] tracking-widest uppercase text-foreground/30 mb-1">{t.host.dashboard.fleet.charters}</div>
                        <div className="text-lg text-white">{yacht.bookings?.length || 0} {t.host.dashboard.fleet.trips}</div>
                      </div>
                    </div>
                    
                    <div className="pt-2 flex gap-4">
                      <Link href={`/fleet/${yacht.id}`} className="flex-1 py-4 bg-white/5 border border-white/10 hover:border-gold rounded-2xl text-[10px] tracking-widest uppercase font-bold transition-all text-center">
                        {t.host.dashboard.fleet.viewListing}
                      </Link>
                      <button className="flex-1 py-4 bg-white/5 border border-white/10 hover:border-gold rounded-2xl text-[10px] tracking-widest uppercase font-bold transition-all">
                        {t.host.dashboard.fleet.editVessel}
                      </button>
                    </div>
                  </div>
                </div>
              )) : (
                <div className="glass p-24 rounded-[4rem] text-center border-dashed border-gold/10">
                  <div className="text-6xl mb-8 opacity-20">🛳️</div>
                  <h3 className="text-3xl font-serif text-white mb-4">{t.host.dashboard.fleet.noVessels}</h3>
                  <p className="text-foreground/30 font-light mb-10 max-w-sm mx-auto text-lg italic">
                    {t.host.dashboard.fleet.noVesselsDesc}
                  </p>
                  <Link href="/host/add-yacht" className="bg-gold text-background px-12 py-5 rounded-full text-[10px] tracking-widest uppercase font-bold hover:scale-105 transition-transform inline-block">
                    {t.host.dashboard.fleet.addFirst}
                  </Link>
                </div>
              )}
            </div>
          </div>
        ) : (
          <div className="space-y-12 animate-reveal">
            <h2 className="font-serif text-3xl mb-12 flex items-center gap-4">
              {t.host.dashboard.reviews.title.split(' ')[0]} <span className="text-gold italic">{t.host.dashboard.reviews.title.split(' ').slice(1).join(' ')}</span>
              <span className="h-[1px] flex-1 bg-surface-border/20"></span>
            </h2>

            <div className="grid grid-cols-1 md:grid-cols-2 gap-8">
              {yachts.flatMap((y: any) => (y.reviews || []).map((r: any) => ({ ...r, yachtName: y.name }))).length > 0 ? (
                yachts.flatMap((y: any) => (y.reviews || []).map((r: any) => ({ ...r, yachtName: y.name }))).map((review: any) => (
                  <div key={review.id} className="glass p-8 rounded-[3rem] border border-gold/10 relative overflow-hidden group">
                    <div className="absolute top-0 right-0 p-8 opacity-5 text-4xl group-hover:opacity-10 transition-opacity">❝</div>
                    <div className="flex items-center gap-4 mb-6">
                      <div className="w-12 h-12 rounded-full bg-gold/10 flex items-center justify-center text-gold font-serif border border-gold/20 text-xl">
                        {review.user?.name?.[0] || "G"}
                      </div>
                      <div>
                        <div className="text-xs font-bold text-white uppercase tracking-widest">{review.user?.name || "Elite Guest"}</div>
                        <div className="text-[9px] text-gold uppercase tracking-[0.2em]">{review.yachtName}</div>
                      </div>
                    </div>
                    <p className="text-base text-foreground/60 italic font-light leading-relaxed mb-8">"{review.comment}"</p>
                    <div className="flex justify-between items-center pt-6 border-t border-surface-border/20">
                      <div className="text-[10px] text-foreground/30 uppercase tracking-widest">{mounted ? new Date(review.createdAt).toLocaleDateString(lang === 'tr' ? 'tr-TR' : 'en-US') : ''}</div>
                      <button className="text-[10px] text-gold uppercase tracking-widest hover:tracking-[0.2em] transition-all font-bold">{t.host.dashboard.reviews.reply}</button>
                    </div>
                  </div>
                ))
              ) : (
                <div className="col-span-2 glass p-24 rounded-[4rem] text-center">
                  <div className="text-4xl mb-6 opacity-20">💬</div>
                  <p className="text-foreground/30 font-light italic">{t.host.dashboard.reviews.noReviews}</p>
                </div>
              )}
            </div>
          </div>
        )}
      </div>
    </main>
  );
}
