"use client";

import { useState, useEffect } from "react";
import { createBooking } from "@/app/actions/booking";
import { useLanguage } from "@/locales/LanguageContext";

export default function BookingForm({ yachtId, pricePerDay }: { yachtId: string, pricePerDay: number }) {
  const { t, lang } = useLanguage();
  const [mounted, setMounted] = useState(false);

  useEffect(() => {
    setMounted(true);
  }, []);
  const [loading, setLoading] = useState(false);
  const [success, setSuccess] = useState(false);
  const [error, setError] = useState("");
  const [tourType, setTourType] = useState("daily");

  const computedPrice = tourType === "sunset" ? pricePerDay * 0.7 : pricePerDay;

  const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    setLoading(true);
    setError("");

    const formData = new FormData(e.currentTarget);
    const dateStr = formData.get("date") as string;

    // Date Validation
    const now = new Date();
    now.setHours(0, 0, 0, 0);
    const start = new Date(dateStr);

    if (start < now) {
      setError(t.booking.errors.pastDate);
      setLoading(false);
      return;
    }

    // Single day bookings
    const endDateStr = dateStr;

    const bookingData = {
      yachtId,
      startDate: dateStr,
      endDate: endDateStr,
      guests: formData.get("guests"),
      promoCode: formData.get("promoCode"),
      tourType,
      pricePerDay: computedPrice
    };

    try {
      const response = await fetch("/api/checkout", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(bookingData),
      });

      const { url, error: apiError } = await response.json();

      if (apiError) {
        setError(apiError);
        setLoading(false);
      } else if (url) {
        window.location.href = url;
      }
    } catch (err) {
      setError(t.booking.errors.unexpected);
      setLoading(false);
    }
  };

  return (
    <div className="glass p-8 rounded-2xl sticky top-28 border border-gold/20">
      <div className="flex items-baseline gap-2 mb-8 border-b border-surface-border pb-6">
        <span className="text-4xl font-serif text-gold">
          {t.booking.pricePerDay.replace('{price}', mounted ? computedPrice.toLocaleString(lang === 'tr' ? 'tr-TR' : 'en-US') : computedPrice.toString())}
        </span>
        <span className="text-sm text-foreground/50 uppercase tracking-widest">{lang === 'tr' ? 'TUR BAŞINA' : 'PER TOUR'}</span>
      </div>

      <form onSubmit={handleSubmit} className="space-y-4">
        {error && <div className="text-[10px] text-red-500 uppercase tracking-widest mb-4 font-bold bg-red-500/10 p-3 rounded-lg border border-red-500/20">{error}</div>}
        <div>
          <label htmlFor="tour-date" className="text-xs tracking-widest text-foreground/60 uppercase mb-2 block">{lang === 'tr' ? 'Tur Tarihi' : 'Tour Date'}</label>
          <input id="tour-date" title={lang === 'tr' ? 'Tur Tarihi' : 'Tour Date'} type="date" name="date" required className="w-full bg-surface-dark border border-surface-border rounded-lg p-3 text-sm text-foreground focus:border-gold outline-none transition-colors" />
        </div>

        <div>
          <label className="text-xs tracking-widest text-foreground/60 uppercase mb-2 block">{lang === 'tr' ? 'Tur Seçeneği' : 'Tour Option'}</label>
          <div className="grid grid-cols-2 gap-2">
            <button 
              type="button"
              onClick={() => setTourType("daily")}
              className={`py-3 rounded-lg text-[10px] tracking-widest uppercase transition-all border ${tourType === "daily" ? "bg-gold text-background border-gold font-bold" : "bg-white/5 border-white/10 text-foreground/60 hover:border-gold/50"}`}
            >
              {lang === 'tr' ? 'Günlük Tur' : 'Daily Tour'}
            </button>
            <button 
              type="button"
              onClick={() => setTourType("sunset")}
              className={`py-3 rounded-lg text-[10px] tracking-widest uppercase transition-all border flex flex-col items-center justify-center gap-1 ${tourType === "sunset" ? "bg-gold text-background border-gold font-bold" : "bg-white/5 border-white/10 text-foreground/60 hover:border-gold/50"}`}
            >
              <span>{lang === 'tr' ? 'Gün Batımı' : 'Sunset Tour'}</span>
              <span className="text-[8px] opacity-80">-30%</span>
            </button>
          </div>
          <p className="text-[9px] text-foreground/40 mt-2 text-center uppercase tracking-widest">
            {tourType === "daily" ? "10:00 - 18:00" : "16:00 - 20:00"}
          </p>
        </div>

        <div>
          <label className="text-xs tracking-widest text-foreground/60 uppercase mb-2 block">{t.booking.guestsLabel}</label>
          <select name="guests" title={t.booking.guestsLabel} className="w-full bg-surface-dark border border-surface-border rounded-lg p-3 text-sm text-foreground focus:border-gold outline-none transition-colors appearance-none cursor-pointer">
            <option value="2">{t.booking.guestsOption.replace('{num}', '2')}</option>
            <option value="4">{t.booking.guestsOption.replace('{num}', '4')}</option>
            <option value="6">{t.booking.guestsOption.replace('{num}', '6')}</option>
            <option value="8">{t.booking.guestsOptionPlus.replace('{num}', '8')}</option>
          </select>
        </div>

        <div>
          <label className="text-xs tracking-widest text-foreground/60 uppercase mb-2 block">{t.booking.promoLabel}</label>
          <input 
            type="text" 
            name="promoCode" 
            placeholder={t.booking.promoPlaceholder}
            className="w-full bg-surface-dark border border-gold/20 rounded-lg p-3 text-sm text-gold focus:border-gold outline-none transition-colors placeholder:text-gold/30 uppercase tracking-widest font-bold" 
          />
        </div>
        
        <div className="pt-4">
          <button type="submit" disabled={loading} className={`w-full ${loading ? 'bg-gold/50 cursor-not-allowed' : 'bg-gold hover:bg-gold-hover'} text-background font-bold py-5 rounded-xl transition-all shadow-xl text-xs tracking-widest uppercase mb-4`}>
            {loading ? t.booking.loading : t.booking.submit}
          </button>
          <p className="text-center text-[10px] text-foreground/40 font-light uppercase tracking-widest mb-4">{t.booking.secure}</p>
          <div className="text-center pt-4 border-t border-surface-border/50">
             <p className="text-[10px] text-foreground/40 tracking-widest uppercase mb-2">
               {lang === 'tr' ? 'Çoklu gün kiralamak mı istiyorsunuz?' : 'Looking for a multi-day charter?'}
             </p>
             <a href="/contact" className="text-gold text-[10px] tracking-widest uppercase font-bold underline hover:text-white transition-colors">
               {lang === 'tr' ? 'Bizimle İletişime Geçin' : 'Contact Us'}
             </a>
          </div>
        </div>
      </form>
    </div>
  );
}
