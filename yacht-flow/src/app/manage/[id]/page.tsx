"use client";

import { useState, useEffect, use } from "react";
import Image from "next/image";
import Link from "next/link";
import { useLanguage } from "@/locales/LanguageContext";
import { useSearchParams } from "next/navigation";

export default function ManageBookingPage({ params }: { params: Promise<{ id: string }> }) {
  const { id } = use(params);
  const searchParams = useSearchParams();
  const isSuccess = searchParams.get("success") === "true";
  const { t, lang } = useLanguage();
  const [activeTab, setActiveTab] = useState("services");
  const [orderedServices, setOrderedServices] = useState<string[]>([]);
  const [booking, setBooking] = useState<any>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    fetch(`/api/bookings/${id}`)
      .then(res => res.json())
      .then(data => setBooking(data))
      .catch(err => console.error(err))
      .finally(() => setLoading(false));
  }, [id]);

  if (loading) return <div className="min-h-screen bg-background flex items-center justify-center text-gold">Loading Voyage...</div>;
  if (!booking) return <div className="min-h-screen bg-background flex items-center justify-center text-gold">Voyage Not Found</div>;


  const services = [
    { id: "wine", name: "Premium Wine Cellar", icon: "🍷", price: "€250", desc: "Selection of 12 vintage Mediterranean wines." },
    { id: "massage", name: "Sunset Massage", icon: "💆‍♂️", price: "€150", desc: "1-hour professional massage on the deck." },
    { id: "sushi", name: "Sushi Platter", icon: "🍱", price: "€180", desc: "Freshly prepared omakase for 4 guests." },
    { id: "jetski", name: "Additional Jet-Ski", icon: "🌊", price: "€450", desc: "Full day rental of a high-speed Yamaha." },
  ];

  return (
    <main className="min-h-screen bg-background pt-32 pb-24 px-6 md:px-12">
      <div className="max-w-7xl mx-auto">
        {isSuccess && (
          <div className="mb-12 p-8 glass-gold rounded-3xl border border-gold/50 animate-reveal text-center relative overflow-hidden">
             <div className="absolute top-0 right-0 p-4 opacity-10 text-6xl">✨</div>
             <h2 className="text-3xl font-serif text-gold mb-2 italic">{lang === 'tr' ? 'Ödeme Başarılı' : 'Payment Successful'}</h2>
             <p className="text-sm text-foreground/60 tracking-widest uppercase">{lang === 'tr' ? 'Aramıza hoş geldiniz! Özel yolculuğunuz onaylandı.' : 'Welcome aboard! Your exclusive voyage is now confirmed.'}</p>
          </div>
        )}
        
        {/* Header */}
        <div className="flex flex-col md:flex-row justify-between items-start md:items-end gap-8 mb-16 animate-reveal">
          <div>
            <div className="text-gold tracking-[0.4em] uppercase text-xs mb-4 font-bold">{t.manage.tagline}</div>
            <h1 className="font-serif text-5xl md:text-7xl mb-4 italic">{t.manage.voyage} <span className="text-gold not-italic">#FLW-{id.slice(-4).toUpperCase()}</span></h1>
            <p className="text-foreground/50 font-light flex items-center gap-3">
              <span className="w-8 h-[1px] bg-gold"></span>
              {t.manage.status}
            </p>
          </div>
          
          <div className="flex gap-4">
             <div className="px-6 py-3 bg-gold text-background rounded-full text-xs tracking-widest uppercase font-bold shadow-lg cursor-pointer hover:scale-105 transition-transform">
               {t.manage.download}
             </div>
          </div>
        </div>

        <div className="grid lg:grid-cols-3 gap-12">
          {/* Tabs & Content */}
          <div className="lg:col-span-2 space-y-8">
            <div className="flex border-b border-surface-border gap-12 text-sm tracking-widest uppercase font-bold text-foreground/40">
              {[
                { id: "services", label: t.manage.tabs.services },
                { id: "itinerary", label: t.manage.tabs.itinerary },
                { id: "chat", label: t.manage.tabs.chat }
              ].map(tab => (
                <button 
                  key={tab.id}
                  onClick={() => setActiveTab(tab.id)}
                  className={`pb-4 transition-all relative ${activeTab === tab.id ? "text-gold" : "hover:text-white"}`}
                >
                  {tab.label}
                  {activeTab === tab.id && <div className="absolute bottom-0 left-0 w-full h-1 bg-gold rounded-full"></div>}
                </button>
              ))}
            </div>

            {activeTab === "services" && (
              <div className="grid grid-cols-1 md:grid-cols-2 gap-6 animate-reveal">
                {services.map(service => (
                  <div key={service.id} className="glass p-8 rounded-[2rem] group hover:border-gold/50 transition-all duration-500">
                    <div className="flex justify-between items-start mb-6">
                      <div className="text-4xl">{service.icon}</div>
                      <div className="text-gold font-bold text-lg">{service.price}</div>
                    </div>
                    <h3 className="text-2xl font-serif text-white mb-2">{service.name}</h3>
                    <p className="text-sm text-foreground/40 font-light mb-6 leading-relaxed">{service.desc}</p>
                    <button 
                      onClick={() => setOrderedServices(prev => prev.includes(service.id) ? prev.filter(s => s !== service.id) : [...prev, service.id])}
                      className={`w-full py-3 rounded-xl text-[10px] tracking-widest uppercase font-bold transition-all duration-500 ${orderedServices.includes(service.id) ? "bg-gold text-background" : "bg-white/5 border border-white/10 text-white hover:border-gold"}`}
                    >
                      {orderedServices.includes(service.id) ? t.manage.services.added : t.manage.services.add}
                    </button>
                  </div>
                ))}
              </div>
            )}

            {activeTab === "itinerary" && (
              <div className="space-y-8 animate-reveal">
                {/* AI Mood Selector */}
                <div className="glass p-10 rounded-[3rem] border border-gold/20 relative overflow-hidden">
                  <div className="absolute top-0 right-0 p-8 opacity-5 text-8xl">🧭</div>
                  <h3 className="text-3xl font-serif text-white mb-4">
                    {t.manage.itinerary.title.split(' ')[0]} <span className="text-gold italic">{t.manage.itinerary.title.split(' ').slice(1).join(' ')}</span>
                  </h3>
                  <p className="text-sm text-foreground/40 font-light mb-8 max-w-xl">{t.manage.itinerary.desc}</p>
                  
                  <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
                    {[
                      { mood: "Secluded", icon: "🏝️", desc: "Quiet bays" },
                      { mood: "Glamour", icon: "🥂", desc: "Beach clubs" },
                      { mood: "Adventure", icon: "🤿", desc: "Diving spots" },
                      { mood: "Heritage", icon: "🏛️", desc: "Ancient ruins" },
                    ].map((m) => (
                      <button key={m.mood} className="p-6 glass border border-white/5 hover:border-gold/50 rounded-2xl transition-all group text-center">
                        <div className="text-3xl mb-3 group-hover:scale-110 transition-transform">{m.icon}</div>
                        <div className="text-[10px] tracking-widest uppercase font-bold text-white mb-1">{m.mood}</div>
                        <div className="text-[9px] text-foreground/40 uppercase">{m.desc}</div>
                      </button>
                    ))}
                  </div>
                </div>

                {/* Timeline */}
                <div className="glass p-12 rounded-[3rem]">
                  <h3 className="text-3xl font-serif text-white mb-8">
                    {t.manage.itinerary.timeline.split(' ')[0]} <span className="text-gold italic">{t.manage.itinerary.timeline.split(' ').slice(1).join(' ')}</span>
                  </h3>
                  <div className="space-y-12 relative before:absolute before:left-8 before:top-2 before:bottom-2 before:w-[1px] before:bg-gold/20">
                    {[
                      { day: "Day 1", spot: "Bodrum Marina", activity: "Check-in at 14:00, Welcome Champagne", time: "14:00", img: "/dest-bodrum.png" },
                      { day: "Day 2", spot: "Orak Island", activity: "The 'Maldives of Bodrum'. Crystal clear waters and lunch on deck.", time: "11:30", img: "/dest-gocek.png" },
                      { day: "Day 3", spot: "Aquarium Bay", activity: "Perfect for snorkeling and seeing the underwater life.", time: "09:00", img: "/dest-marmaris.png" },
                    ].map((item, i) => (
                      <div key={i} className="flex flex-col md:flex-row gap-8 md:gap-12 relative z-10 group">
                        <div className="w-16 h-16 rounded-full bg-surface border border-gold flex items-center justify-center text-gold font-serif text-xl shrink-0 shadow-xl group-hover:bg-gold group-hover:text-background transition-all duration-500">
                          {i+1}
                        </div>
                        <div className="flex-1 flex flex-col md:flex-row gap-8 items-center glass p-6 rounded-3xl border border-white/5 hover:border-gold/20 transition-all">
                          <div className="relative w-full md:w-32 aspect-video md:aspect-square rounded-2xl overflow-hidden shrink-0">
                            <Image src={item.img} alt={item.spot} fill className="object-cover group-hover:scale-110 transition-transform duration-700" />
                          </div>
                          <div>
                            <div className="text-[10px] tracking-widest uppercase text-gold mb-2 font-bold">{item.day} • {item.time}</div>
                            <h4 className="text-2xl font-serif text-white mb-2">{item.spot}</h4>
                            <p className="text-sm text-foreground/40 font-light leading-relaxed">{item.activity}</p>
                          </div>
                        </div>
                      </div>
                    ))}
                  </div>
                </div>
              </div>
            )}

            {activeTab === "chat" && (
              <div className="glass h-[500px] rounded-[3rem] animate-reveal flex flex-col overflow-hidden">
                <div className="p-8 border-b border-surface-border bg-white/5 flex items-center gap-4">
                  <div className="w-12 h-12 rounded-full bg-gold/20 border border-gold/30 flex items-center justify-center text-gold text-xl">👨‍✈️</div>
                  <div>
                    <div className="text-sm font-bold text-white">{t.manage.chat.captain}</div>
                    <div className="text-[10px] text-gold uppercase tracking-widest font-bold">{t.manage.chat.status}</div>
                  </div>
                </div>
                <div className="flex-1 p-8 space-y-6 overflow-y-auto">
                   <div className="bg-surface-dark border border-surface-border p-6 rounded-2xl rounded-tl-none max-w-[80%]">
                      <p className="text-sm text-foreground/60 font-light">Welcome to YachtFlow, Mr. Guest. I am Captain Mehmet. Your route is ready and the crew is preparing the vessel for your arrival. Do you have any specific requests for the welcome drinks?</p>
                   </div>
                   <div className="bg-gold/10 border border-gold/30 p-6 rounded-2xl rounded-tr-none max-w-[80%] ml-auto">
                      <p className="text-sm text-gold font-light">Thank you Captain. We would love to have some chilled rose wine and fresh fruit platters upon boarding.</p>
                   </div>
                </div>
                <div className="p-6 border-t border-surface-border flex gap-4">
                  <input type="text" placeholder={t.manage.chat.placeholder} className="flex-1 bg-surface-dark border border-surface-border rounded-full px-8 py-4 text-sm outline-none focus:border-gold transition-colors" />
                  <button aria-label="Send Message" className="w-14 h-14 rounded-full bg-gold text-background flex items-center justify-center hover:scale-105 transition-transform shadow-lg">
                    <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M5 13l4 4L19 7"></path></svg>
                  </button>
                </div>
              </div>
            )}
          </div>

          {/* Right Sidebar: Booking Stats */}
          <div className="space-y-8">
            <div className="glass p-8 rounded-[3rem] overflow-hidden relative">
              <div className="absolute inset-0 z-0">
                <Image src={booking.yacht.imageUrl} alt={booking.yacht.name} fill className="object-cover opacity-20" />
                <div className="absolute inset-0 bg-gradient-to-t from-surface via-transparent to-transparent"></div>
              </div>
              <div className="relative z-10">
                <h3 className="text-2xl font-serif text-white mb-8">{t.manage.vessel.title}</h3>
                <div className="space-y-6">
                  <div className="flex justify-between items-center border-b border-surface-border/30 pb-4">
                    <span className="text-xs text-foreground/40 tracking-widest uppercase font-bold">{t.manage.vessel.yacht}</span>
                    <span className="text-sm text-white font-serif">{booking.yacht.name}</span>
                  </div>
                  <div className="flex justify-between items-center border-b border-surface-border/30 pb-4">
                    <span className="text-xs text-foreground/40 tracking-widest uppercase font-bold">{t.manage.vessel.checkIn}</span>
                    <span className="text-sm text-white font-serif">{new Date(booking.startDate).toLocaleDateString(lang === 'tr' ? 'tr-TR' : 'en-US')}</span>
                  </div>
                  <div className="flex justify-between items-center border-b border-surface-border/30 pb-4">
                    <span className="text-xs text-foreground/40 tracking-widest uppercase font-bold">{t.manage.vessel.checkOut}</span>
                    <span className="text-sm text-white font-serif">{new Date(booking.endDate).toLocaleDateString(lang === 'tr' ? 'tr-TR' : 'en-US')}</span>
                  </div>
                </div>
              </div>
            </div>

            <div className="glass-gold p-10 rounded-[3rem]">
               <h3 className="text-xl font-serif text-gold mb-6">{t.manage.health.title}</h3>
               <div className="space-y-6">
                  <div>
                    <div className="flex justify-between text-[10px] tracking-widest uppercase text-gold/60 mb-2 font-bold">
                      <span>{t.manage.health.provisions}</span>
                      <span>85%</span>
                    </div>
                    <div className="h-1.5 w-full bg-gold/10 rounded-full overflow-hidden">
                       <div className="h-full bg-gold w-[85%] rounded-full"></div>
                    </div>
                  </div>
                  <div>
                    <div className="flex justify-between text-[10px] tracking-widest uppercase text-gold/60 mb-2 font-bold">
                      <span>{t.manage.health.crew}</span>
                      <span>100%</span>
                    </div>
                    <div className="h-1.5 w-full bg-gold/10 rounded-full overflow-hidden">
                       <div className="h-full bg-gold w-full rounded-full"></div>
                    </div>
                  </div>
               </div>
            </div>

            <div className="glass p-8 rounded-[3rem] border border-white/5 animate-reveal delay-300">
               <h3 className="font-serif text-xl mb-6">
                 {t.manage.riviera.title.split(' ')[0]} <span className="text-gold italic">{t.manage.riviera.title.split(' ').slice(1).join(' ')}</span>
               </h3>
               <div className="grid grid-cols-2 gap-4">
                  <div className="p-4 bg-white/5 rounded-2xl border border-white/5 text-center">
                     <div className="text-2xl mb-1">☀️</div>
                     <div className="text-lg font-serif">32°C</div>
                     <div className="text-[8px] uppercase tracking-widest text-foreground/40">{t.manage.riviera.air}</div>
                  </div>
                  <div className="p-4 bg-white/5 rounded-2xl border border-white/5 text-center">
                     <div className="text-2xl mb-1">🌊</div>
                     <div className="text-lg font-serif">26°C</div>
                     <div className="text-[8px] uppercase tracking-widest text-foreground/40">{t.manage.riviera.sea}</div>
                  </div>
                  <div className="p-4 bg-white/5 rounded-2xl border border-white/5 text-center">
                     <div className="text-2xl mb-1">🎐</div>
                     <div className="text-lg font-serif">8 kts</div>
                     <div className="text-[8px] uppercase tracking-widest text-foreground/40">{t.manage.riviera.wind}</div>
                  </div>
                  <div className="p-4 bg-white/5 rounded-2xl border border-white/5 text-center">
                     <div className="text-2xl mb-1">🔱</div>
                     <div className="text-lg font-serif">{lang === 'tr' ? 'Düşük' : 'Low'}</div>
                     <div className="text-[8px] uppercase tracking-widest text-foreground/40">{t.manage.riviera.swell}</div>
                  </div>
               </div>
            </div>
          </div>
        </div>
      </div>
    </main>
  );
}
