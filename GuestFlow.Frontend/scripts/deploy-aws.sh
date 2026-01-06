#!/bin/bash

# AWS S3 + CloudFront Deployment Script
# Prerequisites: AWS CLI configured
# Usage: ./scripts/deploy-aws.sh [bucket-name] [cloudfront-distribution-id]

set -e

BUCKET_NAME=${1:-guestflow-frontend}
DISTRIBUTION_ID=${2:-}

echo "🚀 Deploying to AWS S3 + CloudFront..."

# Check if AWS CLI is installed
if ! command -v aws &> /dev/null; then
  echo "❌ Error: AWS CLI not found. Please install it first."
  exit 1
fi

# Build the project
echo "🏗️  Building project..."
npm run build

# Sync files to S3
echo "📤 Uploading files to S3..."
aws s3 sync ./dist s3://$BUCKET_NAME \
  --delete \
  --cache-control "public, max-age=31536000, immutable" \
  --exclude "*.html" \
  --exclude "*.json"

# Upload HTML files with no cache
aws s3 sync ./dist s3://$BUCKET_NAME \
  --delete \
  --cache-control "public, max-age=0, must-revalidate" \
  --include "*.html" \
  --include "*.json"

# Invalidate CloudFront cache if distribution ID is provided
if [ ! -z "$DISTRIBUTION_ID" ]; then
  echo "🔄 Invalidating CloudFront cache..."
  aws cloudfront create-invalidation \
    --distribution-id $DISTRIBUTION_ID \
    --paths "/*"
fi

echo "✅ Deployment completed successfully!"

