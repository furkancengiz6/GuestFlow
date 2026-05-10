"use client";

import { useEffect, useState } from "react";
import { useLanguage } from "@/locales/LanguageContext";

interface WeatherData {
  location: string;
  current: {
    temp: number;
    windSpeed: number;
    windDirection: number;
    waveHeight: number;
    weatherCode: number;
    isDay: number;
  };
}

export default function WeatherWidget() {
  const { t } = useLanguage();
  const [data, setData] = useState<WeatherData | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    async function fetchWeather() {
      try {
        const res = await fetch("/api/weather");
        if (res.ok) {
          const json = await res.json();
          setData(json);
        }
      } catch (err) {
        console.error("Weather error:", err);
      } finally {
        setLoading(false);
      }
    }
    fetchWeather();
  }, []);

  if (loading) return (
    <div className="glass p-6 rounded-3xl animate-pulse flex gap-4 items-center">
      <div className="w-12 h-12 rounded-full bg-gold/20"></div>
      <div className="space-y-2">
        <div className="h-2 w-24 bg-gold/10 rounded"></div>
        <div className="h-4 w-32 bg-gold/10 rounded"></div>
      </div>
    </div>
  );

  if (!data) return null;

  return (
    <div className="glass-gold p-8 rounded-[2.5rem] border border-gold/20 flex flex-col md:flex-row items-center gap-12 group hover:bg-gold/5 transition-all duration-700">
      <div className="flex items-center gap-6">
        <div className="relative">
          <div className="text-5xl animate-float">
             {data.current.weatherCode < 3 ? "☀️" : data.current.weatherCode < 50 ? "☁️" : "🌧️"}
          </div>
          {data.current.isDay === 1 && (
            <div className="absolute -top-1 -right-1 w-3 h-3 bg-gold rounded-full animate-ping"></div>
          )}
        </div>
        <div>
          <div className="text-[10px] tracking-[0.3em] uppercase text-gold font-bold mb-1">Bodrum Riviera</div>
          <div className="text-3xl font-serif text-white">{data.current.temp}°C</div>
        </div>
      </div>

      <div className="h-12 w-[1px] bg-gold/20 hidden md:block"></div>

      <div className="grid grid-cols-2 gap-12 w-full md:w-auto">
        <div className="flex flex-col">
          <span className="text-[9px] tracking-widest uppercase text-foreground/30 font-bold mb-2">Sea State</span>
          <div className="flex items-end gap-2">
            <span className="text-2xl font-serif text-white">{data.current.waveHeight}</span>
            <span className="text-[10px] text-gold mb-1">m</span>
          </div>
        </div>
        <div className="flex flex-col">
          <span className="text-[9px] tracking-widest uppercase text-foreground/30 font-bold mb-2">Wind Speed</span>
          <div className="flex items-end gap-2">
            <span className="text-2xl font-serif text-white">{data.current.windSpeed}</span>
            <span className="text-[10px] text-gold mb-1">km/h</span>
          </div>
        </div>
      </div>

      <div className="flex-1 text-right hidden lg:block">
        <p className="text-xs text-foreground/40 font-light italic leading-relaxed">
          "Perfect conditions for a <br /> voyage through the turquoise bays."
        </p>
      </div>
    </div>
  );
}
