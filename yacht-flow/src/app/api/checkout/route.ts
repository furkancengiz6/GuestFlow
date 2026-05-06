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

    const { yachtId, startDate, endDate, guests, pricePerDay, promoCode } = await req.json();
    
    const yacht = await prisma.yacht.findUnique({
      where: { id: yachtId },
    });

    if (!yacht) {
      return NextResponse.json({ error: "Yacht not found" }, { status: 404 });
    }

    // Calculate total days
    const start = new Date(startDate);
    const end = new Date(endDate);
    const diffTime = Math.abs(end.getTime() - start.getTime());
    const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24)) || 1;
    
    let totalPrice = diffDays * yacht.pricePerDay;

    // Apply Promo Code discounts
    if (promoCode === "EARLY2025") totalPrice *= 0.85; // 15% off
    if (promoCode === "EXTENDED") totalPrice *= 0.80; // 20% off

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
              name: `${yacht.name} - Luxury Charter`,
              description: `Booking from ${startDate} to ${endDate} for ${guests} guests.`,
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
