"use client";

import { useState, useEffect } from "react";
import Image from "next/image";
import { useLanguage } from "@/locales/LanguageContext";

interface Extra {
  id: string;
  name: string;
  price: number;
  icon: string;
}

const EXTRAS: Extra[] = [
  { id: "jetski", name: "Jet-Ski (Day)", price: 450, icon: "🌊" },
  { id: "chef", name: "Private Chef", price: 600, icon: "👨‍🍳" },
  { id: "dj", name: "Live DJ", price: 800, icon: "🎧" },
  { id: "massage", name: "Onboard Massage", price: 300, icon: "💆‍♀️" },
];

export default function LuxuryConfigurator({ yachtName, basePrice }: { yachtName: string, basePrice: number }) {
  const { lang } = useLanguage();
  const [step, setStep] = useState(1);
  const [selectedExtras, setSelectedExtras] = useState<string[]>([]);
  const [menu, setMenu] = useState("mediterranean");
  const [isOpen, setIsOpen] = useState(false);
  const [mounted, setMounted] = useState(false);

  useEffect(() => {
    setMounted(true);
  }, []);

  const totalPrice = basePrice + selectedExtras.reduce((acc, id) => {
    const extra = EXTRAS.find(e => e.id === id);
    return acc + (extra?.price || 0);
  }, 0);

  if (!isOpen) {
    return (
      <button 
        onClick={() => setIsOpen(true)}
        className="w-full mt-6 py-4 border border-gold text-gold font-bold rounded-xl hover:bg-gold hover:text-background transition-all duration-500 uppercase tracking-widest text-xs"
      >
        ✨ Design Your Experience
      </button>
    );
  }

  return (
    <div className="fixed inset-0 z-[100] bg-background/95 backdrop-blur-2xl flex items-center justify-center p-6 animate-reveal">
      <div className="bg-surface border border-surface-border w-full max-w-6xl h-[80vh] rounded-[3rem] overflow-hidden flex flex-col md:flex-row relative shadow-2xl">
        <button aria-label="Close Configurator" onClick={() => setIsOpen(false)} className="absolute top-8 right-8 text-foreground/40 hover:text-white transition-colors z-10">
          <svg className="w-8 h-8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth="1.5" d="M6 18L18 6M6 6l12 12"></path></svg>
        </button>

        {/* Left Side: Visuals */}
        <div className="w-full md:w-1/3 bg-surface-dark p-12 flex flex-col justify-between border-r border-surface-border">
          <div>
            <div className="text-gold tracking-[0.3em] uppercase text-[10px] mb-4 font-bold">Personalizing Your Journey</div>
            <h2 className="text-5xl font-serif text-white mb-6 leading-none">{yachtName}</h2>
            <div className="text-foreground/40 font-light mb-8">Crafting a bespoke maritime experience tailored to your exact desires.</div>
          </div>

          <div className="glass-gold p-8 rounded-3xl">
            <div className="text-xs tracking-widest uppercase text-gold/60 mb-2">Estimated Total</div>
            <div className="text-4xl font-serif text-gold">
              €{mounted ? totalPrice.toLocaleString(lang === 'tr' ? 'tr-TR' : 'en-US') : totalPrice}
            </div>
            <div className="text-[10px] text-foreground/30 mt-2 uppercase tracking-widest">Excludes Fuel & VAT</div>
          </div>
        </div>

        {/* Right Side: Configurator */}
        <div className="flex-1 p-12 overflow-y-auto">
          <div className="flex gap-8 mb-12">
             {[1, 2, 3].map(i => (
               <div key={i} className={`h-1 flex-1 rounded-full transition-all duration-700 ${step >= i ? "bg-gold" : "bg-surface-border"}`}></div>
             ))}
          </div>

          {step === 1 && (
            <div className="animate-reveal">
              <h3 className="text-3xl font-serif mb-8 text-white">Select Your <span className="text-gold italic">Atmosphere</span></h3>
              <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
                {[
                  { id: "mediterranean", name: "Aegean Fresh", desc: "Seafood, olive oil, and local herbs." },
                  { id: "italian", name: "Riviera Italian", desc: "Fine pasta, truffles, and curated wines." },
                  { id: "sushi", name: "Nihon Omotenashi", desc: "World-class sushi and premium sake." }
                ].map(m => (
                  <button 
                    key={m.id}
                    onClick={() => setMenu(m.id)}
                    className={`p-8 rounded-[2rem] border transition-all duration-500 text-left ${menu === m.id ? "border-gold bg-gold/5" : "border-surface-border hover:border-gold/30"}`}
                  >
                    <div className="text-2xl font-serif text-white mb-2">{m.name}</div>
                    <div className="text-sm text-foreground/40 font-light">{m.desc}</div>
                  </button>
                ))}
              </div>
            </div>
          )}

          {step === 2 && (
            <div className="animate-reveal">
              <h3 className="text-3xl font-serif mb-8 text-white">Enhance Your <span className="text-gold italic">Leisure</span></h3>
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                {EXTRAS.map(extra => (
                  <button 
                    key={extra.id}
                    onClick={() => setSelectedExtras(prev => prev.includes(extra.id) ? prev.filter(id => id !== extra.id) : [...prev, extra.id])}
                    className={`p-6 rounded-2xl border flex items-center justify-between transition-all duration-500 ${selectedExtras.includes(extra.id) ? "border-gold bg-gold/5" : "border-surface-border hover:border-gold/30"}`}
                  >
                    <div className="flex items-center gap-4">
                      <span className="text-2xl">{extra.icon}</span>
                      <div className="text-left">
                        <div className="text-white font-serif">{extra.name}</div>
                        <div className="text-xs text-foreground/40 tracking-widest">+€{extra.price}</div>
                      </div>
                    </div>
                    <div className={`w-6 h-6 rounded-full border flex items-center justify-center transition-all ${selectedExtras.includes(extra.id) ? "bg-gold border-gold" : "border-surface-border"}`}>
                      {selectedExtras.includes(extra.id) && <svg className="w-4 h-4 text-background" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth="3" d="M5 13l4 4L19 7"></path></svg>}
                    </div>
                  </button>
                ))}
              </div>
            </div>
          )}

          {step === 3 && (
            <div className="animate-reveal text-center py-12">
              <div className="text-6xl mb-6">🥂</div>
              <h3 className="text-4xl font-serif mb-4 text-white">Your Masterpiece is <span className="text-gold italic">Ready</span></h3>
              <p className="text-foreground/40 font-light max-w-md mx-auto mb-12 leading-relaxed">
                We have saved your luxury configuration. Our Elite Concierge will contact you to finalize the fine details and secure your dates.
              </p>
              <button 
                onClick={() => setIsOpen(false)}
                className="px-12 py-5 bg-gold text-background font-bold rounded-full hover:bg-gold-hover transition-all duration-500 uppercase tracking-widest text-sm shadow-2xl"
              >
                Submit Design
              </button>
            </div>
          )}

          <div className="mt-12 flex justify-between">
            {step > 1 && step < 3 && (
              <button onClick={() => setStep(step - 1)} className="text-foreground/40 hover:text-white transition-colors text-xs uppercase tracking-widest font-bold">
                &larr; Back
              </button>
            )}
            <div className="flex-1"></div>
            {step < 3 && (
              <button 
                onClick={() => setStep(step + 1)}
                className="px-10 py-4 bg-white text-background font-bold rounded-full hover:bg-gold hover:text-background transition-all duration-500 uppercase tracking-widest text-xs shadow-xl"
              >
                Next Step &rarr;
              </button>
            )}
          </div>
        </div>
      </div>
    </div>
  );
}
