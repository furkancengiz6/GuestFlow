"use client";

import { useLanguage } from "@/locales/LanguageContext";
import { useState } from "react";

export default function ContactPage() {
  const { t, lang } = useLanguage();
  const [sent, setSent] = useState(false);

  const content = {
    en: {
      tagline: "Global Concierge",
      title: "Contact Our Desk",
      subtitle: "Our curators are available 24/7 to assist with your bespoke maritime journey.",
      form: {
        name: "Name",
        email: "Email",
        subject: "Subject",
        message: "Message",
        submit: "Send Message",
        success: "Message Received. Our desk will contact you shortly."
      },
      office: {
        title: "Headquarters",
        location: "Yalıkavak Marina, Bodrum, TR",
        hours: "Always Open"
      }
    },
    tr: {
      tagline: "Küresel Konsiyerj",
      title: "Bizimle İletişime Geçin",
      subtitle: "Küratörlerimiz, size özel deniz yolculuğunuzda yardımcı olmak için 7/24 hizmetinizdedir.",
      form: {
        name: "İsim",
        email: "E-posta",
        subject: "Konu",
        message: "Mesaj",
        submit: "Mesaj Gönder",
        success: "Mesaj Alındı. Ekibimiz sizinle en kısa sürede iletişime geçecektir."
      },
      office: {
        title: "Merkez Ofis",
        location: "Yalıkavak Marina, Bodrum, TR",
        hours: "Her Zaman Açık"
      }
    }
  };

  const activeContent = content[lang as keyof typeof content] || content.en;

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    setSent(true);
  };

  return (
    <main className="min-h-screen bg-background pt-40 pb-24 px-6">
      <div className="max-w-6xl mx-auto grid grid-cols-1 lg:grid-cols-2 gap-24">
        <div className="animate-reveal">
          <div className="text-gold tracking-[0.4em] uppercase text-xs mb-6 font-bold">{activeContent.tagline}</div>
          <h1 className="font-serif text-6xl md:text-8xl mb-8 tracking-tighter text-white">
            {activeContent.title.split(' ')[0]} <span className="text-gold italic">{activeContent.title.split(' ').slice(1).join(' ')}</span>
          </h1>
          <p className="text-foreground/40 font-light text-xl max-w-md leading-relaxed mb-16">
            {activeContent.subtitle}
          </p>

          <div className="space-y-10">
            <div className="glass p-8 rounded-3xl border border-white/5 max-w-sm">
              <h3 className="text-gold text-[10px] tracking-widest uppercase mb-4 font-bold">{activeContent.office.title}</h3>
              <p className="text-white font-serif text-lg mb-1">{activeContent.office.location}</p>
              <p className="text-foreground/30 text-[10px] uppercase tracking-widest">{activeContent.office.hours}</p>
            </div>
          </div>
        </div>

        <div className="glass p-12 rounded-[4rem] border border-gold/10 relative overflow-hidden group">
          <div className="absolute top-0 right-0 p-12 opacity-[0.03] text-gold pointer-events-none">
            <svg width="200" height="200" viewBox="0 0 24 24" fill="currentColor">
              <path d="M20 4H4c-1.1 0-1.99.9-1.99 2L2 18c0 1.1.9 2 2 2h16c1.1 0 2-.9 2-2V6c0-1.1-.9-2-2-2zm0 4l-8 5-8-5V6l8 5 8-5v2z"/>
            </svg>
          </div>

          {sent ? (
            <div className="h-full flex flex-col items-center justify-center text-center animate-reveal">
              <div className="w-20 h-20 bg-gold/10 rounded-full flex items-center justify-center text-gold mb-8">
                <svg width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
                  <polyline points="20 6 9 17 4 12"></polyline>
                </svg>
              </div>
              <p className="text-white font-serif text-2xl max-w-xs mx-auto italic">{activeContent.form.success}</p>
            </div>
          ) : (
            <form onSubmit={handleSubmit} className="space-y-8">
              <div className="grid grid-cols-2 gap-6">
                <div className="space-y-2">
                  <label className="text-[10px] uppercase tracking-widest text-foreground/40 font-bold ml-2">{activeContent.form.name}</label>
                  <input required type="text" title={activeContent.form.name} className="w-full bg-white/5 border border-white/10 rounded-2xl px-6 py-4 text-white outline-none focus:border-gold transition-all font-light" />
                </div>
                <div className="space-y-2">
                  <label className="text-[10px] uppercase tracking-widest text-foreground/40 font-bold ml-2">{activeContent.form.email}</label>
                  <input required type="email" title={activeContent.form.email} className="w-full bg-white/5 border border-white/10 rounded-2xl px-6 py-4 text-white outline-none focus:border-gold transition-all font-light" />
                </div>
              </div>
              <div className="space-y-2">
                <label className="text-[10px] uppercase tracking-widest text-foreground/40 font-bold ml-2">{activeContent.form.subject}</label>
                <input required type="text" title={activeContent.form.subject} className="w-full bg-white/5 border border-white/10 rounded-2xl px-6 py-4 text-white outline-none focus:border-gold transition-all font-light" />
              </div>
              <div className="space-y-2">
                <label className="text-[10px] uppercase tracking-widest text-foreground/40 font-bold ml-2">{activeContent.form.message}</label>
                <textarea required rows={4} title={activeContent.form.message} className="w-full bg-white/5 border border-white/10 rounded-2xl px-6 py-4 text-white outline-none focus:border-gold transition-all font-light resize-none"></textarea>
              </div>
              <button type="submit" className="w-full bg-gold text-background font-bold py-6 rounded-3xl text-[10px] tracking-[0.4em] uppercase hover:scale-[1.02] active:scale-95 transition-all shadow-2xl">
                {activeContent.form.submit}
              </button>
            </form>
          )}
        </div>
      </div>
    </main>
  );
}
