#!/bin/bash

# FTP/SFTP Deployment Script
# Usage: ./scripts/deploy-ftp.sh
# Environment variables required:
# - FTP_HOST
# - FTP_USER
# - FTP_PASSWORD
# - FTP_REMOTE_PATH

set -e

echo "🚀 Deploying via FTP/SFTP..."

# Check required environment variables
if [ -z "$FTP_HOST" ] || [ -z "$FTP_USER" ] || [ -z "$FTP_PASSWORD" ]; then
  echo "❌ Error: FTP credentials not set."
  echo "Please set the following environment variables:"
  echo "- FTP_HOST"
  echo "- FTP_USER"
  echo "- FTP_PASSWORD"
  echo "- FTP_REMOTE_PATH (optional)"
  exit 1
fi

# Build the project
echo "🏗️  Building project..."
npm run build

# Install lftp if not available
if ! command -v lftp &> /dev/null; then
  echo "📦 Installing lftp..."
  if [[ "$OSTYPE" == "linux-gnu"* ]]; then
    sudo apt-get update && sudo apt-get install -y lftp
  elif [[ "$OSTYPE" == "darwin"* ]]; then
    brew install lftp
  else
    echo "❌ Error: lftp not found. Please install it manually."
    exit 1
  fi
fi

# Deploy via FTP
echo "📤 Uploading files via FTP..."
lftp -c "
set ftp:ssl-allow no
open -u $FTP_USER,$FTP_PASSWORD $FTP_HOST
cd ${FTP_REMOTE_PATH:-/}
mirror -R --delete --verbose ./dist .
bye
"

echo "✅ Deployment completed successfully!"

