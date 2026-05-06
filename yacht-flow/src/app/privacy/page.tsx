"use client";

import { useLanguage } from "@/locales/LanguageContext";
import Link from "next/link";

export default function PrivacyPage() {
  const { t, lang } = useLanguage();

  const content = {
    en: {
      title: "Privacy Policy",
      updated: "Last Updated: May 2026",
      sections: [
        {
          title: "1. Data Collection",
          content: "We collect personal information necessary to facilitate your bookings, including name, email, and payment details via secure encrypted channels."
        },
        {
          title: "2. Data Usage",
          content: "Your data is used solely to enhance your VOY experience, process transactions, and provide personalized concierge recommendations."
        },
        {
          title: "3. Security",
          content: "We implement industry-standard AES-256 encryption and work with trusted partners like Stripe to ensure your financial data never touches our servers directly."
        },
        {
          title: "4. Your Rights",
          content: "You maintain full control over your data. You may request account deactivation and data deletion at any time via your profile settings."
        }
      ]
    },
    tr: {
      title: "Gizlilik Politikası",
      updated: "Son Güncelleme: Mayıs 2026",
      sections: [
        {
          title: "1. Veri Toplama",
          content: "Ad, e-posta ve ödeme detayları dahil olmak üzere rezervasyonlarınızı kolaylaştırmak için gerekli kişisel bilgileri güvenli şifreli kanallar aracılığıyla topluyoruz."
        },
        {
          title: "2. Veri Kullanımı",
          content: "Verileriniz yalnızca VOY deneyiminizi geliştirmek, işlemleri gerçekleştirmek ve kişiselleştirilmiş konsiyerj önerileri sunmak için kullanılır."
        },
        {
          title: "3. Güvenlik",
          content: "Endüstri standardı AES-256 şifreleme uyguluyoruz ve finansal verilerinizin doğrudan sunucularımıza ulaşmamasını sağlamak için Stripe gibi güvenilir ortaklarla çalışıyoruz."
        },
        {
          title: "4. Haklarınız",
          content: "Verileriniz üzerinde tam kontrole sahipsiniz. Profil ayarlarınız üzerinden istediğiniz zaman hesabınızın kapatılmasını ve verilerinizin silinmesini talep edebilirsiniz."
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
