import { NextResponse } from "next/server";
import { stripe } from "@/lib/stripe";
import { prisma } from "@/lib/prisma";
import { getServerSession } from "next-auth";
import { authOptions } from "@/lib/auth";

export async function POST(req: Request) {
  try {
    const session = await getServerSession(authOptions);

    if (!session?.user) {
      return NextResponse.json({ error: "Please login to book a voyage." }, { status: 401 });
    }

    if (!stripe) {
      return NextResponse.json({ error: "Payment system is not configured. Please contact support." }, { status: 503 });
    }

    const { yachtId, startDate, endDate, guests, promoCode, tourType } = await req.json();
    
    const yacht = await prisma.yacht.findUnique({
      where: { id: yachtId },
    });

    if (!yacht) {
      return NextResponse.json({ error: "Yacht not found" }, { status: 404 });
    }

    // Adjust start and end times based on tour type
    const start = new Date(startDate);
    const end = new Date(endDate);
    
    if (tourType === 'sunset') {
      start.setHours(16, 0, 0, 0);
      end.setHours(20, 0, 0, 0);
    } else {
      start.setHours(10, 0, 0, 0);
      end.setHours(18, 0, 0, 0);
    }

    // Calculate total price based on tour type
    let totalPrice = tourType === 'sunset' ? yacht.pricePerDay * 0.7 : yacht.pricePerDay;

    // Apply Promo Code discounts
    if (promoCode === "EARLY2025") totalPrice *= 0.85; // 15% off
    if (promoCode === "EXTENDED") totalPrice *= 0.80; // 20% off

    // Check for existing bookings that overlap with requested dates
    const overlappingBookings = await prisma.booking.findMany({
      where: {
        yachtId,
        status: { in: ["CONFIRMED", "EXTERNAL_BLOCK"] },
        OR: [
          {
            startDate: { lte: end },
            endDate: { gte: start },
          },
        ],
      },
    });

    if (overlappingBookings.length > 0) {
      return NextResponse.json({ error: "Yacht is not available for these dates." }, { status: 400 });
    }

    // 1. Create a PENDING booking in the database first
    const dbBooking = await prisma.booking.create({
      data: {
        yachtId,
        startDate: start,
        endDate: end,
        guestCount: parseInt(guests),
        totalPrice: totalPrice,
        status: "PENDING",
        guestId: session.user.id,
      }
    });

    // 2. Create a Stripe Checkout Session
    const stripeSession = await stripe.checkout.sessions.create({
      payment_method_types: ["card"],
      line_items: [
        {
          price_data: {
            currency: "eur",
            product_data: {
              name: `${yacht.name} - ${tourType === 'sunset' ? 'Sunset Tour' : 'Daily Tour'}`,
              description: `Tour on ${startDate} for ${guests} guests.`,
              images: [process.env.NEXT_PUBLIC_APP_URL + yacht.imageUrl],
            },
            unit_amount: Math.round(totalPrice * 100), // Stripe expects amounts in cents
          },
          quantity: 1,
        },
      ],
      mode: "payment",
      success_url: `${process.env.NEXT_PUBLIC_APP_URL}/manage/${dbBooking.id}?success=true`,
      cancel_url: `${process.env.NEXT_PUBLIC_APP_URL}/fleet/${yachtId}?canceled=true`,
      metadata: {
        bookingId: dbBooking.id,
        yachtId,
      },
    });

    return NextResponse.json({ url: stripeSession.url });
  } catch (error: any) {
    console.error("Stripe Checkout Error:", error);
    return NextResponse.json({ error: error.message }, { status: 500 });
  }
}
