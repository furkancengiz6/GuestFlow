"use client";

export default function LuxuryLoading() {
  return (
    <div className="min-h-screen bg-background flex flex-col items-center justify-center">
      <div className="relative mb-12">
        <div className="text-6xl font-serif tracking-[0.5em] text-gold animate-pulse">VOY.</div>
        <div className="absolute -inset-8 border border-gold/10 rounded-full animate-spin-slow"></div>
        <div className="absolute -inset-12 border border-gold/5 rounded-full animate-reverse-spin"></div>
      </div>
      <div className="text-[10px] tracking-[0.4em] uppercase text-gold/40 animate-reveal">
        Curating Excellence
      </div>
    </div>
  );
}
