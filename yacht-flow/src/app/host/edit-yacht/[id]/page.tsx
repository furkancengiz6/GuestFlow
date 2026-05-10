"use client";

import { useState, useEffect, use } from "react";
import { useRouter } from "next/navigation";
import Link from "next/link";
import { useLanguage } from "@/locales/LanguageContext";
import LuxuryLoading from "@/app/components/LuxuryLoading";

export default function EditYachtPage({ params }: { params: Promise<{ id: string }> }) {
  const unwrappedParams = use(params);
  const { t } = useLanguage();
  const router = useRouter();
  const [loading, setLoading] = useState(false);
  const [initialLoading, setInitialLoading] = useState(true);
  const [error, setError] = useState("");

  const [formData, setFormData] = useState({
    name: "",
    type: "Motor Yacht",
    location: "Bodrum",
    length: "",
    guests: "8",
    cabins: "4",
    crew: "3",
    pricePerDay: "",
    description: "",
    imageUrl: "",
    icalUrl: "",
    amenities: "",
  });

  useEffect(() => {
    fetch(`/api/host/yachts/${unwrappedParams.id}`)
      .then(res => {
        if (!res.ok) throw new Error("Failed to load yacht data");
        return res.json();
      })
      .then(data => {
        setFormData({
          name: data.name || "",
          type: data.type || "Motor Yacht",
          location: data.location || "Bodrum",
          length: data.length || "",
          guests: data.guests?.toString() || "8",
          cabins: data.cabins?.toString() || "4",
          crew: data.crew?.toString() || "3",
          pricePerDay: data.pricePerDay?.toString() || "",
          description: data.description || "",
          imageUrl: data.imageUrl || "",
          icalUrl: data.icalUrl || "",
          amenities: Array.isArray(data.amenities) ? data.amenities.map((a: any) => a.name).join(", ") : (data.amenities || ""),
        });
      })
      .catch(err => setError(err.message))
      .finally(() => setInitialLoading(false));
  }, [unwrappedParams.id]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    setError("");

    try {
      const res = await fetch(`/api/host/yachts/${unwrappedParams.id}`, {
        method: "PUT",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(formData),
      });

      if (!res.ok) {
        const data = await res.json();
        throw new Error(data.error || "Failed to update vessel");
      }

      router.push("/host?success=true");
      router.refresh();
    } catch (err: any) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  if (initialLoading) return <LuxuryLoading />;

  return (
    <main className="min-h-screen bg-background pt-32 pb-24 px-6 max-w-5xl mx-auto">
      <div className="mb-16">
        <Link href="/host" className="text-gold text-[10px] tracking-widest uppercase mb-6 inline-block hover:text-white transition-colors">
          {t.addYacht.back}
        </Link>
        <div className="text-gold text-[10px] tracking-[0.4em] uppercase mb-4 font-bold">UPDATE VESSEL</div>
        <h1 className="font-serif text-5xl md:text-6xl mb-4">
          Edit <span className="text-gold italic">Vessel</span>
        </h1>
        <p className="text-foreground/40 font-light max-w-xl">
          Update the details of your vessel to keep your listing accurate.
        </p>
      </div>

      {error && (
        <div className="bg-red-500/10 border border-red-500/20 text-red-500 text-[10px] uppercase tracking-widest p-5 rounded-2xl mb-12 font-bold animate-reveal">
          {error}
        </div>
      )}

      <form onSubmit={handleSubmit} className="space-y-12">
        {/* Basic Info Section */}
        <section className="glass p-10 rounded-[3rem] border border-gold/10">
          <h2 className="text-[10px] tracking-[0.3em] uppercase text-gold font-bold mb-8 flex items-center gap-4">
            {t.addYacht.sections.essential} <span className="h-[1px] flex-1 bg-gold/10"></span>
          </h2>
          
          <div className="grid grid-cols-1 md:grid-cols-2 gap-8">
            <div className="space-y-2">
              <label className="text-[9px] tracking-widest text-foreground/40 uppercase font-bold ml-1">{t.addYacht.labels.name}</label>
              <input 
                required
                type="text" 
                placeholder={t.addYacht.placeholders.name}
                className="w-full bg-surface/30 border border-surface-border rounded-xl px-6 py-4 text-white outline-none focus:border-gold transition-colors font-light"
                value={formData.name}
                onChange={(e) => setFormData({...formData, name: e.target.value})}
              />
            </div>
            <div className="space-y-2">
              <label className="text-[9px] tracking-widest text-foreground/40 uppercase font-bold ml-1">{t.addYacht.labels.type}</label>
              <select 
                title={t.addYacht.labels.type}
                className="w-full bg-surface/30 border border-surface-border rounded-xl px-6 py-4 text-white outline-none focus:border-gold transition-colors font-light appearance-none"
                value={formData.type}
                onChange={(e) => setFormData({...formData, type: e.target.value})}
              >
                <option value="Motor Yacht">Motor Yacht</option>
                <option value="Luxury Gulet">Luxury Gulet</option>
                <option value="Catamaran">Catamaran</option>
                <option value="Sailing Yacht">Sailing Yacht</option>
              </select>
            </div>
            <div className="space-y-2">
              <label className="text-[9px] tracking-widest text-foreground/40 uppercase font-bold ml-1">{t.addYacht.labels.location}</label>
              <select 
                title={t.addYacht.labels.location}
                className="w-full bg-surface/30 border border-surface-border rounded-xl px-6 py-4 text-white outline-none focus:border-gold transition-colors font-light appearance-none"
                value={formData.location}
                onChange={(e) => setFormData({...formData, location: e.target.value})}
              >
                <option value="Bodrum">Bodrum</option>
                <option value="Gocek">Gocek</option>
                <option value="Marmaris">Marmaris</option>
                <option value="Fethiye">Fethiye</option>
              </select>
            </div>
            <div className="space-y-2">
              <label className="text-[9px] tracking-widest text-foreground/40 uppercase font-bold ml-1">{t.addYacht.labels.price}</label>
              <input 
                required
                type="number" 
                placeholder={t.addYacht.placeholders.price}
                className="w-full bg-gold/5 border border-gold/20 rounded-xl px-6 py-4 text-gold outline-none focus:border-gold transition-colors font-serif text-xl"
                value={formData.pricePerDay}
                onChange={(e) => setFormData({...formData, pricePerDay: e.target.value})}
              />
            </div>
            <div className="space-y-2 md:col-span-2">
              <label className="text-[9px] tracking-widest text-foreground/40 uppercase font-bold ml-1">{t.addYacht.labels.icalUrl}</label>
              <input 
                type="text" 
                placeholder={t.addYacht.placeholders.icalUrl}
                className="w-full bg-surface/30 border border-surface-border rounded-xl px-6 py-4 text-white outline-none focus:border-gold transition-colors font-light"
                value={formData.icalUrl}
                onChange={(e) => setFormData({...formData, icalUrl: e.target.value})}
              />
              <p className="text-[8px] text-foreground/20 uppercase tracking-widest ml-1 italic">
                Sync with Airbnb, Booking or other platforms (Optional)
              </p>
            </div>
          </div>
        </section>

        {/* Technical Specs Section */}
        <section className="glass p-10 rounded-[3rem] border border-gold/10">
          <h2 className="text-[10px] tracking-[0.3em] uppercase text-gold font-bold mb-8 flex items-center gap-4">
            {t.addYacht.sections.technical} <span className="h-[1px] flex-1 bg-gold/10"></span>
          </h2>
          
          <div className="grid grid-cols-2 md:grid-cols-4 gap-8">
            <div className="space-y-2">
              <label className="text-[9px] tracking-widest text-foreground/40 uppercase font-bold ml-1">{t.addYacht.labels.length}</label>
              <input 
                required
                type="text" 
                placeholder={t.addYacht.placeholders.length}
                className="w-full bg-surface/30 border border-surface-border rounded-xl px-6 py-4 text-white outline-none focus:border-gold transition-colors font-light text-center"
                value={formData.length}
                onChange={(e) => setFormData({...formData, length: e.target.value})}
              />
            </div>
            <div className="space-y-2">
              <label className="text-[9px] tracking-widest text-foreground/40 uppercase font-bold ml-1">{t.addYacht.labels.guests}</label>
              <input 
                required
                type="number" 
                title={t.addYacht.labels.guests}
                className="w-full bg-surface/30 border border-surface-border rounded-xl px-6 py-4 text-white outline-none focus:border-gold transition-colors font-light text-center"
                value={formData.guests}
                onChange={(e) => setFormData({...formData, guests: e.target.value})}
              />
            </div>
            <div className="space-y-2">
              <label className="text-[9px] tracking-widest text-foreground/40 uppercase font-bold ml-1">{t.addYacht.labels.cabins}</label>
              <input 
                required
                type="number" 
                title={t.addYacht.labels.cabins}
                className="w-full bg-surface/30 border border-surface-border rounded-xl px-6 py-4 text-white outline-none focus:border-gold transition-colors font-light text-center"
                value={formData.cabins}
                onChange={(e) => setFormData({...formData, cabins: e.target.value})}
              />
            </div>
            <div className="space-y-2">
              <label className="text-[9px] tracking-widest text-foreground/40 uppercase font-bold ml-1">{t.addYacht.labels.crew}</label>
              <input 
                required
                type="number" 
                title={t.addYacht.labels.crew}
                className="w-full bg-surface/30 border border-surface-border rounded-xl px-6 py-4 text-white outline-none focus:border-gold transition-colors font-light text-center"
                value={formData.crew}
                onChange={(e) => setFormData({...formData, crew: e.target.value})}
              />
            </div>
          </div>
        </section>

        {/* Presentation Section */}
        <section className="glass p-10 rounded-[3rem] border border-gold/10">
          <h2 className="text-[10px] tracking-[0.3em] uppercase text-gold font-bold mb-8 flex items-center gap-4">
            {t.addYacht.sections.presentation} <span className="h-[1px] flex-1 bg-gold/10"></span>
          </h2>
          
          <div className="space-y-8">
            <div className="space-y-2">
              <label className="text-[9px] tracking-widest text-foreground/40 uppercase font-bold ml-1">{t.addYacht.labels.imageUrl}</label>
              <input 
                required
                type="text" 
                placeholder={t.addYacht.placeholders.imageUrl}
                className="w-full bg-surface/30 border border-surface-border rounded-xl px-6 py-4 text-white outline-none focus:border-gold transition-colors font-light"
                value={formData.imageUrl}
                onChange={(e) => setFormData({...formData, imageUrl: e.target.value})}
              />
            </div>
            <div className="space-y-2">
              <label className="text-[9px] tracking-widest text-foreground/40 uppercase font-bold ml-1">{t.addYacht.labels.desc}</label>
              <textarea 
                required
                rows={5}
                placeholder={t.addYacht.placeholders.desc}
                className="w-full bg-surface/30 border border-surface-border rounded-xl px-6 py-4 text-white outline-none focus:border-gold transition-colors font-light resize-none"
                value={formData.description}
                onChange={(e) => setFormData({...formData, description: e.target.value})}
              />
            </div>
            <div className="space-y-2">
              <label className="text-[9px] tracking-widest text-foreground/40 uppercase font-bold ml-1">{t.addYacht.labels.amenities}</label>
              <input 
                type="text" 
                placeholder={t.addYacht.placeholders.amenities}
                className="w-full bg-surface/30 border border-surface-border rounded-xl px-6 py-4 text-white outline-none focus:border-gold transition-colors font-light"
                value={formData.amenities}
                onChange={(e) => setFormData({...formData, amenities: e.target.value})}
              />
            </div>
          </div>
        </section>

        <button 
          disabled={loading}
          className="w-full bg-gold text-background font-bold py-6 rounded-[2rem] text-xs tracking-[0.4em] uppercase hover:bg-gold-hover transition-all shadow-2xl disabled:opacity-50 hover:scale-[1.02] active:scale-95"
        >
          {loading ? "SAVING..." : "UPDATE VESSEL"}
        </button>
      </form>
    </main>
  );
}
