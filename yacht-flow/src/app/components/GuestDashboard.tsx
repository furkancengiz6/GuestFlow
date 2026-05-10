"use client";

import { useState, useEffect } from "react";
import Image from "next/image";
import Link from "next/link";
import { useLanguage } from "@/locales/LanguageContext";

interface GuestDashboardProps {
  user: any;
}

export default function GuestDashboard({ user }: GuestDashboardProps) {
  const { t, lang } = useLanguage();
  const [activeTab, setActiveTab] = useState("bookings");
  const [mounted, setMounted] = useState(false);

  useEffect(() => {
    setMounted(true);
  }, []);

  const bookings = user.bookings || [];
  
  const upcomingBookings = bookings.filter((b: any) => new Date(b.startDate) > new Date());
  const pastBookings = bookings.filter((b: any) => new Date(b.startDate) <= new Date());

  const stats = [
    { label: t.guestDashboard.stats.total, value: bookings.length, icon: "🌊" },
    { label: t.guestDashboard.stats.upcoming, value: upcomingBookings.length, icon: "🗓️" },
    { label: t.guestDashboard.stats.status, value: "Platinum", icon: "💎" },
    { label: t.guestDashboard.stats.points, value: "2,450", icon: "✨" },
  ];

  return (
    <main className="min-h-screen bg-background pt-32 pb-24 px-6 md:px-12">
      <div className="max-w-7xl mx-auto">
        {/* Header */}
        <div className="flex flex-col md:flex-row justify-between items-start md:items-end gap-8 mb-16 animate-reveal">
          <div>
            <div className="text-gold tracking-[0.4em] uppercase text-xs mb-4 font-bold">{t.guestDashboard.tagline}</div>
            <h1 className="font-serif text-5xl md:text-7xl mb-4 italic">
              {t.guestDashboard.title.split(' ')[0]} <span className="text-gold not-italic">{t.guestDashboard.title.split(' ').slice(1).join(' ')}</span>
            </h1>
            <p className="text-foreground/50 font-light flex items-center gap-3 text-xl">
              <span className="w-8 h-[1px] bg-gold"></span>
              {t.guestDashboard.welcome.replace('{name}', user.name || "Elite Guest")}
            </p>
          </div>
          
          <Link href="/fleet">
            <button className="px-10 py-5 bg-gold text-background rounded-full text-xs tracking-widest uppercase font-bold shadow-2xl hover:scale-105 transition-transform">
              {t.guestDashboard.explore}
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
            onClick={() => setActiveTab("bookings")}
            className={`text-[10px] tracking-[0.3em] uppercase font-bold transition-all pb-4 relative ${activeTab === 'bookings' ? 'text-gold' : 'text-foreground/40 hover:text-white'}`}
          >
            {t.guestDashboard.tabs.bookings}
            {activeTab === 'bookings' && <div className="absolute bottom-0 left-0 w-full h-[1px] bg-gold"></div>}
          </button>
          <button 
            onClick={() => setActiveTab("favorites")}
            className={`text-[10px] tracking-[0.3em] uppercase font-bold transition-all pb-4 relative ${activeTab === 'favorites' ? 'text-gold' : 'text-foreground/40 hover:text-white'}`}
          >
            {t.guestDashboard.tabs.favorites}
            {activeTab === 'favorites' && <div className="absolute bottom-0 left-0 w-full h-[1px] bg-gold"></div>}
          </button>
        </div>

        {activeTab === "bookings" ? (
          <div className="animate-reveal">
            {bookings.length > 0 ? (
              <div className="grid gap-12">
                {/* Upcoming Section */}
                {upcomingBookings.length > 0 && (
                  <div>
                    <h2 className="font-serif text-3xl mb-8 flex items-center gap-4">
                      {t.guestDashboard.sections.upcoming.split(' ')[0]} <span className="text-gold italic">{t.guestDashboard.sections.upcoming.split(' ').slice(1).join(' ')}</span>
                      <span className="h-[1px] flex-1 bg-surface-border/20"></span>
                    </h2>
                    <div className="grid gap-8">
                      {upcomingBookings.map((booking: any) => (
                        <BookingCard key={booking.id} booking={booking} />
                      ))}
                    </div>
                  </div>
                )}

                {/* Past Section */}
                {pastBookings.length > 0 && (
                  <div>
                    <h2 className="font-serif text-3xl mb-8 flex items-center gap-4 text-foreground/40">
                      {t.guestDashboard.sections.past.split(' ')[0]} <span className="italic">{t.guestDashboard.sections.past.split(' ').slice(1).join(' ')}</span>
                      <span className="h-[1px] flex-1 bg-surface-border/20"></span>
                    </h2>
                    <div className="grid gap-8 opacity-60 grayscale hover:grayscale-0 hover:opacity-100 transition-all duration-700">
                      {pastBookings.map((booking: any) => (
                        <BookingCard key={booking.id} booking={booking} />
                      ))}
                    </div>
                  </div>
                )}
              </div>
            ) : (
              <div className="glass p-24 rounded-[4rem] text-center border-dashed border-gold/10">
                <div className="text-6xl mb-8 opacity-20">⚓</div>
                <h3 className="text-3xl font-serif text-white mb-4">{t.guestDashboard.empty.title}</h3>
                <p className="text-foreground/30 font-light mb-10 max-w-sm mx-auto text-lg italic">
                  {t.guestDashboard.empty.desc}
                </p>
                <Link href="/fleet" className="bg-gold text-background px-12 py-5 rounded-full text-[10px] tracking-widest uppercase font-bold hover:scale-105 transition-transform inline-block">
                  {t.guestDashboard.empty.button}
                </Link>
              </div>
            )}
          </div>
        ) : (
          <div className="glass p-24 rounded-[4rem] text-center">
            <div className="text-4xl mb-6 opacity-20">⭐</div>
            <p className="text-foreground/30 font-light italic text-xl">{t.guestDashboard.wishlistEmpty}</p>
          </div>
        )}
      </div>
    </main>
  );
}

function BookingCard({ booking }: { booking: any }) {
  const { t, lang } = useLanguage();
  const [mounted, setMounted] = useState(false);

  useEffect(() => {
    setMounted(true);
  }, []);

  const yacht = booking.yacht;
  if (!yacht) return null;

  return (
    <div className="glass p-6 rounded-[3rem] flex flex-col md:flex-row gap-8 items-center group hover:border-gold/30 transition-all duration-700">
      <div className="relative w-full md:w-72 aspect-video md:aspect-square rounded-[2.5rem] overflow-hidden">
        <Image src={yacht.imageUrl || "/placeholder-yacht.png"} alt={yacht.name} fill sizes="(max-width: 768px) 100vw, 288px" className="object-cover group-hover:scale-110 transition-transform duration-1000" />
      </div>
      
      <div className="flex-1 space-y-6">
        <div className="flex justify-between items-start">
          <div>
            <h3 className="text-4xl font-serif text-white mb-2">{yacht.name}</h3>
            <div className="flex items-center gap-4">
              <span className={`px-4 py-1 rounded-full text-[9px] tracking-widest uppercase font-bold ${booking.status === 'CONFIRMED' ? 'bg-green-500/10 text-green-400 border border-green-500/20' : 'bg-gold/10 text-gold border border-gold/20'}`}>
                {booking.status}
              </span>
              <span className="text-[10px] text-foreground/40 tracking-widest uppercase">{yacht.location}</span>
            </div>
          </div>
          <div className="text-right">
            <div className="text-[9px] tracking-widest uppercase text-gold/40 font-bold mb-1">{t.guestDashboard.card.id}</div>
            <div className="text-sm font-mono text-foreground/20 italic">#{booking.id.slice(-6).toUpperCase()}</div>
          </div>
        </div>
        
        <div className="grid grid-cols-3 gap-8 py-6 border-y border-surface-border/10">
          <div>
            <div className="text-[9px] tracking-widest uppercase text-foreground/30 mb-1">{t.guestDashboard.card.checkIn}</div>
            <div className="text-lg text-white">{mounted ? new Date(booking.startDate).toLocaleDateString(lang === 'tr' ? 'tr-TR' : 'en-US') : ''}</div>
          </div>
          <div>
            <div className="text-[9px] tracking-widest uppercase text-foreground/30 mb-1">{t.guestDashboard.card.checkOut}</div>
            <div className="text-lg text-white">{mounted ? new Date(booking.endDate).toLocaleDateString(lang === 'tr' ? 'tr-TR' : 'en-US') : ''}</div>
          </div>
          <div>
            <div className="text-[9px] tracking-widest uppercase text-foreground/30 mb-1">{t.guestDashboard.card.total}</div>
            <div className="text-lg text-white">
              €{mounted ? booking.totalPrice?.toLocaleString(lang === 'tr' ? 'tr-TR' : 'en-US') : booking.totalPrice}
            </div>
          </div>
        </div>
        
        <div className="pt-2 flex gap-4">
          <Link href={`/fleet/${yacht.id}`} className="flex-1 py-4 bg-white/5 border border-white/10 hover:border-gold rounded-2xl text-[10px] tracking-widest uppercase font-bold transition-all text-center">
            {t.guestDashboard.card.viewVessel}
          </Link>
          <Link href={`/manage/${booking.id}`} className="flex-1 py-4 bg-white/5 border border-white/10 hover:border-gold rounded-2xl text-[10px] tracking-widest uppercase font-bold transition-all text-center">
            {t.guestDashboard.card.itinerary}
          </Link>
        </div>
      </div>
    </div>
  );
}
