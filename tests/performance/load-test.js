import http from 'k6/http';
import { check, sleep } from 'k6';

// Test configuration
export const options = {
  stages: [
    // Ramp up to 50 users over 1 minute
    { duration: '1m', target: 50 },

    // Stay at 50 users for 3 minutes
    { duration: '3m', target: 50 },

    // Ramp up to 100 users over 2 minutes
    { duration: '2m', target: 100 },

    // Stay at 100 users for 5 minutes
    { duration: '5m', target: 100 },

    // Ramp down to 0 users over 1 minute
    { duration: '1m', target: 0 },
  ],

  thresholds: {
    // HTTP request duration should be < 500ms for 95% of requests
    http_req_duration: ['p(95)<500'],

    // HTTP request failed rate should be < 1%
    http_req_failed: ['rate<0.01'],

    // 95% of requests should be successful
    http_req_success: ['rate>0.95'],
  },

  // Cloud test configuration (if using k6 cloud)
  ext: {
    loadimpact: {
      projectID: 1234567,
      name: 'GuestFlow API Load Test'
    }
  }
};

// Base URL - configure for your environment
const BASE_URL = __ENV.BASE_URL || 'http://localhost:5000';

// Test data
const testUsers = [
  { email: 'test1@example.com', password: 'TestPass123!' },
  { email: 'test2@example.com', password: 'TestPass123!' },
  { email: 'admin@example.com', password: 'AdminPass123!' }
];

let authTokens = [];

// Setup function - run once before the test starts
export function setup() {
  console.log('Starting GuestFlow API Load Test');

  // Login and get auth tokens
  for (const user of testUsers) {
    const loginResponse = http.post(`${BASE_URL}/api/auth/login`, {
      email: user.email,
      password: user.password
    });

    if (loginResponse.status === 200) {
      const responseBody = JSON.parse(loginResponse.body);
      if (responseBody.isSuccess && responseBody.data?.token) {
        authTokens.push(responseBody.data.token);
      }
    }
  }

  console.log(`Obtained ${authTokens.length} auth tokens`);
  return { authTokens };
}

// Default function - executed for each VU iteration
export default function (data) {
  const token = data.authTokens[Math.floor(Math.random() * data.authTokens.length)];

  const headers = {
    'Authorization': `Bearer ${token}`,
    'Content-Type': 'application/json'
  };

  // Test 1: Dashboard quick stats (high frequency endpoint)
  const dashboardResponse = http.get(`${BASE_URL}/api/dashboard/quick-stats`, { headers });
  check(dashboardResponse, {
    'dashboard status is 200': (r) => r.status === 200,
    'dashboard response time < 300ms': (r) => r.timings.duration < 300,
    'dashboard returns valid data': (r) => {
      try {
        const data = JSON.parse(r.body);
        return data && typeof data.totalGuests === 'number';
      } catch {
        return false;
      }
    }
  });

  // Test 2: Transfers list (medium frequency)
  const transfersResponse = http.get(`${BASE_URL}/api/transfers?pageSize=10&page=1`, { headers });
  check(transfersResponse, {
    'transfers status is 200': (r) => r.status === 200,
    'transfers response time < 500ms': (r) => r.timings.duration < 500,
  });

  // Test 3: Health check (monitoring endpoint)
  const healthResponse = http.get(`${BASE_URL}/health`);
  check(healthResponse, {
    'health status is 200': (r) => r.status === 200,
    'health response time < 100ms': (r) => r.timings.duration < 100,
  });

  // Random sleep between 1-3 seconds to simulate real user behavior
  sleep(Math.random() * 2 + 1);
}

// Handle summary - custom summary output
export function handleSummary(data) {
  const summary = {
    'stdout': textSummary(data, { indent: ' ', enableColors: true }),
    'performance-report.json': JSON.stringify(data, null, 2),
    'performance-summary.html': htmlReport(data),
  };

  return summary;
}

function textSummary(data, options) {
  return `
📊 GuestFlow API Performance Test Results
==========================================

Test Duration: ${data.metrics.iteration_duration.values.avg}ms avg iteration
Total Requests: ${data.metrics.http_reqs.values.count}
Failed Requests: ${data.metrics.http_req_failed.values.rate * 100}%

Response Times:
  Average: ${Math.round(data.metrics.http_req_duration.values.avg)}ms
  95th percentile: ${Math.round(data.metrics.http_req_duration.values['p(95)'])}ms
  99th percentile: ${Math.round(data.metrics.http_req_duration.values['p(99)'])}ms

HTTP Status:
  2xx: ${data.metrics.http_req_success.values.rate * 100}%
  4xx: ${data.metrics.http_req_failed.values.rate * 100}%

Load Pattern:
  Peak concurrent users: 100
  Test duration: ~12 minutes
  Gradual ramp-up and ramp-down

Threshold Results:
${Object.entries(data.metrics)
  .filter(([key]) => key.includes('http_req'))
  .map(([key, metric]) => `  ${key}: ${metric.values.rate || metric.values.avg}ms`)
  .join('\n')}

Recommendations:
${generateRecommendations(data)}
`;
}

function htmlReport(data) {
  return `
<!DOCTYPE html>
<html>
<head>
    <title>GuestFlow API Performance Test Report</title>
    <style>
        body { font-family: Arial, sans-serif; margin: 40px; }
        .metric { background: #f5f5f5; padding: 10px; margin: 10px 0; border-radius: 5px; }
        .success { color: green; }
        .warning { color: orange; }
        .error { color: red; }
        h1, h2 { color: #333; }
        table { border-collapse: collapse; width: 100%; }
        th, td { border: 1px solid #ddd; padding: 8px; text-align: left; }
        th { background-color: #f2f2f2; }
    </style>
</head>
<body>
    <h1>🚀 GuestFlow API Performance Test Report</h1>

    <div class="metric">
        <h2>📈 Key Metrics</h2>
        <table>
            <tr><th>Metric</th><th>Value</th><th>Status</th></tr>
            <tr>
                <td>Average Response Time</td>
                <td>${Math.round(data.metrics.http_req_duration.values.avg)}ms</td>
                <td class="${data.metrics.http_req_duration.values.avg < 300 ? 'success' : 'warning'}">
                    ${data.metrics.http_req_duration.values.avg < 300 ? '✅ Good' : '⚠️ Slow'}
                </td>
            </tr>
            <tr>
                <td>95th Percentile</td>
                <td>${Math.round(data.metrics.http_req_duration.values['p(95)'])}ms</td>
                <td class="${data.metrics.http_req_duration.values['p(95)'] < 500 ? 'success' : 'error'}">
                    ${data.metrics.http_req_duration.values['p(95)'] < 500 ? '✅ Good' : '❌ Too Slow'}
                </td>
            </tr>
            <tr>
                <td>Success Rate</td>
                <td>${(data.metrics.http_req_success.values.rate * 100).toFixed(2)}%</td>
                <td class="${data.metrics.http_req_success.values.rate > 0.95 ? 'success' : 'error'}">
                    ${data.metrics.http_req_success.values.rate > 0.95 ? '✅ Good' : '❌ Too Low'}
                </td>
            </tr>
            <tr>
                <td>Total Requests</td>
                <td>${data.metrics.http_reqs.values.count}</td>
                <td>ℹ️ Info</td>
            </tr>
        </table>
    </div>

    <div class="metric">
        <h2>🎯 Test Configuration</h2>
        <ul>
            <li><strong>Peak Load:</strong> 100 concurrent users</li>
            <li><strong>Test Duration:</strong> ~12 minutes</li>
            <li><strong>Ramp Pattern:</strong> Gradual increase/decrease</li>
            <li><strong>Endpoints Tested:</strong> Dashboard, Transfers, Health Check</li>
        </ul>
    </div>

    <div class="metric">
        <h2>💡 Recommendations</h2>
        <ul>
            ${generateRecommendations(data).split('\n').map(rec => `<li>${rec}</li>`).join('')}
        </ul>
    </div>
</body>
</html>
`;
}

function generateRecommendations(data) {
  const recommendations = [];

  const avgResponseTime = data.metrics.http_req_duration.values.avg;
  const p95ResponseTime = data.metrics.http_req_duration.values['p(95)'];
  const successRate = data.metrics.http_req_success.values.rate;

  if (avgResponseTime > 300) {
    recommendations.push('⚠️ Average response time is high. Consider optimizing database queries and adding more caching.');
  }

  if (p95ResponseTime > 500) {
    recommendations.push('❌ 95th percentile response time exceeds 500ms. Critical performance issue detected.');
  }

  if (successRate < 0.95) {
    recommendations.push('❌ Success rate is below 95%. Investigate error patterns and implement better error handling.');
  }

  if (avgResponseTime < 200 && successRate > 0.99) {
    recommendations.push('✅ Excellent performance! System handles load well.');
  }

  if (recommendations.length === 0) {
    recommendations.push('✅ Performance is within acceptable ranges.');
  }

  return recommendations.join('\n');
}