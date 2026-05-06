"use client";

import { useState, Suspense } from "react";
import Image from "next/image";
import Link from "next/link";
import { useLanguage } from "@/locales/LanguageContext";
import { signIn } from "next-auth/react";
import { useRouter, useSearchParams } from "next/navigation";

function LoginContent() {
  const { t } = useLanguage();
  const searchParams = useSearchParams();
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");
  const router = useRouter();

  const registered = searchParams.get("registered");

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    setError("");

    try {
      const result = await signIn("credentials", {
        email,
        password,
        redirect: false,
      });

      if (result?.error) {
        setError("Invalid email or access key.");
      } else {
        router.push("/dashboard");
      }
    } catch (err) {
      setError("An unexpected error occurred.");
    } finally {
      setLoading(false);
    }
  };

  return (
    <main className="min-h-screen relative flex items-center justify-center p-6 overflow-hidden">
      {/* Background with overlay */}
      <div className="absolute inset-0 z-0">
        <Image src="/hero-bg.png" alt="Background" fill className="object-cover opacity-20 scale-105" />
        <div className="absolute inset-0 bg-background/80 backdrop-blur-sm"></div>
      </div>

      <div className="relative z-10 w-full max-w-md animate-reveal">
        <div className="glass p-12 rounded-[3rem] shadow-2xl border border-gold/10">
          <div className="text-center mb-12">
            <Link href="/" className="text-4xl font-serif tracking-tighter text-white hover:text-gold transition-colors inline-block mb-6">
              VOY<span className="text-gold italic">.</span>
            </Link>
            <h2 className="text-xl font-serif text-gold tracking-widest uppercase italic">Elite <span className="not-italic">Access</span></h2>
            <p className="text-[10px] text-foreground/40 mt-3 font-light tracking-[0.2em] uppercase">Authenticate your journey</p>
          </div>

          <form className="space-y-6" onSubmit={handleSubmit}>
            {registered && (
              <div className="bg-gold/10 border border-gold/20 text-gold text-[10px] p-5 rounded-2xl text-center uppercase tracking-[0.2em] font-bold animate-reveal mb-4">
                Welcome to the Circle. Please sign in to begin.
              </div>
            )}
            {error && <div className="bg-red-500/10 border border-red-500/20 text-red-500 text-[10px] p-4 rounded-xl text-center uppercase tracking-widest font-bold">{error}</div>}
            <div>
              <label className="text-[10px] tracking-widest text-gold uppercase mb-2 block font-bold">Email Address</label>
              <input 
                type="email" 
                required
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                placeholder="exclusive@guest.com"
                className="w-full bg-white/5 border border-white/10 rounded-2xl px-6 py-4 text-sm text-white outline-none focus:border-gold transition-all"
              />
            </div>
            <div>
              <label className="text-[10px] tracking-widest text-gold uppercase mb-2 block font-bold">Access Key</label>
              <input 
                type="password" 
                required
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                placeholder="••••••••"
                className="w-full bg-white/5 border border-white/10 rounded-2xl px-6 py-4 text-sm text-white outline-none focus:border-gold transition-all"
              />
            </div>

            <button 
              type="submit"
              disabled={loading}
              className="w-full bg-gold text-background font-bold py-5 rounded-2xl text-[10px] tracking-[0.2em] uppercase hover:bg-gold-hover hover:scale-[1.02] active:scale-95 transition-all shadow-xl mt-4 flex items-center justify-center gap-2"
            >
              {loading ? "Authenticating..." : "Sign In to Voyage"}
            </button>

            <div className="text-center mt-8">
              <p className="text-[10px] text-foreground/40 uppercase tracking-widest font-light">
                New to the Riviera? <Link href="/fleet" className="text-gold hover:text-white transition-colors">Start Journey</Link>
              </p>
            </div>
          </form>
        </div>
      </div>
    </main>
  );
}

export default function LoginPage() {
  return (
    <Suspense fallback={<div className="min-h-screen bg-background flex items-center justify-center text-gold uppercase tracking-widest text-[10px]">Loading Access...</div>}>
      <LoginContent />
    </Suspense>
  );
}
