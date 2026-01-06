/**
 * Calendar export utilities for iCal/ICS format
 */

export interface CalendarEvent {
  uid: string
  summary: string
  description?: string
  location?: string
  startDate: Date
  endDate: Date
  url?: string
  organizer?: {
    name: string
    email: string
  }
  attendees?: Array<{
    name: string
    email: string
  }>
  reminder?: {
    minutes: number
    action: 'DISPLAY' | 'EMAIL' | 'AUDIO'
  }
}

/**
 * Generate iCal/ICS file content
 */
export const generateICS = (events: CalendarEvent[]): string => {
  const lines: string[] = []

  // Header
  lines.push('BEGIN:VCALENDAR')
  lines.push('VERSION:2.0')
  lines.push('PRODID:-//GuestFlow//Guest Management System//EN')
  lines.push('CALSCALE:GREGORIAN')
  lines.push('METHOD:PUBLISH')

  events.forEach((event) => {
    lines.push('BEGIN:VEVENT')
    lines.push(`UID:${event.uid}`)
    lines.push(`SUMMARY:${escapeICS(event.summary)}`)
    
    if (event.description) {
      lines.push(`DESCRIPTION:${escapeICS(event.description)}`)
    }
    
    if (event.location) {
      lines.push(`LOCATION:${escapeICS(event.location)}`)
    }
    
    lines.push(`DTSTART:${formatICSDate(event.startDate)}`)
    lines.push(`DTEND:${formatICSDate(event.endDate)}`)
    lines.push(`DTSTAMP:${formatICSDate(new Date())}`)
    
    if (event.url) {
      lines.push(`URL:${event.url}`)
    }
    
    if (event.organizer) {
      lines.push(`ORGANIZER;CN="${escapeICS(event.organizer.name)}":mailto:${event.organizer.email}`)
    }
    
    if (event.attendees && event.attendees.length > 0) {
      event.attendees.forEach((attendee) => {
        lines.push(`ATTENDEE;CN="${escapeICS(attendee.name)}":mailto:${attendee.email}`)
      })
    }
    
    if (event.reminder) {
      lines.push('BEGIN:VALARM')
      lines.push(`TRIGGER:-PT${event.reminder.minutes}M`)
      lines.push(`ACTION:${event.reminder.action}`)
      if (event.reminder.action === 'DISPLAY') {
        lines.push(`DESCRIPTION:${escapeICS(event.summary)}`)
      }
      lines.push('END:VALARM')
    }
    
    lines.push('END:VEVENT')
  })

  // Footer
  lines.push('END:VCALENDAR')

  return lines.join('\r\n')
}

/**
 * Format date for ICS format (YYYYMMDDTHHmmssZ)
 */
const formatICSDate = (date: Date): string => {
  const year = date.getUTCFullYear()
  const month = String(date.getUTCMonth() + 1).padStart(2, '0')
  const day = String(date.getUTCDate()).padStart(2, '0')
  const hours = String(date.getUTCHours()).padStart(2, '0')
  const minutes = String(date.getUTCMinutes()).padStart(2, '0')
  const seconds = String(date.getUTCSeconds()).padStart(2, '0')
  
  return `${year}${month}${day}T${hours}${minutes}${seconds}Z`
}

/**
 * Escape special characters for ICS format
 */
const escapeICS = (text: string): string => {
  return text
    .replace(/\\/g, '\\\\')
    .replace(/;/g, '\\;')
    .replace(/,/g, '\\,')
    .replace(/\n/g, '\\n')
    .replace(/\r/g, '')
}

/**
 * Download ICS file
 */
export const downloadICS = (events: CalendarEvent[], filename: string = 'calendar.ics'): void => {
  const content = generateICS(events)
  const blob = new Blob([content], { type: 'text/calendar;charset=utf-8' })
  const url = URL.createObjectURL(blob)
  const link = document.createElement('a')
  link.href = url
  link.download = filename
  document.body.appendChild(link)
  link.click()
  document.body.removeChild(link)
  URL.revokeObjectURL(url)
}

/**
 * Create calendar event from transfer
 */
export const createTransferEvent = (transfer: any): CalendarEvent => {
  return {
    uid: `transfer-${transfer.id}@guestflow.com`,
    summary: `Transfer #${transfer.id}`,
    description: `Transfer from ${transfer.pickupAddress} to ${transfer.dropoffAddress}`,
    location: `${transfer.pickupAddress} → ${transfer.dropoffAddress}`,
    startDate: new Date(transfer.transferDate),
    endDate: new Date(new Date(transfer.transferDate).getTime() + 60 * 60 * 1000), // 1 hour
    url: `${window.location.origin}/transfers/${transfer.id}`,
    reminder: {
      minutes: 30,
      action: 'DISPLAY',
    },
  }
}

/**
 * Create calendar event from tour
 */
export const createTourEvent = (tour: any, type: 'city' | 'yacht'): CalendarEvent => {
  const tourType = type === 'city' ? 'Şehir Turu' : 'Yat Turu'
  return {
    uid: `${type}-tour-${tour.id}@guestflow.com`,
    summary: `${tourType} #${tour.id}`,
    description: type === 'city' ? `Şehir turu - ${tour.language}` : `Yat turu - ${tour.yachtName}`,
    location: tour.location || 'Tour Location',
    startDate: new Date(tour.tourDate),
    endDate: new Date(new Date(tour.tourDate).getTime() + (tour.durationHours || 4) * 60 * 60 * 1000),
    url: `${window.location.origin}/tours/${type}/${tour.id}`,
    reminder: {
      minutes: 60,
      action: 'DISPLAY',
    },
  }
}

