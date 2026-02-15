import React, { useState } from 'react';
import { loadStripe } from '@stripe/stripe-js';
import {
    Elements,
    CardElement,
    useStripe,
    useElements,
} from '@stripe/react-stripe-js';
import {
    Box,
    Button,
    Typography,
    CircularProgress,
    Alert,
} from '@mui/material';
import env from '../../config/env';

// Initialize Stripe with placeholder key
const stripePromise = loadStripe(env.stripePublishableKey);

interface CheckoutFormProps {
    clientSecret: string;
    onSuccess: (paymentIntent: any) => void;
    onError: (error: string) => void;
    amount: number;
    currency: string;
}

const CheckoutForm: React.FC<CheckoutFormProps> = ({ clientSecret, onSuccess, onError, amount, currency }) => {
    const stripe = useStripe();
    const elements = useElements();
    const [isProcessing, setIsProcessing] = useState(false);

    const handleSubmit = async (event: React.FormEvent) => {
        event.preventDefault();

        if (!stripe || !elements) {
            return;
        }

        setIsProcessing(true);

        const cardElement = elements.getElement(CardElement);

        if (!cardElement) {
            setIsProcessing(false);
            return;
        }

        const { error, paymentIntent } = await stripe.confirmCardPayment(clientSecret, {
            payment_method: {
                card: cardElement,
            },
        });

        if (error) {
            onError(error.message || 'Ödeme işlemi başarısız oldu.');
            setIsProcessing(false);
        } else if (paymentIntent.status === 'succeeded') {
            onSuccess(paymentIntent);
        }
    };

    return (
        <form onSubmit={handleSubmit}>
            <Box sx={{ p: 2, border: '1px solid #ccc', borderRadius: 1, mb: 2 }}>
                <CardElement options={{
                    style: {
                        base: {
                            fontSize: '16px',
                            color: '#424770',
                            '::placeholder': {
                                color: '#aab7c4',
                            },
                        },
                        invalid: {
                            color: '#9e2146',
                        },
                    },
                }} />
            </Box>
            <Button
                type="submit"
                variant="contained"
                fullWidth
                disabled={!stripe || isProcessing}
                sx={{ mt: 2 }}
            >
                {isProcessing ? <CircularProgress size={24} /> : `Ödemeyi Tamamla (${amount} ${currency.toUpperCase()})`}
            </Button>
        </form>
    );
};

interface StripePaymentProps {
    clientSecret: string;
    amount: number;
    currency: string;
    onSuccess: (paymentIntent: any) => void;
    onCancel: () => void;
}

const StripePayment: React.FC<StripePaymentProps> = ({ clientSecret, amount, currency, onSuccess, onCancel }) => {
    const [error, setError] = useState<string | null>(null);

    return (
        <Box sx={{ mt: 2 }}>
            <Typography variant="h6" gutterBottom>
                Kredi Kartı Bilgileri
            </Typography>

            {error && (
                <Alert severity="error" sx={{ mb: 2 }}>
                    {error}
                </Alert>
            )}

            <Elements stripe={stripePromise} options={{ clientSecret }}>
                <CheckoutForm
                    clientSecret={clientSecret}
                    onSuccess={onSuccess}
                    onError={setError}
                    amount={amount}
                    currency={currency}
                />
            </Elements>

            <Button
                fullWidth
                variant="text"
                onClick={onCancel}
                sx={{ mt: 1 }}
            >
                İptal Et
            </Button>
        </Box>
    );
};

export default StripePayment;
