// K6 Quick Verification Test
import http from 'k6/http';
import { check, sleep, group } from 'k6';
import { Rate, Trend, Counter } from 'k6/metrics';

// Custom metrics
const errorRate = new Rate('errors');
const checkInTrend = new Trend('checkin_duration');
const successfulCheckIns = new Counter('successful_checkins');
const failedCheckIns = new Counter('failed_checkins');

export const options = {
    scenarios: {
        quick: {
            executor: 'constant-vus',
            vus: 20,
            duration: '30s',
        },
    },
    thresholds: {
        http_req_duration: ['p(95)<2000'],
        errors: ['rate<0.05'],
        checkin_duration: ['p(95)<2000'],
    },
};

const BASE_URL = __ENV.BASE_URL || 'http://localhost:5000';

const TEST_USER = {
    email: __ENV.TEST_USER || 'demo.admin.demo.admin@guestflow.local',
    password: __ENV.TEST_PASSWORD || 'GuestFlow123!'
};

const FIRST_NAMES = ['John', 'Emma', 'Lucas', 'Olivia', 'Ahmet', 'Ayşe'];
const LAST_NAMES = ['Smith', 'Johnson', 'Williams', 'Brown', 'Yılmaz', 'Müller'];
const NATIONALITIES = ['USA', 'UK', 'Turkey', 'Germany', 'Russia', 'France'];

function randomElement(arr) {
    return arr[Math.floor(Math.random() * arr.length)];
}

function generateGuest() {
    const firstName = randomElement(FIRST_NAMES);
    const lastName = randomElement(LAST_NAMES);
    return {
        fullName: `${firstName} ${lastName}`,
        email: `${firstName.toLowerCase()}.${lastName.toLowerCase()}${Date.now()}${Math.floor(Math.random() * 100000)}@test.com`,
        phoneNumber: `+90${Math.floor(Math.random() * 10000000000).toString().padStart(10, '0')}`,
        nationality: randomElement(NATIONALITIES),
        isVIP: Math.random() < 0.1,
        roomNumber: `${Math.floor(Math.random() * 5) + 1}${(Math.floor(Math.random() * 10) + 1).toString().padStart(2, '0')}`,
        checkInDate: new Date().toISOString(),
        checkOutDate: new Date(Date.now() + (Math.floor(Math.random() * 7) + 1) * 24 * 60 * 60 * 1000).toISOString(),
    };
}

export function setup() {
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
        const token = loginRes.json('accessToken');
        if (!token) {
            console.error(`Login success but token not found in body: ${loginRes.body}`);
        }
        return { token: token };
    }

    console.error(`Setup failed - login unsuccessful: ${loginRes.status} ${loginRes.body}`);
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
        const guest = generateGuest(); // Ensure unique email? Added random int range 10000

        const startTime = Date.now();
        const createRes = http.post(`${BASE_URL}/api/v1.0/Guests`, JSON.stringify(guest), params);
        const duration = Date.now() - startTime;

        checkInTrend.add(duration);

        const success = check(createRes, {
            'guest created': (r) => r.status === 200 || r.status === 201,
            'guest has ID': (r) => r.json('data') !== undefined && r.json('data') !== null,
        });

        if (success) {
            successfulCheckIns.add(1);
            const guestId = createRes.json('data');

            const prefRes = http.get(`${BASE_URL}/api/v1.0/Guests/${guestId}/preferences`, params);
            check(prefRes, {
                'preferences accessible': (r) => r.status === 200 || r.status === 404,
            });

        } else {
            failedCheckIns.add(1);
            errorRate.add(1);
            if (createRes.status !== 200 && createRes.status !== 201) {
                console.log(`Check-in failed: ${createRes.status} - ${createRes.body}`);
            } else {
                console.log(`Check-in ID check failed: ${createRes.body}`);
            }
        }
    });

    sleep(1);
}
