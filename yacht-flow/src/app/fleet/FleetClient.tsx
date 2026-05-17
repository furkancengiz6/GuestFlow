"use client";

import Image from "next/image";
import Link from "next/link";
import { useLanguage } from "@/locales/LanguageContext";
import FleetFilters from "@/app/components/FleetFilters";
import WeatherWidget from "@/app/components/WeatherWidget";
import { useState, useEffect } from "react";
import { createPortal } from "react-dom";

export default function FleetClient({ yachts }: { yachts: any[] }) {
  const { t, lang } = useLanguage();
  const [mounted, setMounted] = useState(false);
  const [isFilterOpen, setIsFilterOpen] = useState(false);

  useEffect(() => {
    setMounted(true);
  }, []);

  return (
    <>
    {/* Live Conditions - Premium Widget */}
    <div className="mb-16 animate-reveal">
      <WeatherWidget />
    </div>

    <div className="flex flex-col lg:flex-row gap-12">
      {/* Sidebar - Desktop */}
      <aside className="hidden lg:block w-72 shrink-0 animate-reveal">
        <FleetFilters />
      </aside>

      {/* Yacht Grid */}
      <div className="flex-1">
        <div className="mb-16 animate-reveal">
          <div className="flex justify-between items-end mb-4">
            <h1 className="font-serif text-5xl md:text-7xl tracking-tighter">
              {t.nav.fleet.split(' ')[0]} <span className="text-gold italic font-light">{t.nav.fleet.split(' ').slice(1).join(' ')}</span>
            </h1>
            
            {/* Mobile Filter Trigger */}
            <button 
              onClick={() => setIsFilterOpen(true)}
              className="lg:hidden flex items-center gap-2 bg-gold text-background px-6 py-3 rounded-full text-[10px] tracking-widest uppercase font-bold shadow-xl active:scale-95 transition-transform"
            >
              <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M3 4a1 1 0 011-1h16a1 1 0 011 1v2.586a1 1 0 01-.293.707l-6.414 6.414a1 1 0 00-.293.707V17l-4 4v-6.586a1 1 0 00-.293-.707L3.293 7.293A1 1 0 013 6.586V4z"></path></svg>
              {t.filters.title}
            </button>
          </div>
          
          <p className="text-foreground/40 font-light max-w-2xl text-xl leading-relaxed">
            Curated selection of the most exquisite yachts in the Turkish Riviera. 
            Performance meeting unprecedented luxury.
          </p>
        </div>

        {yachts.length > 0 ? (
          <div className="grid md:grid-cols-2 gap-8">
            {yachts.map((yacht, index) => (
              <Link href={`/fleet/${yacht.id}`} key={yacht.id} className="group animate-reveal" style={{ animationDelay: `${index * 50}ms` }}>
                <div className="glass rounded-[2.5rem] overflow-hidden hover:border-gold/40 transition-all duration-700 h-full flex flex-col relative group">
                  <div className="relative aspect-[4/3] w-full overflow-hidden shrink-0">
                    <Image 
                      src={yacht.imageUrl} 
                      alt={yacht.name} 
                      fill 
                      sizes="(max-width: 768px) 100vw, 50vw"
                      className="object-cover group-hover:scale-110 transition-transform duration-1000 ease-out"
                    />
                    <div className="absolute top-6 left-6 bg-background/80 backdrop-blur-md px-4 py-1 rounded-full text-[10px] tracking-widest uppercase text-gold font-bold border border-gold/10">
                      {yacht.type}
                    </div>
                  </div>
                  
                  <div className="p-8 flex-1 flex flex-col justify-between">
                    <div>
                      <div className="flex justify-between items-start mb-4">
                        <h2 className="font-serif text-3xl group-hover:text-gold transition-colors">{yacht.name}</h2>
                        <div className="text-right">
                          <div className="text-2xl font-serif text-gold">
                            €{mounted ? yacht.pricePerDay.toLocaleString(lang === 'tr' ? 'tr-TR' : 'en-US') : yacht.pricePerDay}
                          </div>
                          <div className="text-[8px] text-foreground/30 uppercase tracking-widest">/ Day</div>
                        </div>
                      </div>
                      
                      <p className="text-sm text-foreground/50 mb-8 flex items-center gap-2 font-light italic">
                        <svg className="w-4 h-4 text-gold" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth="1.5" d="M15 10.5a3 3 0 11-6 0 3 3 0 016 0z"></path><path strokeLinecap="round" strokeLinejoin="round" strokeWidth="1.5" d="M19.5 10.5c0 7.142-7.5 11.25-7.5 11.25S4.5 17.642 4.5 10.5a7.5 7.5 0 1115 0z"></path></svg>
                        {yacht.location}
                      </p>
                    </div>

                    <div className="grid grid-cols-3 gap-4 border-t border-surface-border/50 pt-6 text-center">
                      <div>
                        <div className="text-[8px] text-foreground/20 uppercase tracking-widest mb-1 font-bold">Length</div>
                        <div className="text-xs text-foreground/70">{yacht.length}</div>
                      </div>
                      <div>
                        <div className="text-[8px] text-foreground/20 uppercase tracking-widest mb-1 font-bold">Guests</div>
                        <div className="text-xs text-foreground/70">{yacht.guests} Max</div>
                      </div>
                      <div>
                        <div className="text-[8px] text-foreground/20 uppercase tracking-widest mb-1 font-bold">Cabins</div>
                        <div className="text-xs text-foreground/70">{yacht.cabins} Suite</div>
                      </div>
                    </div>
                  </div>
                </div>
              </Link>
            ))}
          </div>
        ) : (
          <div className="glass p-32 rounded-[4rem] text-center border-dashed border-gold/10">
            <div className="text-7xl mb-8 animate-float">⚓</div>
            <h3 className="text-4xl font-serif text-white mb-6">No Vessels Matching</h3>
            <p className="text-foreground/40 font-light mb-10 max-w-md mx-auto text-lg leading-relaxed">
              We couldn't find a vessel matching your exact criteria. Our concierge can arrange a bespoke collection for you.
            </p>
            <Link href="/fleet">
              <button className="px-12 py-5 bg-gold text-background font-bold text-[10px] tracking-widest uppercase rounded-full shadow-2xl hover:scale-105 transition-transform">
                Clear All Filters
              </button>
            </Link>
          </div>
        )}
      </div>
    </div>
    
    {/* Mobile Filter Overlay */}
    {mounted && typeof document !== "undefined" && createPortal(
      <div className={`fixed inset-0 z-[100] bg-background/95 backdrop-blur-2xl lg:hidden transition-all duration-700 ${isFilterOpen ? "opacity-100 translate-y-0" : "opacity-0 translate-y-10 pointer-events-none"}`}>
        <div className="p-8 pt-20">
          <button 
            onClick={() => setIsFilterOpen(false)}
            className="absolute top-8 right-8 text-foreground/40 hover:text-white transition-colors"
            title={lang === 'tr' ? 'Kapat' : 'Close'}
            aria-label={lang === 'tr' ? 'Filtreleri Kapat' : 'Close Filters'}
          >
            <svg className="w-8 h-8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth="1.5" d="M6 18L18 6M6 6l12 12"></path></svg>
          </button>
          <div className="max-w-md mx-auto">
            <FleetFilters />
            <button 
              onClick={() => setIsFilterOpen(false)}
              className="w-full mt-12 bg-gold text-background py-5 rounded-2xl font-bold text-xs tracking-widest uppercase shadow-2xl"
            >
              Show Results
            </button>
          </div>
        </div>
      </div>,
      document.body
    )}
    </>
  );
}
