"use client";

import { useRouter, useSearchParams } from "next/navigation";
import { useState, useEffect } from "react";
import { useLanguage } from "@/locales/LanguageContext";

export default function FleetFilters() {
  const { t } = useLanguage();
  const router = useRouter();
  const searchParams = useSearchParams();
  
  const [filters, setFilters] = useState({
    location: searchParams.get("location") || "",
    type: searchParams.get("type") || "",
    guests: searchParams.get("guests") || "",
    sort: searchParams.get("sort") || "newest",
  });

  const updateFilters = (newFilters: any) => {
    const updated = { ...filters, ...newFilters };
    setFilters(updated);
    
    const params = new URLSearchParams();
    if (updated.location) params.set("location", updated.location);
    if (updated.type) params.set("type", updated.type);
    if (updated.guests) params.set("guests", updated.guests);
    if (updated.sort) params.set("sort", updated.sort);
    
    router.push(`/fleet?${params.toString()}`, { scroll: false });
  };

  return (
    <div className="glass p-8 rounded-[2.5rem] sticky top-28 border border-gold/10 shadow-2xl">
      <h3 className="font-serif text-2xl mb-8 border-b border-surface-border/50 pb-4">
        {t.filters.title.split(' ')[0]} <span className="text-gold italic">{t.filters.title.split(' ').slice(1).join(' ')}</span>
      </h3>
      
      {/* Location Filter */}
      <div className="mb-8">
        <label className="text-[10px] tracking-[0.3em] text-gold uppercase mb-4 block font-bold">{t.filters.locationLabel}</label>
        <select 
          value={filters.location}
          onChange={(e) => updateFilters({ location: e.target.value })}
          title={t.filters.locationLabel}
          className="w-full bg-white/5 border border-white/10 rounded-xl px-4 py-3 text-sm text-foreground outline-none focus:border-gold transition-all appearance-none"
        >
          <option value="">{t.filters.locations.all}</option>
          <option value="Bodrum">{t.filters.locations.bodrum}</option>
          <option value="Göcek">{t.filters.locations.gocek}</option>
          <option value="Marmaris">{t.filters.locations.marmaris}</option>
        </select>
      </div>

      {/* Vessel Type Filter */}
      <div className="mb-8">
        <label className="text-[10px] tracking-[0.3em] text-gold uppercase mb-4 block font-bold">{t.filters.typeLabel}</label>
        <div className="grid grid-cols-2 gap-2">
          {["Motor Yacht", "Gulet", "Catamaran", "Sailing"].map((type) => (
            <button
              key={type}
              onClick={() => updateFilters({ type: filters.type === type ? "" : type })}
              className={`px-3 py-2 rounded-lg text-[10px] tracking-widest uppercase transition-all border ${filters.type === type ? "bg-gold text-background border-gold font-bold" : "bg-white/5 border-white/10 text-foreground/60 hover:border-gold/50"}`}
            >
              {type}
            </button>
          ))}
        </div>
      </div>

      {/* Capacity Filter */}
      <div className="mb-8">
        <label className="text-[10px] tracking-[0.3em] text-gold uppercase mb-4 block font-bold">{t.filters.guestsLabel}</label>
        <div className="flex gap-2">
          {[2, 6, 12].map((num) => (
            <button
              key={num}
              onClick={() => updateFilters({ guests: filters.guests === num.toString() ? "" : num.toString() })}
              className={`flex-1 py-2 rounded-lg text-[10px] tracking-widest uppercase transition-all border ${filters.guests === num.toString() ? "bg-gold text-background border-gold font-bold" : "bg-white/5 border-white/10 text-foreground/60"}`}
            >
              {num}+
            </button>
          ))}
        </div>
      </div>

      {/* Sorting */}
      <div className="mb-8">
        <label className="text-[10px] tracking-[0.3em] text-gold uppercase mb-4 block font-bold">{t.filters.sortLabel}</label>
        <select 
          value={filters.sort}
          onChange={(e) => updateFilters({ sort: e.target.value })}
          title={t.filters.sortLabel}
          className="w-full bg-white/5 border border-white/10 rounded-xl px-4 py-3 text-sm text-foreground outline-none focus:border-gold transition-all appearance-none"
        >
          <option value="newest">{t.filters.sortOptions.newest}</option>
          <option value="price-asc">{t.filters.sortOptions.priceAsc}</option>
          <option value="price-desc">{t.filters.sortOptions.priceDesc}</option>
        </select>
      </div>

      <button 
        onClick={() => {
          setFilters({ location: "", type: "", guests: "", sort: "newest" });
          router.push("/fleet");
        }}
        className="w-full text-[10px] tracking-widest uppercase text-foreground/30 hover:text-gold transition-colors font-bold"
      >
        {t.filters.reset}
      </button>
    </div>
  );
}
