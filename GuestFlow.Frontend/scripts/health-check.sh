#!/bin/bash

# Health check script for GuestFlow Frontend
# This script checks if the application is running correctly

set -e

HEALTH_URL=${1:-http://localhost/health}
TIMEOUT=${2:-5}

echo "🔍 Checking health at: $HEALTH_URL"

# Check if URL is accessible
if curl -f -s --max-time $TIMEOUT "$HEALTH_URL" > /dev/null; then
  echo "✅ Health check passed"
  exit 0
else
  echo "❌ Health check failed"
  exit 1
fi

