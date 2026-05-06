"use client";

import Link from "next/link";
import { useLanguage } from "@/locales/LanguageContext";

export default function Footer() {
  const { t } = useLanguage();

  return (
    <footer className="bg-surface-dark border-t border-surface-border py-16 px-6 md:px-12">
      <div className="max-w-7xl mx-auto grid grid-cols-1 md:grid-cols-4 gap-12">
        <div className="md:col-span-1">
          <div className="text-3xl font-serif tracking-widest text-gold mb-6">VOY<span className="italic">.</span></div>
          <p className="text-sm text-foreground/50 font-light leading-relaxed">
            {t.footer.desc}
          </p>
        </div>
        <div>
          <h4 className="text-xs tracking-widest uppercase text-gold mb-6">{t.footer.explore}</h4>
          <ul className="space-y-4 text-sm text-foreground/70 font-light">
            <li><Link href="/fleet" className="hover:text-gold transition-colors">{t.nav.fleet}</Link></li>
            <li><Link href="/#destinations" className="hover:text-gold transition-colors">{t.nav.destinations}</Link></li>
            <li><Link href="/#experiences" className="hover:text-gold transition-colors">{t.nav.experiences}</Link></li>
            <li><Link href="/host" className="text-gold hover:underline transition-all font-bold">{t.nav.host}</Link></li>
          </ul>
        </div>
        <div>
          <h4 className="text-xs tracking-widest uppercase text-gold mb-6">{t.footer.support}</h4>
          <ul className="space-y-4 text-sm text-foreground/70 font-light">
            <li><Link href="/#contact" className="hover:text-gold transition-colors">{t.nav.contact}</Link></li>
            <li><Link href="/#terms" className="hover:text-gold transition-colors">Terms of Service</Link></li>
            <li><Link href="/#privacy" className="hover:text-gold transition-colors">Privacy Policy</Link></li>
          </ul>
        </div>
        <div>
          <h4 className="text-xs tracking-widest uppercase text-gold mb-6">{t.footer.newsletter}</h4>
          <p className="text-xs text-foreground/50 mb-4">{t.footer.newsletterDesc}</p>
          <div className="flex gap-2">
            <input type="email" placeholder={t.footer.emailPlaceholder} className="bg-background border border-surface-border rounded-lg p-3 text-xs outline-none focus:border-gold w-full" />
            <button className="bg-gold text-background text-xs font-semibold px-4 py-3 rounded-lg hover:bg-gold-hover transition-colors">{t.footer.join}</button>
          </div>
        </div>
      </div>
      <div className="max-w-7xl mx-auto mt-16 pt-8 border-t border-surface-border/30 text-center text-xs text-foreground/30 font-light">
        © 2026 YachtFlow. {t.footer.rights}
      </div>
    </footer>
  );
}
