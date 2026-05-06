"use client";

import { useState } from "react";
import Image from "next/image";
import Link from "next/link";

interface Spot {
  id: string;
  name: string;
  coords: { x: number; y: number };
  description: string;
  image: string;
}

const SPOTS: Spot[] = [
  { id: "aquarium", name: "Aquarium Bay", coords: { x: 20, y: 35 }, description: "Crystal clear water where you can see the seabed up to 30 meters. Perfect for snorkeling.", image: "/dest-bodrum.png" },
  { id: "cleopatra", name: "Cleopatra Beach", coords: { x: 45, y: 60 }, description: "Famous for its unique golden sand and turquoise waters. Legend says Antony brought the sand for Cleopatra.", image: "/dest-marmaris.png" },
  { id: "gobun", name: "Göbün Bay", coords: { x: 75, y: 40 }, description: "A hidden gem in Göcek. Protected from all winds, it's a natural harbor surrounded by pine trees.", image: "/dest-gocek.png" },
  { id: "knidos", name: "Ancient Knidos", coords: { x: 10, y: 75 }, description: "Where the Aegean meets the Mediterranean. Explore ancient ruins while anchored in history.", image: "/dest-bodrum.png" },
];

export default function RouteExplorer() {
  const [selectedSpot, setSelectedSpot] = useState<Spot>(SPOTS[0]);
  const [myRoute, setMyRoute] = useState<string[]>([]);

  const addToRoute = (id: string) => {
    if (!myRoute.includes(id)) {
      setMyRoute([...myRoute, id]);
    }
  };

  return (
    <div className="w-full bg-surface-dark rounded-[3rem] overflow-hidden flex flex-col lg:flex-row border border-surface-border shadow-2xl min-h-[600px]">
      {/* Map Side */}
      <div className="flex-1 relative bg-[#0a0d14] p-12 overflow-hidden group min-h-[400px] lg:min-h-0">
        <div className="absolute inset-0 opacity-20 pointer-events-none">
          <svg width="100%" height="100%" viewBox="0 0 800 600" fill="none" xmlns="http://www.w3.org/2000/svg">
             {/* Stylized Coastline */}
             <path d="M50 100C150 120 200 50 300 80C400 110 450 250 550 220C650 190 700 350 750 380C800 410 700 550 600 520C500 490 350 580 200 540C50 500 20 200 50 100Z" stroke="#c5a059" strokeWidth="0.5" strokeDasharray="4 4" />
          </svg>
        </div>

        <div className="absolute inset-0">
          {SPOTS.map((spot) => (
            <button
              key={spot.id}
              onClick={() => setSelectedSpot(spot)}
              className="absolute z-30 group/pin transform -translate-x-1/2 -translate-y-1/2 transition-all duration-500 hover:scale-125"
              style={{ left: `${spot.coords.x}%`, top: `${spot.coords.y}%` }}
            >
              <div className={`w-6 h-6 md:w-4 md:h-4 rounded-full border-2 transition-all duration-500 ${selectedSpot.id === spot.id ? "bg-gold border-white scale-125 md:scale-150 shadow-[0_0_15px_rgba(197,160,89,0.5)]" : "bg-background border-gold"}`}></div>
              <div className={`absolute top-8 left-1/2 -translate-x-1/2 whitespace-nowrap text-[10px] tracking-widest uppercase font-bold transition-all duration-500 ${selectedSpot.id === spot.id ? "text-gold opacity-100" : "text-foreground/30 opacity-0 group-hover/pin:opacity-100"}`}>
                {spot.name}
              </div>
            </button>
          ))}
          
          {/* Animated Route Lines */}
          <svg className="absolute inset-0 pointer-events-none" width="100%" height="100%">
            {myRoute.length > 1 && myRoute.map((id, index) => {
              if (index === 0) return null;
              const start = SPOTS.find(s => s.id === myRoute[index-1])?.coords;
              const end = SPOTS.find(s => s.id === id)?.coords;
              if (!start || !end) return null;
              return (
                <line 
                  key={index}
                  x1={`${start.x}%`} y1={`${start.y}%`} 
                  x2={`${end.x}%`} y2={`${end.y}%`} 
                  stroke="#c5a059" strokeWidth="2" strokeDasharray="8 8" 
                  className="animate-[shimmer_2s_linear_infinite]"
                />
              );
            })}
          </svg>
        </div>

        <div className="absolute top-6 left-6 md:top-12 md:left-12 z-20 pointer-events-none bg-[#0a0d14]/60 backdrop-blur-xl p-6 rounded-3xl border border-white/5 shadow-2xl">
           <div className="text-gold tracking-[0.3em] uppercase text-[10px] mb-2 font-bold">Riviera Explorer</div>
           <h3 className="text-3xl font-serif text-white">Discover <span className="text-gold italic">Hidden Bays</span></h3>
        </div>
      </div>

      {/* Info Side */}
      <div className="w-full lg:w-[450px] bg-surface p-12 flex flex-col justify-between border-l border-surface-border">
        <div className="animate-reveal" key={selectedSpot.id}>
          <div className="relative aspect-video w-full rounded-2xl overflow-hidden mb-8 shadow-xl">
            <Image src={selectedSpot.image} alt={selectedSpot.name} fill className="object-cover" />
          </div>
          <h4 className="text-4xl font-serif text-white mb-4">{selectedSpot.name}</h4>
          <p className="text-foreground/50 font-light leading-relaxed mb-8">
            {selectedSpot.description}
          </p>
          <button 
            onClick={() => addToRoute(selectedSpot.id)}
            className="w-full py-4 bg-white/5 border border-gold/30 text-gold rounded-xl hover:bg-gold hover:text-background transition-all duration-500 uppercase tracking-widest text-[10px] font-bold"
          >
            {myRoute.includes(selectedSpot.id) ? "Added to My Journey" : "+ Add to My Route"}
          </button>
        </div>

        <div className="mt-12 pt-8 border-t border-surface-border">
          <div className="text-[10px] tracking-widest uppercase text-foreground/30 mb-4 font-bold">My Custom Route</div>
          <div className="flex flex-wrap gap-2">
            {myRoute.length === 0 && <div className="text-xs text-foreground/20 italic">Select points on the map to build your journey...</div>}
            {myRoute.map(id => (
              <div key={id} className="px-4 py-2 bg-gold/10 border border-gold/20 rounded-full text-[10px] text-gold uppercase tracking-tighter flex items-center gap-2">
                {SPOTS.find(s => s.id === id)?.name}
                <button onClick={() => setMyRoute(myRoute.filter(mid => mid !== id))} className="hover:text-white">&times;</button>
              </div>
            ))}
          </div>
          {myRoute.length > 0 && (
            <Link href="/fleet" className="block mt-6">
              <button className="w-full py-4 bg-gold text-background font-bold rounded-xl uppercase tracking-widest text-[10px] hover:scale-[1.02] transition-transform">
                Find Yachts for this Route
              </button>
            </Link>
          )}
        </div>
      </div>
    </div>
  );
}
