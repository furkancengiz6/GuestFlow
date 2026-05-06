import { NextResponse } from "next/server";
import { headers } from "next/headers";
import { stripe } from "@/lib/stripe";
import { prisma } from "@/lib/prisma";
import Stripe from "stripe";

const webhookSecret = process.env.STRIPE_WEBHOOK_SECRET;

export async function POST(req: Request) {
  const body = await req.text();
  const signature = (await headers()).get("stripe-signature") as string;

  let event: Stripe.Event;

  try {
    event = stripe.webhooks.constructEvent(body, signature, webhookSecret!);
  } catch (error: any) {
    console.error(`Webhook Error: ${error.message}`);
    return NextResponse.json({ error: error.message }, { status: 400 });
  }

  // Handle successful checkout
  if (event.type === "checkout.session.completed") {
    const session = event.data.object as Stripe.Checkout.Session;
    const metadata = session.metadata;

    try {
      if (!metadata?.bookingId) {
        throw new Error("No bookingId found in session metadata");
      }

      // Update the existing PENDING booking to CONFIRMED
      await prisma.booking.update({
        where: { id: metadata.bookingId },
        data: {
          status: "CONFIRMED",
        },
      });

      console.log(`Booking ${metadata.bookingId} confirmed via Stripe webhook.`);

      console.log(`Booking confirmed for session ${session.id}`);
    } catch (err) {
      console.error("Database error in webhook:", err);
    }
  }

  return NextResponse.json({ received: true });
}
