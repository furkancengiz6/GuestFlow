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

  const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    setLoading(true);
    setError("");

    const formData = new FormData(e.currentTarget);
    const startDate = formData.get("startDate") as string;
    const endDate = formData.get("endDate") as string;

    // Date Validation
    const now = new Date();
    now.setHours(0, 0, 0, 0);
    const start = new Date(startDate);
    const end = new Date(endDate);

    if (start < now) {
      setError(t.booking.errors.pastDate);
      setLoading(false);
      return;
    }

    if (end <= start) {
      setError(t.booking.errors.invalidRange);
      setLoading(false);
      return;
    }

    const bookingData = {
      yachtId,
      startDate,
      endDate,
      guests: formData.get("guests"),
      promoCode: formData.get("promoCode"),
      pricePerDay
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
          {t.booking.pricePerDay.replace('{price}', mounted ? pricePerDay.toLocaleString(lang === 'tr' ? 'tr-TR' : 'en-US') : pricePerDay.toString())}
        </span>
        <span className="text-sm text-foreground/50 uppercase tracking-widest">{t.booking.perDay}</span>
      </div>

      <form onSubmit={handleSubmit} className="space-y-4">
        {error && <div className="text-[10px] text-red-500 uppercase tracking-widest mb-4 font-bold bg-red-500/10 p-3 rounded-lg border border-red-500/20">{error}</div>}
        <div>
          <label className="text-xs tracking-widest text-foreground/60 uppercase mb-2 block">{t.booking.datesLabel}</label>
          <div className="flex gap-2">
            <input type="date" name="startDate" title={t.booking.datesLabel} required className="w-full bg-surface-dark border border-surface-border rounded-lg p-3 text-sm text-foreground focus:border-gold outline-none transition-colors" />
            <input type="date" name="endDate" title={t.booking.datesLabel} required className="w-full bg-surface-dark border border-surface-border rounded-lg p-3 text-sm text-foreground focus:border-gold outline-none transition-colors" />
          </div>
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
          <p className="text-center text-[10px] text-foreground/40 font-light uppercase tracking-widest">{t.booking.secure}</p>
        </div>
      </form>
    </div>
  );
}
