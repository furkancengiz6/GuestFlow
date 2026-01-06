#!/bin/bash

# Azure Static Web Apps Deployment Script
# Prerequisites: Azure CLI and Static Web Apps CLI installed
# Usage: ./scripts/deploy-azure.sh

set -e

echo "🚀 Deploying to Azure Static Web Apps..."

# Check if Azure CLI is installed
if ! command -v az &> /dev/null; then
  echo "❌ Error: Azure CLI not found. Please install it first."
  exit 1
fi

# Check if SWA CLI is installed
if ! command -v swa &> /dev/null; then
  echo "📦 Installing Azure Static Web Apps CLI..."
  npm install -g @azure/static-web-apps-cli
fi

# Build the project
echo "🏗️  Building project..."
npm run build

# Deploy to Azure
echo "☁️  Deploying to Azure..."
swa deploy ./dist \
  --deployment-token $AZURE_STATIC_WEB_APPS_API_TOKEN \
  --env production

echo "✅ Deployment completed successfully!"

