"use client";

import Image from "next/image";
import Link from "next/link";
import { useLanguage } from "@/locales/LanguageContext";
import { useEffect, useState } from "react";
import RouteExplorer from "@/app/components/RouteExplorer";

export default function Home() {
  const { t } = useLanguage();
  const [scrollY, setScrollY] = useState(0);

  useEffect(() => {
    const handleScroll = () => setScrollY(window.scrollY);
    window.addEventListener("scroll", handleScroll);
    return () => window.removeEventListener("scroll", handleScroll);
  }, []);

  return (
    <main className="min-h-screen bg-background">
      {/* Cinematic Hero Section */}
      <section className="relative min-h-screen flex items-center justify-center overflow-hidden pt-32 pb-24">
        <div className="absolute inset-0 z-0 scale-110" style={{ transform: `translateY(${scrollY * 0.3}px) scale(1.1)` }}>
          <Image
            src="/hero-bg.png"
            alt="Luxury superyacht"
            fill
            sizes="100vw"
            className="object-cover opacity-50"
            priority
          />
          <div className="absolute inset-0 bg-gradient-to-b from-background/40 via-transparent to-background"></div>
        </div>

        <div className="relative z-10 text-center px-4 max-w-6xl mx-auto mt-12 md:mt-20 w-full">
          <div className="inline-block px-4 py-1 border border-gold/30 rounded-full text-[10px] tracking-[0.3em] uppercase text-gold mb-6 md:mb-8 animate-reveal opacity-0">
            {t.hero.tagline}
          </div>
          
          <h1 className="font-serif text-5xl sm:text-7xl md:text-9xl lg:text-[11rem] text-foreground mb-6 md:mb-8 leading-tight md:leading-[0.8] tracking-tighter animate-reveal opacity-0 stagger-1">
            {t.hero.title.split(' ')[0]} <span className="shimmer-text italic font-light">{t.hero.title.split(' ').slice(1).join(' ')}</span> <br className="hidden md:block" />
            <span className="md:hidden"> </span>
            {t.hero.brandLine.split('VOY')[0]} <span className="text-gold">VOY.</span>
          </h1>
          
          <p className="text-base md:text-2xl text-foreground/60 font-light mb-8 md:mb-12 max-w-3xl mx-auto animate-reveal opacity-0 stagger-2 leading-relaxed px-4">
            {t.hero.subtitle}
          </p>
          
          {/* Advanced Search Widget */}
          <div className="glass p-2 md:p-2 rounded-[1.5rem] md:rounded-[2.5rem] flex flex-col md:flex-row gap-1 md:gap-2 max-w-5xl mx-auto animate-reveal opacity-0 stagger-3 shadow-2xl">
            <div className="flex-[1.5] flex flex-col text-left px-6 md:px-8 py-3 md:py-4 border-b md:border-b-0 md:border-r border-surface-border group hover:bg-white/5 transition-colors rounded-t-[1.5rem] md:rounded-l-[2rem] md:rounded-tr-none">
              <span className="text-[9px] md:text-[10px] tracking-widest text-gold uppercase mb-1 md:mb-2 font-bold">{t.hero.search.location}</span>
              <select title={t.hero.search.location} className="bg-transparent text-base md:text-lg text-foreground outline-none appearance-none cursor-pointer font-serif">
                <option value="bodrum">{t.filters.locations.bodrum}</option>
                <option value="gocek">{t.filters.locations.gocek}</option>
                <option value="marmaris">{t.filters.locations.marmaris}</option>
              </select>
            </div>
            <div className="flex-1 flex flex-col text-left px-6 md:px-8 py-3 md:py-4 border-b md:border-b-0 md:border-r border-surface-border group hover:bg-white/5 transition-colors">
              <span className="text-[9px] md:text-[10px] tracking-widest text-gold uppercase mb-1 md:mb-2 font-bold">{t.hero.search.date}</span>
              <input type="date" title={t.hero.search.date} className="bg-transparent text-base md:text-lg text-foreground outline-none cursor-pointer font-serif w-full" />
            </div>
            <div className="flex-1 flex flex-col text-left px-6 md:px-8 py-3 md:py-4 group hover:bg-white/5 transition-colors">
              <span className="text-[9px] md:text-[10px] tracking-widest text-gold uppercase mb-1 md:mb-2 font-bold">{t.hero.search.guests}</span>
              <select title={t.hero.search.guests} className="bg-transparent text-base md:text-lg text-foreground outline-none appearance-none cursor-pointer font-serif">
                <option value="2">{t.booking.guestsOption.replace('{num}', '2')}</option>
                <option value="4">{t.booking.guestsOption.replace('{num}', '4')}</option>
                <option value="6">{t.booking.guestsOptionPlus.replace('{num}', '6')}</option>
              </select>
            </div>
            <Link href="/fleet" className="shrink-0 p-1 md:p-0">
              <button className="h-full w-full bg-gold text-background font-bold px-8 md:px-12 py-5 md:py-6 rounded-[1.2rem] md:rounded-[2rem] hover:bg-gold-hover transition-all duration-500 hover:scale-[1.02] shadow-lg text-[11px] md:text-sm tracking-widest uppercase">
                {t.hero.search.button}
              </button>
            </Link>
          </div>
        </div>
        
        <div className="absolute bottom-12 left-1/2 -translate-x-1/2 animate-bounce opacity-40">
           <svg className="w-6 h-6 text-gold" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth="1.5" d="M19.5 8.25l-7.5 7.5-7.5-7.5"></path></svg>
        </div>
      </section>

      {/* Prime Destinations - Cinematic Cards */}
      <section id="destinations" className="py-32 px-6 md:px-12 max-w-8xl mx-auto overflow-hidden">
        <div className="mb-24 flex flex-col md:flex-row justify-between items-end gap-8">
          <div className="max-w-2xl">
            <h2 className="font-serif text-5xl md:text-7xl mb-6 leading-tight">
              {t.destinations.title.split('Destinations')[0]} <span className="text-gold italic">Destinations</span>
            </h2>
            <p className="text-foreground/50 text-xl font-light leading-relaxed">{t.destinations.subtitle}</p>
          </div>
          <Link href="/fleet" className="group flex items-center gap-4 text-gold tracking-[0.3em] uppercase text-xs">
            {t.destinations.exploreAll} <span className="w-12 h-[1px] bg-gold group-hover:w-20 transition-all duration-500"></span>
          </Link>
        </div>

        <div className="grid grid-cols-1 md:grid-cols-3 gap-12">
          {[
            { name: "Bodrum", img: "/dest-bodrum.png", desc: "The Saint-Tropez of Turkey, where history meets luxury." },
            { name: "Göcek", img: "/dest-gocek.png", desc: "A hidden paradise of twelve islands and crystal clear bays." },
            { name: "Marmaris", img: "/dest-marmaris.png", desc: "Emerald green forests meeting deep blue waters." }
          ].map((dest, i) => (
            <Link key={i} href={`/fleet?location=${dest.name}`} className="group relative aspect-[3/4.5] rounded-[3rem] overflow-hidden shadow-2xl transform transition-all duration-700 hover:-translate-y-4 cursor-pointer">
              <Image src={dest.img} alt={dest.name} fill sizes="(max-width: 768px) 100vw, 33vw" className="object-cover group-hover:scale-110 transition-transform duration-1000" />
              <div className="absolute inset-0 bg-gradient-to-t from-background via-background/10 to-transparent z-10"></div>
              <div className="absolute inset-0 border-[0.5px] border-white/10 rounded-[3rem] m-4 z-0"></div>
              
              <div className="absolute bottom-0 p-12 w-full z-20">
                <div className="text-[10px] tracking-[0.4em] uppercase text-gold/80 mb-4 font-bold">Experience {dest.name}</div>
                <h3 className="text-5xl font-serif text-white mb-6 leading-none">{dest.name}</h3>
                <p className="text-sm text-white/60 font-light mb-8 opacity-0 group-hover:opacity-100 transform translate-y-4 group-hover:translate-y-0 transition-all duration-500 leading-relaxed">{dest.desc}</p>
                <div className="inline-block px-8 py-3 bg-white/10 backdrop-blur-md border border-white/20 text-white rounded-full text-[10px] tracking-widest uppercase group-hover:bg-gold group-hover:text-background group-hover:border-gold transition-all duration-500">
                  Explore {dest.name}
                </div>
              </div>
            </Link>
          ))}
        </div>
      </section>

      {/* Privileged Offers Section */}
      <section className="py-24 bg-gold/5 relative overflow-hidden">
        <div className="absolute top-0 left-0 w-full h-[1px] bg-gradient-to-r from-transparent via-gold/30 to-transparent"></div>
        <div className="max-w-8xl mx-auto px-6 md:px-12">
          <div className="text-center mb-16">
            <div className="text-xs tracking-[0.4em] uppercase text-gold font-bold mb-4">Limited Availability</div>
            <h2 className="font-serif text-5xl md:text-6xl italic">Privileged <span className="not-italic">Offers</span></h2>
          </div>

          <div className="grid md:grid-cols-2 lg:grid-cols-3 gap-8">
            {[
              { title: "Early Bird Privilege", code: "EARLY2025", disc: "-15%", desc: "Secure your summer voyage 6 months in advance.", bg: "bg-surface" },
              { title: "Honeymoon Suite", code: "LOVEFLOW", disc: "Gift", desc: "Complimentary champagne & private island dinner.", bg: "bg-gold text-background" },
              { title: "Long Stay Benefit", code: "EXTENDED", disc: "-20%", desc: "Stay 14 days or more for exclusive platinum rates.", bg: "bg-surface" },
            ].map((offer, i) => (
              <div key={i} className={`p-10 rounded-[3rem] border border-gold/20 flex flex-col justify-between h-80 relative group overflow-hidden ${offer.bg}`}>
                {offer.bg.includes('surface') && <div className="absolute top-0 right-0 p-8 text-6xl opacity-5">💎</div>}
                <div>
                   <div className="text-3xl font-serif mb-2">{offer.title}</div>
                   <p className={`text-sm font-light ${offer.bg.includes('gold') ? 'text-background/70' : 'text-foreground/50'}`}>{offer.desc}</p>
                </div>
                <div className="flex justify-between items-end">
                   <div>
                      <div className={`text-[10px] uppercase tracking-widest mb-1 ${offer.bg.includes('gold') ? 'text-background/60' : 'text-gold'}`}>Promo Code</div>
                      <div className="text-xl font-bold tracking-tighter">{offer.code}</div>
                   </div>
                   <div className="text-4xl font-serif italic">{offer.disc}</div>
                </div>
              </div>
            ))}
          </div>
        </div>
        <div className="absolute bottom-0 left-0 w-full h-[1px] bg-gradient-to-r from-transparent via-gold/30 to-transparent"></div>
      </section>

      {/* Route Explorer - INNOVATIVE FEATURE */}
      <section className="py-24 px-6 md:px-12 max-w-8xl mx-auto">
        <RouteExplorer />
      </section>

      {/* Experience Section - Editorial Layout */}
      <section id="experiences" className="py-32 px-6 md:px-12 max-w-8xl mx-auto border-t border-surface-border/30 relative">
        <div className="absolute top-0 right-0 w-1/2 h-full bg-gold/5 blur-[120px] rounded-full -translate-y-1/2 translate-x-1/2 pointer-events-none"></div>
        
        <div className="grid md:grid-cols-2 gap-24 items-center">
          <div className="relative group">
            <div className="relative aspect-[4/5] w-full max-w-xl mx-auto rounded-[4rem] overflow-hidden shadow-2xl">
              <Image
                src="/experience.png"
                alt="People enjoying yacht experience"
                fill
                sizes="(max-width: 768px) 100vw, 50vw"
                className="object-cover group-hover:scale-105 transition-transform duration-1000"
              />
              <div className="absolute inset-0 bg-gold/10 mix-blend-overlay"></div>
            </div>
            {/* Floating Detail */}
            <div className="absolute -bottom-10 -right-10 glass-gold p-8 rounded-3xl max-w-[280px] hidden lg:block animate-float">
               <div className="text-3xl font-serif text-gold mb-2">350+</div>
               <div className="text-xs tracking-widest uppercase text-foreground/70 leading-relaxed font-bold">Curated journeys created this season</div>
            </div>
          </div>
          
          <div className="max-w-2xl">
            <h2 className="font-serif text-6xl md:text-8xl mb-8 leading-[0.9]">
              {t.experiences.title.split('Experiences')[0]} <span className="text-gold italic">Experiences</span>
            </h2>
            <p className="text-foreground/50 text-xl mb-12 leading-relaxed font-light">
              {t.experiences.desc}
            </p>
            
            <div className="space-y-12">
              {t.experiences.items.map((item, i) => (
                <div key={i} className="flex items-start gap-8 group">
                  <div className="w-16 h-16 rounded-full border border-gold/30 flex items-center justify-center text-gold font-serif text-2xl shrink-0 group-hover:bg-gold group-hover:text-background transition-all duration-500">
                    0{i+1}
                  </div>
                  <div className="pt-2">
                    <h3 className="text-3xl font-serif mb-4 group-hover:text-gold transition-colors">{item.title}</h3>
                    <p className="text-lg text-foreground/40 font-light leading-relaxed">{item.desc}</p>
                  </div>
                </div>
              ))}
            </div>
          </div>
        </div>
      </section>
      {/* Contact & Concierge Section */}
      <section id="contact" className="py-32 bg-surface-dark relative">
        <div className="max-w-7xl mx-auto px-6 md:px-12 grid lg:grid-cols-2 gap-24 items-center">
          <div>
            <div className="text-xs tracking-[0.4em] uppercase text-gold font-bold mb-6">Personal Concierge</div>
            <h2 className="font-serif text-6xl md:text-7xl mb-8 leading-tight">Your <span className="text-gold italic">Wish</span> is Our Command</h2>
            <p className="text-foreground/50 text-xl font-light leading-relaxed mb-12">
              Whether you need a helicopter transfer to your vessel, a private Michelin-starred chef on board, 
              or a curated itinerary for the most hidden bays, our 24/7 concierge team is at your disposal.
            </p>
            
            <div className="space-y-8">
              <div className="flex items-center gap-6 group cursor-pointer">
                <div className="w-14 h-14 rounded-full border border-gold/30 flex items-center justify-center text-gold group-hover:bg-gold group-hover:text-background transition-all duration-500">
                  <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth="1.5" d="M3 5a2 2 0 012-2h3.28a1 1 0 01.948.684l1.498 4.493a1 1 0 01-.502 1.21l-2.257 1.13a11.042 11.042 0 005.516 5.516l1.13-2.257a1 1 0 011.21-.502l4.493 1.498a1 1 0 01.684.949V19a2 2 0 01-2 2h-1C9.716 21 3 14.284 3 6V5z"></path></svg>
                </div>
                <div>
                  <div className="text-[10px] tracking-widest uppercase text-foreground/30 font-bold mb-1">Global Concierge</div>
                  <div className="text-xl font-serif text-white">+90 252 VOY LINE</div>
                </div>
              </div>
              <div className="flex items-center gap-6 group cursor-pointer">
                <div className="w-14 h-14 rounded-full border border-gold/30 flex items-center justify-center text-gold group-hover:bg-gold group-hover:text-background transition-all duration-500">
                  <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth="1.5" d="M3 8l7.89 5.26a2 2 0 002.22 0L21 8M5 19h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v10a2 2 0 002 2z"></path></svg>
                </div>
                <div>
                  <div className="text-[10px] tracking-widest uppercase text-foreground/30 font-bold mb-1">Electronic Correspondence</div>
                  <div className="text-xl font-serif text-white">concierge@voy-yachting.com</div>
                </div>
              </div>
            </div>
          </div>
          
          <div className="glass p-12 rounded-[3.5rem] border border-gold/10">
            <form className="space-y-8">
              <div className="grid md:grid-cols-2 gap-8">
                <div>
                  <label className="text-[10px] tracking-widest text-gold uppercase mb-3 block font-bold">Your Name</label>
                  <input type="text" className="w-full bg-white/5 border-b border-white/10 py-3 outline-none focus:border-gold transition-colors text-white" placeholder="Alexander..." />
                </div>
                <div>
                  <label className="text-[10px] tracking-widest text-gold uppercase mb-3 block font-bold">Inquiry Type</label>
                  <select title="Inquiry Type" className="w-full bg-white/5 border-b border-white/10 py-3 outline-none focus:border-gold transition-colors text-white appearance-none">
                    <option>Bespoke Charter</option>
                    <option>Event Hosting</option>
                    <option>Corporate Partnership</option>
                  </select>
                </div>
              </div>
              <div>
                <label className="text-[10px] tracking-widest text-gold uppercase mb-3 block font-bold">Message</label>
                <textarea className="w-full bg-white/5 border-b border-white/10 py-3 outline-none focus:border-gold transition-colors text-white h-32 resize-none" placeholder="How may we elevate your experience?"></textarea>
              </div>
              <button className="w-full bg-gold text-background font-bold py-6 rounded-2xl text-xs tracking-[0.3em] uppercase hover:bg-gold-hover transition-all shadow-2xl">
                Send Request
              </button>
            </form>
          </div>
        </div>
      </section>


    </main>
  );
}
