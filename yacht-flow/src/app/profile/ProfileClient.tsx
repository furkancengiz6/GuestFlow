"use client";

import Link from "next/link";
import { useLanguage } from "@/locales/LanguageContext";

interface ProfileClientProps {
  user: {
    id: string;
    name: string | null;
    email: string | null;
    image: string | null;
    role: string;
    phoneNumber: string | null;
    companyName: string | null;
    createdAt: Date;
    bookings: any[];
  };
}

export default function ProfileClient({ user }: ProfileClientProps) {
  const { t } = useLanguage();

  return (
    <main className="min-h-screen bg-background pt-32 pb-24 px-6 md:px-12">
      <div className="max-w-4xl mx-auto">
        <div className="flex flex-col md:flex-row justify-between items-start md:items-end gap-8 mb-16 animate-reveal">
          <div>
            <div className="text-gold tracking-[0.4em] uppercase text-xs mb-4 font-bold">{t.profile.tagline}</div>
            <h1 className="font-serif text-6xl md:text-8xl mb-4 tracking-tighter">
              {t.profile.title.split(' ')[0]} <span className="text-gold italic">{t.profile.title.split(' ').slice(1).join(' ')}</span>
            </h1>
            <p className="text-foreground/40 font-light text-xl">{t.profile.subtitle}</p>
          </div>
        </div>

        <div className="grid grid-cols-1 lg:grid-cols-3 gap-12">
          {/* Sidebar / Info */}
          <div className="lg:col-span-1 space-y-8">
            <div className="glass p-10 rounded-[3rem] text-center">
              <div className="w-32 h-32 rounded-full bg-gold/10 mx-auto mb-6 flex items-center justify-center text-gold border border-gold/20 overflow-hidden">
                {user.image ? (
                  <img src={user.image} alt={user.name || ""} className="w-full h-full object-cover" />
                ) : (
                  <span className="text-4xl font-serif">{user.name?.charAt(0) || "U"}</span>
                )}
              </div>
              <h2 className="text-2xl font-serif text-white mb-2">{user.name}</h2>
              <div className="text-[10px] tracking-[0.3em] uppercase text-gold font-bold bg-gold/10 px-4 py-1.5 rounded-full inline-block border border-gold/20">
                {user.role}
              </div>
              <div className="mt-8 pt-8 border-t border-surface-border/30 space-y-4">
                <div className="flex justify-between text-left">
                  <span className="text-[10px] uppercase tracking-widest text-foreground/40">{t.profile.memberSince}</span>
                  <span className="text-[10px] uppercase tracking-widest text-white">{new Date(user.createdAt).getFullYear()}</span>
                </div>
                <div className="flex justify-between text-left">
                  <span className="text-[10px] uppercase tracking-widest text-foreground/40">{t.profile.status}</span>
                  <span className="text-[10px] uppercase tracking-widest text-green-400">{t.profile.verified}</span>
                </div>
              </div>
            </div>

            <div className="glass p-10 rounded-[3rem]">
              <h3 className="text-sm tracking-[0.3em] uppercase text-gold font-bold mb-6">{t.profile.quickLinks}</h3>
              <div className="space-y-4">
                <Link href="/dashboard" className="block text-[11px] tracking-widest uppercase text-foreground/60 hover:text-gold transition-colors">{t.profile.myVoyages}</Link>
                <Link href="/fleet" className="block text-[11px] tracking-widest uppercase text-foreground/60 hover:text-gold transition-colors">{t.profile.exploreFleet}</Link>
                <button className="block text-[11px] tracking-widest uppercase text-red-400/60 hover:text-red-400 transition-colors">{t.profile.deactivate}</button>
              </div>
            </div>
          </div>

          {/* Main Form Area */}
          <div className="lg:col-span-2 space-y-12">
            <div className="glass p-12 rounded-[3.5rem]">
              <h3 className="text-3xl font-serif text-white mb-12">
                {t.profile.accountDetails.split(' ')[0]} <span className="text-gold italic">{t.profile.accountDetails.split(' ').slice(1).join(' ')}</span>
              </h3>
              
              <div className="grid grid-cols-1 md:grid-cols-2 gap-8 mb-12">
                <div className="space-y-3">
                  <label className="text-[10px] uppercase tracking-[0.2em] text-foreground/40 font-bold ml-4">{t.profile.fullName}</label>
                  <div className="w-full bg-surface-dark/50 border border-surface-border/50 rounded-2xl px-6 py-4 text-white font-light focus:border-gold/50 outline-none transition-all">
                    {user.name}
                  </div>
                </div>
                <div className="space-y-3">
                  <label className="text-[10px] uppercase tracking-[0.2em] text-foreground/40 font-bold ml-4">{t.profile.email}</label>
                  <div className="w-full bg-surface-dark/50 border border-surface-border/50 rounded-2xl px-6 py-4 text-white font-light focus:border-gold/50 outline-none transition-all">
                    {user.email}
                  </div>
                </div>
                <div className="space-y-3">
                  <label className="text-[10px] uppercase tracking-[0.2em] text-foreground/40 font-bold ml-4">{t.profile.phone}</label>
                  <div className="w-full bg-surface-dark/50 border border-surface-border/50 rounded-2xl px-6 py-4 text-white font-light focus:border-gold/50 outline-none transition-all placeholder:text-foreground/10">
                    {user.phoneNumber || "---"}
                  </div>
                </div>
                <div className="space-y-3">
                  <label className="text-[10px] uppercase tracking-[0.2em] text-foreground/40 font-bold ml-4">{t.profile.company}</label>
                  <div className="w-full bg-surface-dark/50 border border-surface-border/50 rounded-2xl px-6 py-4 text-white font-light focus:border-gold/50 outline-none transition-all placeholder:text-foreground/10">
                    {user.companyName || "---"}
                  </div>
                </div>
              </div>

              <button className="px-10 py-4 bg-gold text-background font-bold rounded-full text-[10px] tracking-widest uppercase shadow-2xl hover:scale-105 transition-transform">
                {t.profile.editInfo}
              </button>
            </div>

            <div className="glass p-12 rounded-[3.5rem]">
              <div className="flex justify-between items-center mb-12">
                <h3 className="text-3xl font-serif text-white">
                  {t.profile.recentBookings.split(' ')[0]} <span className="text-gold italic">{t.profile.recentBookings.split(' ').slice(1).join(' ')}</span>
                </h3>
                <Link href="/dashboard" className="text-[10px] tracking-widest uppercase text-gold hover:text-white transition-colors underline">{t.profile.viewAll}</Link>
              </div>
              
              <div className="space-y-6">
                {user.bookings.length > 0 ? (
                  user.bookings.map((booking) => (
                    <div key={booking.id} className="p-8 bg-surface-dark/40 border border-surface-border/30 rounded-3xl flex flex-col md:flex-row justify-between items-center gap-8 group hover:border-gold/20 transition-all">
                      <div className="flex items-center gap-6 text-left w-full">
                        <div className="w-16 h-16 rounded-2xl bg-gold/10 flex items-center justify-center text-gold text-xl font-serif overflow-hidden relative">
                           <img src={booking.yacht.imageUrl} alt="" className="w-full h-full object-cover opacity-50 group-hover:opacity-100 transition-opacity" />
                        </div>
                        <div>
                          <div className="text-white font-serif text-xl group-hover:text-gold transition-colors">{booking.yacht.name}</div>
                          <div className="text-[10px] text-foreground/40 uppercase tracking-widest">
                            {new Date(booking.startDate).toLocaleDateString()} - {new Date(booking.endDate).toLocaleDateString()}
                          </div>
                        </div>
                      </div>
                      <div className="text-right whitespace-nowrap">
                        <div className={`text-[9px] px-3 py-1 rounded-full border tracking-widest uppercase font-bold ${booking.status === 'CONFIRMED' ? 'border-green-400 text-green-400' : 'border-gold text-gold'}`}>
                          {booking.status}
                        </div>
                      </div>
                    </div>
                  ))
                ) : (
                  <div className="text-center py-12 border border-dashed border-surface-border/30 rounded-[2rem]">
                    <p className="text-foreground/30 text-[10px] uppercase tracking-[0.2em] mb-6">{t.profile.noVoyages}</p>
                    <Link href="/fleet">
                      <button className="text-[10px] tracking-widest uppercase text-gold font-bold hover:text-white transition-colors underline">
                        {t.profile.startPlanning}
                      </button>
                    </Link>
                  </div>
                )}
              </div>
            </div>
          </div>
        </div>
      </div>
    </main>
  );
}
