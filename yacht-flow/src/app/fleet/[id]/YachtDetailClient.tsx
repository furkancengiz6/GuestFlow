"use client";

import Image from "next/image";
import Link from "next/link";
import BookingForm from "./BookingForm";
import LuxuryConfigurator from "@/app/components/LuxuryConfigurator";
import { useLanguage } from "@/locales/LanguageContext";

interface YachtDetailClientProps {
  yacht: any;
}

export default function YachtDetailClient({ yacht }: YachtDetailClientProps) {
  const { t, lang } = useLanguage();
  const amenitiesList = yacht.amenities;

  // Localized Labels
  const labels = {
    en: {
      collection: "Our Collection",
      length: "Length",
      guests: "Guests",
      cabins: "Cabins",
      crew: "Crew",
      experience: "The Experience",
      amenities: "Elite Amenities",
      reviewsTitle: "Verified Voyages",
      noReviews: "This vessel is awaiting its first verified review from our elite collection.",
      assistance: "Need Assistance?",
      assistanceDesc: "Our 24/7 Elite Concierge is available for custom requests.",
      contact: "Contact Concierge"
    },
    tr: {
      collection: "Koleksiyonumuz",
      length: "Uzunluk",
      guests: "Misafir",
      cabins: "Kabin",
      crew: "Mürettebat",
      experience: "Deneyim",
      amenities: "Seçkin Olanaklar",
      reviewsTitle: "Doğrulanmış Yolculuklar",
      noReviews: "Bu tekne, seçkin koleksiyonumuzdan ilk doğrulanmış yorumunu bekliyor.",
      assistance: "Yardıma mı İhtiyacınız Var?",
      assistanceDesc: "7/24 Elite Konsiyerj ekibimiz özel talepleriniz için hazırdır.",
      contact: "Konsiyerj ile İletişime Geç"
    }
  };

  const activeLabels = labels[lang as keyof typeof labels] || labels.en;

  return (
    <main className="min-h-screen bg-background">
      {/* Immersive Hero */}
      <section className="relative h-[60vh] md:h-[85vh] w-full flex items-end pb-12 px-6 overflow-hidden">
        <div className="absolute inset-0 z-0">
          <Image 
            src={yacht.imageUrl} 
            alt={yacht.name} 
            fill 
            className="object-cover opacity-80 animate-reveal"
            priority
          />
          <div className="absolute inset-0 bg-gradient-to-t from-background via-background/40 to-transparent"></div>
        </div>
        
        <div className="relative z-10 w-full max-w-7xl mx-auto flex justify-between items-end">
          <div className="opacity-0 animate-reveal">
            <Link href="/fleet" className="text-gold hover:text-white flex items-center gap-2 text-[10px] md:text-xs uppercase tracking-[0.3em] mb-4 md:mb-8 transition-all hover:tracking-[0.4em]">
              <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M15 19l-7-7 7-7"></path></svg>
              {activeLabels.collection}
            </Link>
            <div className="text-gold tracking-[0.4em] uppercase text-[9px] md:text-xs mb-2 md:mb-4 font-bold">{yacht.type}</div>
            <h1 className="font-serif text-4xl md:text-9xl mb-4 md:mb-6 tracking-tighter leading-none">{yacht.name}</h1>
            <p className="text-base md:text-xl flex items-center gap-3 text-foreground/60 font-light">
               <span className="w-8 h-[1px] bg-gold"></span>
              {yacht.location}
            </p>
          </div>
        </div>
      </section>

      {/* Content & Booking Form */}
      <section className="max-w-7xl mx-auto px-6 py-16 md:py-32 flex flex-col lg:flex-row gap-16 lg:gap-24">
        {/* Main Details */}
        <div className="flex-1 opacity-0 animate-reveal stagger-1">
          <div className="grid grid-cols-2 md:grid-cols-4 gap-8 md:gap-12 mb-16 md:mb-24 py-8 md:py-12 border-y border-surface-border/50">
            <div>
              <div className="text-[9px] md:text-[10px] text-gold uppercase tracking-[0.3em] mb-1 md:mb-2 font-bold">{activeLabels.length}</div>
              <div className="text-2xl md:text-3xl font-serif">{yacht.length}</div>
            </div>
            <div>
              <div className="text-[9px] md:text-[10px] text-gold uppercase tracking-[0.3em] mb-1 md:mb-2 font-bold">{activeLabels.guests}</div>
              <div className="text-2xl md:text-3xl font-serif">{yacht.guests}</div>
            </div>
            <div>
              <div className="text-[9px] md:text-[10px] text-gold uppercase tracking-[0.3em] mb-1 md:mb-2 font-bold">{activeLabels.cabins}</div>
              <div className="text-2xl md:text-3xl font-serif">{yacht.cabins}</div>
            </div>
            <div>
              <div className="text-[9px] md:text-[10px] text-gold uppercase tracking-[0.3em] mb-1 md:mb-2 font-bold">{activeLabels.crew}</div>
              <div className="text-2xl md:text-3xl font-serif">{yacht.crew}</div>
            </div>
          </div>

          <div className="mb-16 md:mb-24">
            <h2 className="font-serif text-3xl md:text-5xl mb-8 md:mb-12">
              {activeLabels.experience.split(' ')[0]} <span className="text-gold italic">{activeLabels.experience.split(' ').slice(1).join(' ')}</span>
            </h2>
            <p className="text-foreground/50 font-light leading-[1.8] mb-12 text-base md:text-xl whitespace-pre-line">
              {yacht.description}
            </p>
          </div>

          <div>
            <h3 className="font-serif text-2xl md:text-3xl mb-8 md:mb-12">{activeLabels.amenities}</h3>
            <div className="grid grid-cols-1 sm:grid-cols-2 gap-6 md:gap-8">
              {amenitiesList.map((item: any) => (
                <div key={item.id} className="flex items-center gap-4 md:gap-6 group">
                  <div className="w-2.5 h-2.5 rounded-full border border-gold group-hover:bg-gold transition-all duration-500"></div>
                  <span className="text-foreground/70 text-base md:text-lg font-light group-hover:text-white transition-colors">{item.name}</span>
                </div>
              ))}
            </div>
          </div>

          {/* Verified Voyages Section */}
          <div className="mt-24 md:mt-32">
            <h3 className="font-serif text-3xl md:text-5xl mb-12 md:mb-16">
              {activeLabels.reviewsTitle.split(' ')[0]} <span className="text-gold italic">{activeLabels.reviewsTitle.split(' ').slice(1).join(' ')}</span>
            </h3>
            
            {yacht.reviews.length > 0 ? (
              <div className="grid grid-cols-1 md:grid-cols-2 gap-8 md:gap-12">
                {yacht.reviews.map((review: any, i: number) => (
                  <div key={review.id} className="glass p-10 rounded-[3rem] border border-gold/10 hover:border-gold/30 transition-all duration-700">
                    <div className="flex gap-1 mb-6">
                      {[...Array(5)].map((_, star) => (
                        <span key={star} className={star < review.rating ? "text-gold text-lg" : "text-foreground/10 text-lg"}>★</span>
                      ))}
                    </div>
                    <p className="text-foreground/60 font-light italic leading-relaxed mb-8 text-lg">"{review.comment}"</p>
                    <div className="flex items-center gap-4">
                      <div className="w-12 h-12 rounded-full bg-gold/20 flex items-center justify-center font-serif text-gold text-xl border border-gold/30">
                        {review.user.name?.[0] || "G"}
                      </div>
                      <div>
                        <div className="text-sm font-bold text-white tracking-widest uppercase">{review.user.name || "Elite Guest"}</div>
                        <div className="text-[10px] text-foreground/40 uppercase tracking-widest">{new Date(review.createdAt).toLocaleDateString(lang === 'tr' ? 'tr-TR' : 'en-US', { month: 'long', year: 'numeric' })}</div>
                      </div>
                    </div>
                  </div>
                ))}
              </div>
            ) : (
              <div className="p-20 text-center glass rounded-[3rem] border border-gold/5 italic text-foreground/30 font-light">
                {activeLabels.noReviews}
              </div>
            )}
          </div>
        </div>

        {/* Sticky Booking Widget */}
        <aside className="w-full lg:w-[400px] shrink-0 opacity-0 animate-reveal stagger-2">
          <div className="sticky top-32">
            <BookingForm yachtId={yacht.id} pricePerDay={yacht.pricePerDay} />
            <LuxuryConfigurator yachtName={yacht.name} basePrice={yacht.pricePerDay} />
            
            <div className="mt-12 p-8 rounded-3xl border border-surface-border bg-surface-dark/30 text-center">
              <div className="text-[10px] tracking-[0.3em] uppercase text-gold mb-4 font-bold">{activeLabels.assistance}</div>
              <p className="text-xs text-foreground/40 font-light mb-6">{activeLabels.assistanceDesc}</p>
              <Link href="/contact" className="text-sm text-white hover:text-gold transition-colors underline font-serif">{activeLabels.contact}</Link>
            </div>
          </div>
        </aside>
      </section>
    </main>
  );
}
