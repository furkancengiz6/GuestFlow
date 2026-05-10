"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import { useLanguage } from "@/locales/LanguageContext";

interface Host {
  id: string;
  name: string;
  email: string;
  companyName: string;
  phoneNumber: string;
  createdAt: string;
}

export default function AdminClient({ hosts }: { hosts: Host[] }) {
  const { t } = useLanguage();
  const router = useRouter();
  const [processing, setProcessing] = useState<string | null>(null);

  const handleApprove = async (userId: string) => {
    setProcessing(userId);
    try {
      const res = await fetch("/api/admin/approve-host", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ userId }),
      });

      if (res.ok) {
        router.refresh(); // Refresh server data
      } else {
        alert(t.admin.approveError);
      }
    } catch (err) {
      console.error(err);
      alert(t.admin.unexpectedError);
    } finally {
      setProcessing(null);
    }
  };

  const handleReject = async (userId: string) => {
    if (!confirm(t.admin.confirmReject)) return;
    setProcessing(userId);
    try {
      const res = await fetch("/api/admin/reject-host", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ userId }),
      });

      if (res.ok) {
        router.refresh();
      } else {
        alert(t.admin.rejectError);
      }
    } catch (err) {
      console.error(err);
      alert(t.admin.unexpectedError);
    } finally {
      setProcessing(null);
    }
  };

  return (
    <>
      {hosts.map((host) => (
        <tr key={host.id} className="hover:bg-white/5 transition-colors group">
          <td className="px-8 py-8">
            <div className="text-white font-serif text-lg">{host.companyName}</div>
            <div className="text-[10px] text-foreground/30 uppercase tracking-widest">{host.name}</div>
          </td>
          <td className="px-8 py-8">
            <div className="text-sm text-foreground/60">{host.email}</div>
            <div className="text-sm text-foreground/40 font-light">{host.phoneNumber}</div>
          </td>
          <td className="px-8 py-8 text-sm text-foreground/40 font-light">
            {host.createdAt}
          </td>
          <td className="px-8 py-8 text-right">
            <div className="flex justify-end gap-3">
              <button 
                onClick={() => handleReject(host.id)}
                disabled={processing === host.id}
                className="bg-transparent border border-red-500/30 text-red-500/50 px-6 py-2 rounded-full text-[10px] tracking-widest uppercase font-bold hover:bg-red-500 hover:text-white hover:border-red-500 transition-all disabled:opacity-50"
              >
                {t.admin.rejectAction}
              </button>
              <button 
                onClick={() => handleApprove(host.id)}
                disabled={processing === host.id}
                className="bg-gold text-background px-6 py-2 rounded-full text-[10px] tracking-widest uppercase font-bold hover:bg-gold-hover transition-all disabled:opacity-50"
              >
                {processing === host.id ? t.admin.processing : t.admin.approveAction}
              </button>
            </div>
          </td>
        </tr>
      ))}
    </>
  );
}
