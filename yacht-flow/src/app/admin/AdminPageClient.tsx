"use client";

import { useLanguage } from "@/locales/LanguageContext";
import AdminClient from "./AdminClient";

interface Host {
  id: string;
  name: string;
  email: string;
  companyName: string;
  phoneNumber: string;
  createdAt: string;
}

export default function AdminPageClient({ pendingHosts }: { pendingHosts: Host[] }) {
  const { t } = useLanguage();

  return (
    <main className="min-h-screen bg-background pt-32 pb-12 px-6 max-w-7xl mx-auto">
      <div className="mb-16">
        <div className="text-gold text-[10px] tracking-[0.4em] uppercase mb-4 font-bold">{t.admin.commandCenter}</div>
        <h1 className="font-serif text-5xl md:text-6xl mb-4">
          {t.admin.approvalsTitle.split(' ')[0]} <span className="text-gold italic">{t.admin.approvalsTitle.split(' ').slice(1).join(' ')}</span>
        </h1>
        <p className="text-foreground/40 font-light max-w-xl">
          {t.admin.approvalsDesc}
        </p>
      </div>

      <div className="glass rounded-[3rem] overflow-x-auto border border-gold/10">
        <table className="w-full text-left border-collapse min-w-[800px]">
          <thead>
            <tr className="bg-surface/50 border-b border-surface-border">
              <th className="px-8 py-6 text-[10px] tracking-widest uppercase text-gold font-bold">{t.admin.table.company}</th>
              <th className="px-8 py-6 text-[10px] tracking-widest uppercase text-gold font-bold">{t.admin.table.contact}</th>
              <th className="px-8 py-6 text-[10px] tracking-widest uppercase text-gold font-bold">{t.admin.table.regDate}</th>
              <th className="px-8 py-6 text-[10px] tracking-widest uppercase text-gold font-bold text-right">{t.admin.table.actions}</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-surface-border/50">
            {pendingHosts.length === 0 ? (
              <tr>
                <td colSpan={4} className="px-8 py-20 text-center text-foreground/30 italic font-light">
                  {t.admin.table.noPending}
                </td>
              </tr>
            ) : (
              <AdminClient hosts={pendingHosts} />
            )}
          </tbody>
        </table>
      </div>
    </main>
  );
}
