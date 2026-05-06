import type { Metadata } from "next";
import { Inter, Playfair_Display } from "next/font/google";
import "./globals.css";
import Link from "next/link";
import { Providers } from "./components/Providers";
import NavContent from "./components/NavContent";

const inter = Inter({
  variable: "--font-sans",
  subsets: ["latin"],
  display: "swap",
});

const playfair = Playfair_Display({
  variable: "--font-serif",
  subsets: ["latin"],
  display: "swap",
});

export const metadata: Metadata = {
  title: "VOY | Elite Yachting & Charters",
  description: "Experience the ultimate luxury voyage. VOY connects world-class travelers with the most exclusive yacht collection on the Turkish Riviera.",
};

import Footer from "./components/Footer";

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="en" className={`${inter.variable} ${playfair.variable}`}>
      <body className="antialiased">
        <Providers>
          <NavContent />
          {children}
          <Footer />
        </Providers>
      </body>
    </html>
  );
}
