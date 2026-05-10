import { prisma } from './prisma';

export async function syncYachtCalendar(yachtId: string) {
  const yacht = await prisma.yacht.findUnique({
    where: { id: yachtId },
    select: { id: true, icalUrl: true }
  });

  if (!yacht || !yacht.icalUrl) return;

  try {
    const res = await fetch(yacht.icalUrl);
    if (!res.ok) throw new Error('Failed to fetch calendar');
    const icalData = await res.text();
    
    // Clear existing external bookings for this yacht to avoid duplicates
    await prisma.booking.deleteMany({
      where: {
        yachtId,
        status: "EXTERNAL_BLOCK"
      }
    });

    const bookingsToCreate = [];
    
    // Simple regex parsing for VEVENTs
    const eventRegex = /BEGIN:VEVENT([\s\S]*?)END:VEVENT/g;
    let match;
    
    while ((match = eventRegex.exec(icalData)) !== null) {
      const eventContent = match[1];
      
      const dtStartMatch = eventContent.match(/DTSTART(?:;.*?)?:(.*?)(?:\r\n|\n)/);
      const dtEndMatch = eventContent.match(/DTEND(?:;.*?)?:(.*?)(?:\r\n|\n)/);
      const summaryMatch = eventContent.match(/SUMMARY:(.*?)(?:\r\n|\n)/);
      
      if (dtStartMatch && dtEndMatch) {
        // Basic parsing for YYYYMMDD or YYYYMMDDTHHMMSSZ format
        const parseDateStr = (str: string) => {
          if (str.length >= 8) {
            const year = str.substring(0, 4);
            const month = str.substring(4, 6);
            const day = str.substring(6, 8);
            if (str.length >= 15 && str.includes('T')) {
              const hour = str.substring(9, 11);
              const min = str.substring(11, 13);
              const sec = str.substring(13, 15);
              return new Date(`${year}-${month}-${day}T${hour}:${min}:${sec}Z`);
            }
            return new Date(`${year}-${month}-${day}T00:00:00Z`);
          }
          return new Date(); // fallback
        };
        
        bookingsToCreate.push({
          yachtId,
          startDate: parseDateStr(dtStartMatch[1]),
          endDate: parseDateStr(dtEndMatch[1]),
          status: "EXTERNAL_BLOCK",
          totalPrice: 0,
          guestCount: 0,
          guestId: "SYSTEM",
          specialNotes: `External Sync: ${summaryMatch ? summaryMatch[1] : 'Blocked'}`
        });
      }
    }

    // Since guestId is required in our schema and must exist, we should use a system user
    // or modify the schema. For now, let's find the host or admin to act as the placeholder guest.
    const systemUser = await prisma.user.findFirst({ where: { role: "ADMIN" } });
    
    if (systemUser) {
      for (const booking of bookingsToCreate) {
        await prisma.booking.create({
          data: {
            ...booking,
            guestId: systemUser.id
          }
        });
      }
    }

    console.log(`Synced ${bookingsToCreate.length} events for yacht ${yachtId}`);
  } catch (error) {
    console.error(`Failed to sync calendar for yacht ${yachtId}:`, error);
  }
}
