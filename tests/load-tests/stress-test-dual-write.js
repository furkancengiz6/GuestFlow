// K6 Stress Test Script for Dual-Write Scenario (SQL + Neo4j)
// Tests high-volume check-in operations that trigger both SQL and Graph database writes
// Run with: k6 run stress-test-dual-write.js

import http from 'k6/http';
import { check, sleep, group } from 'k6';
import { Rate, Trend, Counter } from 'k6/metrics';

// Custom metrics
const errorRate = new Rate('errors');
const checkInTrend = new Trend('checkin_duration');
const graphWriteTrend = new Trend('graph_write_duration');
const successfulCheckIns = new Counter('successful_checkins');
const failedCheckIns = new Counter('failed_checkins');

// Stress test configuration - simulates peak hotel check-in hours
export const options = {
    scenarios: {
        // Scenario 1: Gradual ramp-up
        gradual: {
            executor: 'ramping-vus',
            startVUs: 0,
            stages: [
                { duration: '1m', target: 20 },   // Morning arrival start
                { duration: '2m', target: 50 },   // Peak check-in time
                { duration: '3m', target: 100 },  // Maximum concurrent check-ins
                { duration: '1m', target: 50 },   // Slowdown
                { duration: '1m', target: 0 },    // End of rush
            ],
            gracefulRampDown: '30s',
        },
        // Scenario 2: Spike test
        spike: {
            executor: 'ramping-vus',
            startVUs: 0,
            stages: [
                { duration: '10s', target: 100 }, // Sudden spike (bus arrival)
                { duration: '30s', target: 100 }, // Sustained spike
                { duration: '10s', target: 0 },   // Recovery
            ],
            startTime: '9m', // Start after gradual scenario
        },
    },
    thresholds: {
        http_req_duration: ['p(95)<1000', 'p(99)<2000'], // Relaxed for dual-write
        errors: ['rate<0.05'],                           // Max 5% error rate
        checkin_duration: ['p(95)<1500'],                // Check-in including graph write
        successful_checkins: ['count>100'],              // At least 100 successful check-ins
    },
};

const BASE_URL = __ENV.BASE_URL || 'http://localhost:5000';

// Test user credentials
const TEST_USER = {
    email: 'admin@guestflow.com',
    password: 'Admin123!'
};

// Sample guest data pool for creating realistic check-ins
const FIRST_NAMES = ['John', 'Emma', 'Lucas', 'Olivia', 'Ahmet', 'Ayşe', 'Hans', 'Marie', 'Dmitri', 'Natasha'];
const LAST_NAMES = ['Smith', 'Johnson', 'Williams', 'Brown', 'Yılmaz', 'Müller', 'Ivanov', 'Petrov'];
const NATIONALITIES = ['USA', 'UK', 'Turkey', 'Germany', 'Russia', 'France'];

function randomElement(arr) {
    return arr[Math.floor(Math.random() * arr.length)];
}

function generateGuest() {
    const firstName = randomElement(FIRST_NAMES);
    const lastName = randomElement(LAST_NAMES);
    return {
        fullName: `${firstName} ${lastName}`,
        email: `${firstName.toLowerCase()}.${lastName.toLowerCase()}${Math.floor(Math.random() * 1000)}@test.com`,
        phoneNumber: `+90555${Math.floor(Math.random() * 10000000).toString().padStart(7, '0')}`,
        nationality: randomElement(NATIONALITIES),
        isVIP: Math.random() < 0.1, // 10% VIP guests
        roomNumber: `${Math.floor(Math.random() * 5) + 1}${(Math.floor(Math.random() * 10) + 1).toString().padStart(2, '0')}`,
        checkInDate: new Date().toISOString(),
        checkOutDate: new Date(Date.now() + (Math.floor(Math.random() * 7) + 1) * 24 * 60 * 60 * 1000).toISOString(),
    };
}

export function setup() {
    // Login and get token
    const loginRes = http.post(`${BASE_URL}/api/v1.0/Auth/login`, JSON.stringify({
        email: TEST_USER.email,
        password: TEST_USER.password
    }), {
        headers: { 'Content-Type': 'application/json' },
    });

    check(loginRes, {
        'login successful': (r) => r.status === 200,
    });

    if (loginRes.status === 200) {
        return { token: loginRes.json('data.accessToken') };
    }

    console.error('Setup failed - login unsuccessful');
    return { token: null };
}

export default function (data) {
    if (!data.token) {
        console.error('No auth token available');
        return;
    }

    const params = {
        headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${data.token}`,
        },
    };

    group('Dual-Write Check-In Simulation', () => {
        const guest = generateGuest();

        // Step 1: Create guest (writes to SQL + triggers Neo4j graph update)
        const startTime = Date.now();
        const createRes = http.post(`${BASE_URL}/api/v1.0/Guests`, JSON.stringify(guest), params);
        const duration = Date.now() - startTime;

        checkInTrend.add(duration);

        const success = check(createRes, {
            'guest created': (r) => r.status === 200 || r.status === 201,
            'guest has ID': (r) => r.json('data.id') !== undefined,
            'response time acceptable': (r) => duration < 2000,
        });

        if (success) {
            successfulCheckIns.add(1);

            // Step 2: Simulate additional graph operations (if guest created)
            const guestId = createRes.json('data.id');

            // Get guest preferences (reads from graph)
            const prefRes = http.get(`${BASE_URL}/api/v1.0/Guests/${guestId}/preferences`, params);
            check(prefRes, {
                'preferences accessible': (r) => r.status === 200 || r.status === 404,
            });

        } else {
            failedCheckIns.add(1);
            errorRate.add(1);
            console.log(`Check-in failed: ${createRes.status} - ${createRes.body}`);
        }
    });

    group('Concurrent Read Operations', () => {
        // Simulate concurrent dashboard reads during check-in rush
        const dashRes = http.get(`${BASE_URL}/api/v1.0/Dashboard`, params);
        check(dashRes, {
            'dashboard available during load': (r) => r.status === 200,
        });

        // Active guests list
        const guestsRes = http.get(`${BASE_URL}/api/v1.0/Guests?isActive=true`, params);
        check(guestsRes, {
            'active guests list available': (r) => r.status === 200,
        });
    });

    sleep(Math.random() * 2 + 0.5); // Random think time 0.5-2.5s
}

export function teardown(data) {
    console.log('Stress test completed');
    console.log('Summary: Check dual-write performance and data consistency');
}
