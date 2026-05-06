"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import Link from "next/link";
import Image from "next/image";

export default function RegisterPage() {
  const router = useRouter();
  const [role, setRole] = useState<"GUEST" | "HOST">("GUEST");
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  const [formData, setFormData] = useState({
    name: "",
    email: "",
    password: "",
    companyName: "",
    phoneNumber: "",
  });

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    setError("");

    try {
      const res = await fetch("/api/auth/register", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ ...formData, role }),
      });

      const data = await res.json();

      if (!res.ok) throw new Error(data.error || "Something went wrong");

      // Success - redirect to login
      router.push("/login?registered=true");
    } catch (err: any) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-screen bg-background flex">
      {/* Visual Side */}
      <div className="hidden lg:flex w-1/2 relative overflow-hidden">
        <Image 
          src="/hero-bg.png" 
          alt="Luxury Yacht" 
          fill 
          className="object-cover opacity-60"
        />
        <div className="absolute inset-0 bg-gradient-to-r from-background via-transparent to-transparent"></div>
        <div className="relative z-10 p-20 flex flex-col justify-center">
          <div className="text-gold text-xs tracking-[0.5em] uppercase mb-6 font-bold">Join the Circle</div>
          <h1 className="font-serif text-8xl text-white leading-tight mb-8">
            Beyond <br />
            <span className="text-gold italic">Expectations</span>
          </h1>
          <p className="text-foreground/40 text-xl font-light max-w-md leading-relaxed">
            Register to access our exclusive fleet and bespoke nautical experiences across the Turkish Riviera.
          </p>
        </div>
      </div>

      {/* Form Side */}
      <div className="w-full lg:w-1/2 flex flex-col justify-center px-8 sm:px-20 lg:px-32 py-12">
        <div className="max-w-md w-full mx-auto">
          <Link href="/" className="inline-block mb-12">
            <span className="text-3xl font-serif tracking-tighter text-white">VOY<span className="text-gold italic">.</span></span>
          </Link>

          <h2 className="text-4xl font-serif text-white mb-2">Create Account</h2>
          <p className="text-foreground/40 text-sm mb-10 font-light tracking-wide">Enter your details to begin your journey.</p>

          {/* Role Selector */}
          <div className="flex bg-surface/40 p-1 rounded-2xl border border-surface-border mb-10 h-14 items-center">
            <button 
              onClick={() => setRole("GUEST")}
              className={`flex-1 h-full rounded-xl text-[10px] tracking-widest uppercase transition-all duration-500 font-bold ${role === "GUEST" ? "bg-gold text-background shadow-lg" : "text-foreground/30 hover:text-white"}`}
            >
              I am a Traveler
            </button>
            <button 
              onClick={() => setRole("HOST")}
              className={`flex-1 h-full rounded-xl text-[10px] tracking-widest uppercase transition-all duration-500 font-bold ${role === "HOST" ? "bg-gold text-background shadow-lg" : "text-foreground/30 hover:text-white"}`}
            >
              I am a Partner
            </button>
          </div>

          {error && (
            <div className="bg-red-500/10 border border-red-500/20 text-red-500 text-[10px] uppercase tracking-widest p-4 rounded-xl mb-8 font-bold animate-fade-in">
              {error}
            </div>
          )}

          <form onSubmit={handleSubmit} className="space-y-6">
            <div className="space-y-2">
              <label className="text-[10px] tracking-widest text-gold uppercase font-bold ml-1">Full Name</label>
              <input 
                required
                type="text" 
                placeholder="Alexander VOY"
                className="w-full bg-surface/30 border border-surface-border rounded-xl px-6 py-4 text-white outline-none focus:border-gold transition-colors font-light"
                value={formData.name}
                onChange={(e) => setFormData({...formData, name: e.target.value})}
              />
            </div>

            <div className="space-y-2">
              <label className="text-[10px] tracking-widest text-gold uppercase font-bold ml-1">Email Address</label>
              <input 
                required
                type="email" 
                placeholder="voyager@voy.com"
                className="w-full bg-surface/30 border border-surface-border rounded-xl px-6 py-4 text-white outline-none focus:border-gold transition-colors font-light"
                value={formData.email}
                onChange={(e) => setFormData({...formData, email: e.target.value})}
              />
            </div>

            {role === "HOST" && (
              <>
                <div className="space-y-2 animate-fade-in">
                  <label className="text-[10px] tracking-widest text-gold uppercase font-bold ml-1">Yacht Company Name</label>
                  <input 
                    required
                    type="text" 
                    placeholder="VOY Yachting Ltd."
                    className="w-full bg-surface/30 border border-surface-border rounded-xl px-6 py-4 text-white outline-none focus:border-gold transition-colors font-light"
                    value={formData.companyName}
                    onChange={(e) => setFormData({...formData, companyName: e.target.value})}
                  />
                </div>
                <div className="space-y-2 animate-fade-in">
                  <label className="text-[10px] tracking-widest text-gold uppercase font-bold ml-1">Phone Number</label>
                  <input 
                    required
                    type="tel" 
                    placeholder="+90 5XX XXX XX XX"
                    className="w-full bg-surface/30 border border-surface-border rounded-xl px-6 py-4 text-white outline-none focus:border-gold transition-colors font-light"
                    value={formData.phoneNumber}
                    onChange={(e) => setFormData({...formData, phoneNumber: e.target.value})}
                  />
                </div>
              </>
            )}

            <div className="space-y-2">
              <label className="text-[10px] tracking-widest text-gold uppercase font-bold ml-1">Secure Password</label>
              <input 
                required
                type="password" 
                placeholder="••••••••"
                className="w-full bg-surface/30 border border-surface-border rounded-xl px-6 py-4 text-white outline-none focus:border-gold transition-colors font-light"
                value={formData.password}
                onChange={(e) => setFormData({...formData, password: e.target.value})}
              />
            </div>

            <button 
              disabled={loading}
              className="w-full bg-gold text-background font-bold py-5 rounded-2xl text-[10px] tracking-[0.3em] uppercase hover:bg-gold-hover transition-all shadow-2xl mt-4 disabled:opacity-50"
            >
              {loading ? "Creating Account..." : "Join Now"}
            </button>
          </form>

          <p className="mt-12 text-center text-foreground/30 text-[10px] tracking-widest uppercase">
            Already have an account? <Link href="/login" className="text-gold font-bold hover:underline">Sign In</Link>
          </p>
        </div>
      </div>
    </div>
  );
}
