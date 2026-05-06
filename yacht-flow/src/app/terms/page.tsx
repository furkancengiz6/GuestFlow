"use client";

import { useLanguage } from "@/locales/LanguageContext";
import Link from "next/link";

export default function TermsPage() {
  const { t, lang } = useLanguage();

  const content = {
    en: {
      title: "Terms of Service",
      updated: "Last Updated: May 2026",
      sections: [
        {
          title: "1. Acceptance of Terms",
          content: "By accessing and using VOY (the 'Platform'), you agree to be bound by these Terms of Service. If you do not agree, please refrain from using our services."
        },
        {
          title: "2. Scope of Services",
          content: "VOY acts as a premium digital ecosystem connecting luxury yacht owners (Hosts) with discerning travelers (Guests). We provide booking facilitation, secure payments, and concierge support."
        },
        {
          title: "3. Booking & Payments",
          content: "All bookings are subject to availability and host confirmation. Payments are processed securely via Stripe. Cancellation policies vary by vessel and are clearly stated during checkout."
        },
        {
          title: "4. User Conduct",
          content: "Users must provide accurate information, respect maritime laws, and maintain the exclusivity of the VOY ecosystem. Any breach may result in immediate account deactivation."
        }
      ]
    },
    tr: {
      title: "Kullanım Koşulları",
      updated: "Son Güncelleme: Mayıs 2026",
      sections: [
        {
          title: "1. Şartların Kabulü",
          content: "VOY ('Platform') sistemine erişerek ve kullanarak bu Kullanım Koşullarını kabul etmiş sayılırsınız. Kabul etmiyorsanız lütfen hizmetlerimizi kullanmayın."
        },
        {
          title: "2. Hizmet Kapsamı",
          content: "VOY, lüks yat sahiplerini (Ev Sahipleri) seçkin gezginlerle (Misafirler) buluşturan birinci sınıf bir dijital ekosistemdir. Rezervasyon kolaylığı, güvenli ödemeler ve konsiyerj desteği sağlıyoruz."
        },
        {
          title: "3. Rezervasyon ve Ödemeler",
          content: "Tüm rezervasyonlar müsaitlik durumuna ve ev sahibi onayına tabidir. Ödemeler Stripe üzerinden güvenli bir şekilde işlenir. İptal politikaları tekneye göre değişir ve ödeme sırasında belirtilir."
        },
        {
          title: "4. Kullanıcı Davranışı",
          content: "Kullanıcılar doğru bilgi vermeli, denizcilik kanunlarına saygı göstermeli ve VOY ekosisteminin seçkinliğini korumalıdır. Herhangi bir ihlal, hesabın derhal kapatılmasına neden olabilir."
        }
      ]
    }
  };

  const activeContent = content[lang as keyof typeof content] || content.en;

  return (
    <main className="min-h-screen bg-background pt-40 pb-24 px-6">
      <div className="max-w-3xl mx-auto">
        <Link href="/" className="text-gold text-[10px] tracking-widest uppercase mb-12 inline-block hover:text-white transition-colors">
          ← {lang === 'tr' ? 'Ana Sayfa' : 'Back to Home'}
        </Link>
        
        <h1 className="font-serif text-5xl md:text-7xl mb-4 tracking-tighter text-white">
          {activeContent.title.split(' ')[0]} <span className="text-gold italic">{activeContent.title.split(' ').slice(1).join(' ')}</span>
        </h1>
        <p className="text-foreground/30 text-xs tracking-widest uppercase mb-16">{activeContent.updated}</p>

        <div className="space-y-12">
          {activeContent.sections.map((section, idx) => (
            <section key={idx} className="glass p-10 rounded-[2.5rem] border border-white/5">
              <h2 className="text-xl font-serif text-white mb-6">{section.title}</h2>
              <p className="text-foreground/60 leading-relaxed font-light">{section.content}</p>
            </section>
          ))}
        </div>

        <div className="mt-20 pt-10 border-t border-surface-border/20 text-center">
          <p className="text-foreground/30 text-[10px] tracking-[0.2em] uppercase">
            Designed for the Turkish Riviera • VOY Private Collection
          </p>
        </div>
      </div>
    </main>
  );
}
