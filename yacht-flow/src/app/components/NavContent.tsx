"use client";

import Link from "next/link";
import { useLanguage } from "@/locales/LanguageContext";
import { useEffect, useState } from "react";
import { useSession, signOut } from "next-auth/react";

export default function NavContent() {
  const { t, lang, setLang } = useLanguage();
  const { data: session } = useSession();
  const [scrolled, setScrolled] = useState(false);
  const [mobileMenuOpen, setMobileMenuOpen] = useState(false);

  const userRole = session?.user?.role;

  useEffect(() => {
    const handleScroll = () => setScrolled(window.scrollY > 50);
    window.addEventListener("scroll", handleScroll);
    return () => window.removeEventListener("scroll", handleScroll);
  }, []);

  const links = [
    { name: t.nav.destinations, href: '/#destinations' },
    { name: t.nav.experiences, href: '/#experiences' },
    { name: t.nav.contact || "Contact", href: '/#contact' },
  ];

  if (session) {
    if (userRole === "ADMIN") links.push({ name: t.nav.admin, href: "/admin" });
    if (userRole === "HOST" || userRole === "ADMIN") links.push({ name: t.nav.host, href: "/host" });
    links.push({ name: t.nav.myVoyages, href: "/dashboard" });
    links.push({ name: t.nav.profile, href: "/profile" });
  }

  return (
    <>
      <nav className={`fixed w-full z-50 transition-all duration-700 px-6 md:px-12 py-6 flex justify-between items-center ${scrolled || mobileMenuOpen ? "bg-background/95 backdrop-blur-2xl py-4 border-b border-surface-border/30" : "bg-transparent"}`}>
        <Link href="/" className="text-4xl font-serif tracking-tighter text-foreground hover:text-gold transition-all z-50 group">
          VOY<span className="text-gold italic group-hover:pl-1 transition-all">.</span>
        </Link>
        
        {/* Desktop Menu */}
        <div className="hidden lg:flex gap-12 text-[10px] tracking-[0.3em] uppercase text-foreground/60 font-bold items-center">
          {links.map((link) => (
            <Link key={link.href} href={link.href} className="hover:text-gold transition-all hover:tracking-[0.4em]">{link.name}</Link>
          ))}
        </div>
        
        <div className="flex items-center gap-4 md:gap-8">
          {/* Language Switcher */}
          <div className="flex bg-surface/40 border border-surface-border rounded-full p-1 h-9 items-center">
            <button 
              onClick={() => setLang("en")}
              className={`px-3 md:px-4 h-full rounded-full text-[8px] md:text-[9px] uppercase tracking-widest transition-all ${lang === "en" ? "bg-gold text-background font-black shadow-lg" : "text-foreground/40 hover:text-white"}`}
            >
              EN
            </button>
            <button 
              onClick={() => setLang("tr")}
              className={`px-3 md:px-4 h-full rounded-full text-[8px] md:text-[9px] uppercase tracking-widest transition-all ${lang === "tr" ? "bg-gold text-background font-black shadow-lg" : "text-foreground/40 hover:text-white"}`}
            >
              TR
            </button>
          </div>

          {/* Auth State */}
          {!session ? (
            <div className="hidden lg:flex items-center gap-8">
              <Link href="/register" className="text-[10px] tracking-[0.2em] uppercase text-gold hover:text-white transition-colors font-bold">
                {t.nav.join}
              </Link>
              <Link href="/login" className="text-[10px] tracking-[0.2em] uppercase text-foreground/40 hover:text-gold transition-colors font-bold">
                {t.nav.login}
              </Link>
            </div>
          ) : (
            <div className="hidden lg:flex items-center gap-6">
               <Link href="/profile" className="flex items-center gap-3 group">
                  <div className="w-8 h-8 rounded-full bg-gold/20 flex items-center justify-center text-gold border border-gold/30 group-hover:bg-gold group-hover:text-background transition-all overflow-hidden">
                    {session.user?.image ? (
                      <img src={session.user.image} alt="" className="w-full h-full object-cover" />
                    ) : (
                      <span className="text-[10px] font-bold">{session.user?.name?.charAt(0) || "U"}</span>
                    )}
                  </div>
                  <div className="text-[10px] tracking-[0.1em] uppercase text-foreground/60 group-hover:text-gold transition-colors font-bold">
                    {session.user?.name}
                  </div>
               </Link>
               <button 
                 onClick={() => signOut()}
                 className="text-[10px] tracking-[0.2em] uppercase text-red-400/60 hover:text-red-400 transition-colors font-bold"
               >
                 {t.nav.logout}
               </button>
            </div>
          )}
          
          <Link href="/fleet" className="hidden sm:block">
            <button className="px-8 md:px-10 py-3 md:py-4 bg-gold text-background font-bold hover:bg-gold-hover transition-all duration-500 text-[10px] tracking-[0.2em] uppercase rounded-full shadow-2xl hover:scale-105 active:scale-95">
              {t.nav.bookNow}
            </button>
          </Link>

          {/* Mobile Menu Toggle */}
          <button 
            onClick={() => setMobileMenuOpen(!mobileMenuOpen)}
            className="lg:hidden w-10 h-10 flex flex-col items-center justify-center gap-1.5 z-50"
            aria-label="Toggle mobile menu"
            {...({ "aria-expanded": mobileMenuOpen })}
          >
            <div className={`w-6 h-[1px] bg-white transition-all ${mobileMenuOpen ? "rotate-45 translate-y-2" : ""}`}></div>
            <div className={`w-6 h-[1px] bg-white transition-all ${mobileMenuOpen ? "opacity-0" : ""}`}></div>
            <div className={`w-6 h-[1px] bg-white transition-all ${mobileMenuOpen ? "-rotate-45 -translate-y-2" : ""}`}></div>
          </button>
        </div>
      </nav>

      {/* Mobile Menu Overlay */}
      <div className={`fixed inset-0 z-40 bg-background transition-all duration-700 lg:hidden ${mobileMenuOpen ? "opacity-100 pointer-events-auto translate-y-0" : "opacity-0 pointer-events-none -translate-y-10"}`}>
        <div className="flex flex-col items-center justify-center h-full gap-8 pt-20">
          <Link onClick={() => setMobileMenuOpen(false)} href="/" className="text-2xl font-serif text-foreground/40 hover:text-gold transition-all tracking-[0.2em] uppercase">{t.nav.home || (lang === 'tr' ? 'Ana Sayfa' : 'Home')}</Link>
          
          {links.map((link) => (
            <Link 
              key={link.href} 
              onClick={() => setMobileMenuOpen(false)} 
              href={link.href} 
              className="text-3xl font-serif text-foreground hover:text-gold transition-all"
            >
              {link.name}
            </Link>
          ))}
          
          <div className="h-[1px] w-12 bg-gold/20 my-4"></div>

          {!session ? (
            <div className="flex flex-col items-center gap-6">
              <Link onClick={() => setMobileMenuOpen(false)} href="/register" className="text-[10px] tracking-[0.3em] uppercase text-background bg-gold font-bold px-12 py-4 rounded-full shadow-xl">
                {t.nav.join}
              </Link>
              <Link onClick={() => setMobileMenuOpen(false)} href="/login" className="text-[10px] tracking-[0.3em] uppercase text-gold font-bold border border-gold/30 px-12 py-4 rounded-full hover:bg-gold hover:text-background transition-all">
                {t.nav.login}
              </Link>
            </div>
          ) : (
            <div className="flex flex-col items-center gap-6">
              <Link onClick={() => setMobileMenuOpen(false)} href="/profile" className="text-center group">
                <div className="text-[10px] tracking-[0.3em] uppercase text-gold font-bold mb-1 group-hover:text-white transition-colors">{session.user?.name}</div>
                <div className="text-[8px] tracking-[0.4em] uppercase text-foreground/20 font-bold">{userRole}</div>
              </Link>
              <button 
                onClick={() => {
                  setMobileMenuOpen(false);
                  signOut();
                }}
                className="text-[9px] tracking-[0.2em] uppercase text-red-400 font-bold border border-red-500/10 px-8 py-3 rounded-full hover:bg-red-500/5 transition-all"
              >
                {t.nav.logout}
              </button>
            </div>
          )}
          
          <Link onClick={() => setMobileMenuOpen(false)} href="/fleet" className="mt-4">
            <button className="px-12 py-5 bg-gold text-background font-bold text-xs tracking-[0.2em] uppercase rounded-full shadow-2xl hover:scale-105 transition-transform">
              {t.nav.fleet}
            </button>
          </Link>
        </div>
      </div>
    </>
  );
}
