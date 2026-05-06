"use client";

import Image from "next/image";
import Link from "next/link";
import { useEffect, useState } from "react";

export default function ExperiencePage() {
  const [activeSection, setActiveSection] = useState(0);

  const features = [
    {
      title: "Route Explorer",
      subtitle: "Visual Journey Planning",
      desc: "Our interactive SVG-based mapping engine allows guests to discover hidden coves and ancient ruins. Plan your route with surgical precision and visualize your Aegean dream before setting sail.",
      img: "/dest-bodrum.png",
      tag: "INNOVATION"
    },
    {
      title: "Bespoke Configurator",
      subtitle: "Tailored to Perfection",
      desc: "Luxury is in the details. From Michelin-star menus to onboard DJ setups and wellness therapists, our multi-step configurator lets you design an experience that is uniquely yours.",
      img: "/yacht-1.png",
      tag: "LUXURY"
    },
    {
      "title": "Unified Ecosystem",
      "subtitle": "Web to Mobile Sync",
      "desc": "A seamless bridge between devices. Whether you are on a desktop at home or an iPhone on the deck, your concierge requests, route updates, and crew chat are perfectly synchronized in real-time.",
      "img": "/yacht-2.png",
      "tag": "TECHNOLOGY"
    },
    {
      "title": "Fleet Command",
      "subtitle": "Intelligence for Owners",
      "desc": "Empowering yacht owners with real-time revenue analytics, maintenance tracking, and demand forecasting. Turn your vessel into a high-performance digital asset.",
      "img": "/hero-bg.png",
      "tag": "MANAGEMENT"
    }
  ];

  return (
    <main className="min-h-screen bg-background">
      {/* Editorial Hero */}
      <section className="h-screen relative flex items-center justify-center overflow-hidden border-b border-gold/10">
        <div className="absolute inset-0 z-0">
          <div className="absolute inset-0 bg-gradient-to-b from-background via-transparent to-background z-10"></div>
          <Image src="/hero-bg.png" alt="YachtFlow Hero" fill className="object-cover opacity-30 scale-105 animate-pulse-slow" />
        </div>
        
        <div className="relative z-20 text-center px-6 max-w-5xl">
          <div className="inline-block px-4 py-1 border border-gold/30 rounded-full text-[10px] tracking-[0.5em] uppercase text-gold mb-8 animate-reveal">
            A New Era of Maritime Excellence
          </div>
          <h1 className="font-serif text-6xl md:text-9xl text-white mb-12 leading-[0.8] tracking-tighter animate-reveal stagger-1">
            Beyond the <span className="text-gold italic">Horizon</span>
          </h1>
          <p className="text-xl md:text-3xl text-foreground/50 font-light leading-relaxed animate-reveal stagger-2 max-w-3xl mx-auto">
            YachtFlow is not just a platform; it is a technological ecosystem designed for the world's most discerning travelers and yacht owners.
          </p>
        </div>
      </section>

      {/* Feature Showcase - Vertical Scroll Design */}
      <section className="py-32">
        {features.map((f, i) => (
          <div key={i} className="min-h-[80vh] flex items-center px-6 md:px-24 mb-32 group">
            <div className={`grid md:grid-cols-2 gap-24 items-center ${i % 2 === 1 ? 'md:flex-row-reverse' : ''}`}>
               <div className={`${i % 2 === 1 ? 'md:order-2' : ''} relative`}>
                  <div className="text-gold text-[10px] tracking-[0.4em] uppercase font-bold mb-6">{f.tag}</div>
                  <h2 className="text-6xl md:text-8xl font-serif text-white mb-4 leading-none">{f.title}</h2>
                  <h3 className="text-2xl font-serif text-gold/60 italic mb-8">{f.subtitle}</h3>
                  <p className="text-xl text-foreground/40 font-light leading-relaxed mb-12 max-w-lg">
                    {f.desc}
                  </p>
                  <div className="w-24 h-[1px] bg-gold/30"></div>
               </div>
               
               <div className={`relative aspect-[4/5] rounded-[4rem] overflow-hidden shadow-2xl border border-white/5 transform group-hover:scale-[1.02] transition-transform duration-1000 ${i % 2 === 1 ? 'md:order-1' : ''}`}>
                  <Image src={f.img} alt={f.title} fill className="object-cover group-hover:scale-110 transition-transform duration-[2000ms]" />
                  <div className="absolute inset-0 bg-gradient-to-t from-background via-transparent to-transparent"></div>
               </div>
            </div>
          </div>
        ))}
      </section>

      {/* Statistics Section */}
      <section className="py-32 bg-white/[0.02] border-y border-gold/10 relative overflow-hidden">
        <div className="max-w-7xl mx-auto px-6 grid md:grid-cols-3 gap-24 relative z-10 text-center">
          <div>
            <div className="text-8xl font-serif text-gold mb-4 tracking-tighter">0.1s</div>
            <div className="text-xs tracking-[0.3em] uppercase text-foreground/40 font-bold">Latency across ecosystem</div>
          </div>
          <div>
            <div className="text-8xl font-serif text-gold mb-4 tracking-tighter">100%</div>
            <div className="text-xs tracking-[0.3em] uppercase text-foreground/40 font-bold">Bespoke Customization</div>
          </div>
          <div>
            <div className="text-8xl font-serif text-gold mb-4 tracking-tighter">∞</div>
            <div className="text-xs tracking-[0.3em] uppercase text-foreground/40 font-bold">Unforgettable Memories</div>
          </div>
        </div>
      </section>

      {/* Final CTA */}
      <section className="py-48 text-center px-6">
        <h2 className="font-serif text-6xl md:text-9xl text-white mb-16 tracking-tighter">Ready to <span className="text-gold italic">Ascend?</span></h2>
        <div className="flex flex-col md:flex-row gap-8 justify-center items-center">
          <Link href="/fleet" className="px-16 py-6 bg-gold text-background font-bold rounded-full text-sm tracking-widest uppercase hover:scale-105 transition-transform shadow-2xl">
            Explore the Fleet
          </Link>
          <Link href="/" className="px-16 py-6 border border-white/10 text-white font-bold rounded-full text-sm tracking-widest uppercase hover:bg-white/5 transition-all">
            Contact Concierge
          </Link>
        </div>
      </section>

      {/* Bottom Visual */}
      <div className="h-64 relative overflow-hidden">
         <div className="absolute inset-0 bg-gradient-to-t from-background to-transparent z-10"></div>
         <Image src="/experience.png" alt="Experience" fill className="object-cover opacity-20" />
      </div>
    </main>
  );
}
