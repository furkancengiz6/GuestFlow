import Stripe from 'stripe';

const stripeSecretKey = process.env.STRIPE_SECRET_KEY;

// Only initialize stripe when a real key is provided
export const stripe = stripeSecretKey && !stripeSecretKey.startsWith('sk_test_...')
  ? new Stripe(stripeSecretKey, {
      apiVersion: '2025-01-27.acacia' as any,
      typescript: true,
    })
  : null;
