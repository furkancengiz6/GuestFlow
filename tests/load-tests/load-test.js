// K6 Load Test Script for GuestFlow API
// Run with: k6 run load-test.js

import http from 'k6/http';
import { check, sleep, group } from 'k6';
import { Rate, Trend } from 'k6/metrics';

// Custom metrics
const errorRate = new Rate('errors');
const authTrend = new Trend('auth_duration');
const guestsTrend = new Trend('guests_duration');
const transfersTrend = new Trend('transfers_duration');

// Test configuration
export const options = {
    stages: [
        { duration: '30s', target: 10 },  // Ramp up to 10 users
        { duration: '1m', target: 50 },   // Stay at 50 users
        { duration: '30s', target: 100 }, // Ramp up to 100 users
        { duration: '1m', target: 100 },  // Stay at 100 users (peak)
        { duration: '30s', target: 0 },   // Ramp down
    ],
    thresholds: {
        http_req_duration: ['p(95)<500'],  // 95% of requests should be below 500ms
        errors: ['rate<0.1'],               // Error rate should be below 10%
        auth_duration: ['p(95)<300'],       // Auth should be fast
        guests_duration: ['p(95)<500'],     // Guest operations
        transfers_duration: ['p(95)<500'],  // Transfer operations
    },
};

const BASE_URL = __ENV.BASE_URL || 'http://localhost:5000';

// Test user credentials (from seeded demo data)
const TEST_USER = {
    email: __ENV.TEST_USER || 'demo.admin@guestflow.local',
    password: __ENV.TEST_PASSWORD || 'GuestFlow123!'
};

let authToken = null;

// Setup: Login once and get token
export function setup() {
    const loginRes = http.post(`${BASE_URL}/api/v1.0/Auth/login`, JSON.stringify({
        email: TEST_USER.email,
        password: TEST_USER.password
    }), {
        headers: { 'Content-Type': 'application/json' },
    });

    check(loginRes, {
        'login successful': (r) => r.status === 200,
        'has access token': (r) => r.json('data.accessToken') !== undefined,
    });

    if (loginRes.status === 200) {
        return { token: loginRes.json('data.accessToken') };
    }

    console.error('Login failed:', loginRes.body);
    return { token: null };
}

export default function (data) {
    const params = {
        headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${data.token}`,
        },
    };

    group('Health Check', () => {
        const healthRes = http.get(`${BASE_URL}/health`);
        check(healthRes, {
            'health check OK': (r) => r.status === 200,
        });
        errorRate.add(healthRes.status !== 200);
    });

    group('Guest Operations', () => {
        // List guests
        const start = Date.now();
        const guestsRes = http.get(`${BASE_URL}/api/v1.0/Guests`, params);
        guestsTrend.add(Date.now() - start);

        check(guestsRes, {
            'guests list OK': (r) => r.status === 200,
            'guests response has data': (r) => r.json('data') !== undefined,
        });
        errorRate.add(guestsRes.status !== 200);

        // Get single guest (if available)
        if (guestsRes.status === 200 && guestsRes.json('data.length') > 0) {
            const guestId = guestsRes.json('data.0.id');
            const guestDetailRes = http.get(`${BASE_URL}/api/v1.0/Guests/${guestId}`, params);
            check(guestDetailRes, {
                'guest detail OK': (r) => r.status === 200,
            });
        }
    });

    group('Transfer Operations', () => {
        const start = Date.now();
        const transfersRes = http.get(`${BASE_URL}/api/v1.0/Transfers`, params);
        transfersTrend.add(Date.now() - start);

        check(transfersRes, {
            'transfers list OK': (r) => r.status === 200,
        });
        errorRate.add(transfersRes.status !== 200);
    });

    group('Dashboard', () => {
        const dashboardRes = http.get(`${BASE_URL}/api/v1.0/Dashboard`, params);
        check(dashboardRes, {
            'dashboard OK': (r) => r.status === 200,
        });
        errorRate.add(dashboardRes.status !== 200);
    });

    group('PMS Mock Data', () => {
        // Test Mock PMS adapter if available
        const pmsRes = http.get(`${BASE_URL}/api/v1.0/PMS/integrations`, params);
        check(pmsRes, {
            'PMS integrations list OK': (r) => r.status === 200 || r.status === 404,
        });
    });

    sleep(1); // Think time between iterations
}

export function teardown(data) {
    console.log('Load test completed');
}
