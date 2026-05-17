import FleetClient from "./FleetClient";
import { Suspense } from "react";

export default function FleetPage() {
  return (
    <main className="min-h-screen bg-background pt-32 pb-12 px-6 max-w-7xl mx-auto">
      <Suspense fallback={<div className="min-h-screen bg-background flex items-center justify-center text-gold">Loading Collection...</div>}>
        <FleetClient yachts={[]} />
      </Suspense>
    </main>
  );
}
