#!/bin/bash

# GuestFlow Frontend Deployment Script
# Usage: ./scripts/deploy.sh [environment]
# Example: ./scripts/deploy.sh production

set -e

ENVIRONMENT=${1:-production}
BUILD_DIR="dist"
DEPLOY_DIR="deploy"

echo "🚀 Starting deployment for environment: $ENVIRONMENT"

# Check if .env file exists
if [ ! -f ".env" ]; then
  echo "⚠️  Warning: .env file not found. Creating from .env.example..."
  if [ -f ".env.example" ]; then
    cp .env.example .env
  else
    echo "❌ Error: .env.example not found. Please create .env file manually."
    exit 1
  fi
fi

# Install dependencies
echo "📦 Installing dependencies..."
npm ci

# Run linting
echo "🔍 Running linter..."
npm run lint

# Run type check
echo "🔍 Running type check..."
npx tsc --noEmit

# Run tests
echo "🧪 Running tests..."
npm run test:ci

# Build for production
echo "🏗️  Building for production..."
npm run build

# Check if build was successful
if [ ! -d "$BUILD_DIR" ]; then
  echo "❌ Error: Build directory not found. Build failed."
  exit 1
fi

# Create deployment directory
mkdir -p $DEPLOY_DIR

# Copy build files
echo "📋 Copying build files..."
cp -r $BUILD_DIR/* $DEPLOY_DIR/

# Create deployment package
echo "📦 Creating deployment package..."
tar -czf "deploy-$ENVIRONMENT-$(date +%Y%m%d-%H%M%S).tar.gz" -C $DEPLOY_DIR .

echo "✅ Deployment package created successfully!"
echo "📁 Package: deploy-$ENVIRONMENT-$(date +%Y%m%d-%H%M%S).tar.gz"
echo ""

if [ "$ENVIRONMENT" = "kubernetes" ]; then
    echo "🐳 Deploying to Kubernetes..."

    # Check if kubectl is available
    if ! command -v kubectl &> /dev/null; then
        echo "❌ Error: kubectl not found. Please install kubectl."
        exit 1
    fi

    # Apply Kubernetes manifests
    echo "📋 Applying Kubernetes manifests..."
    kubectl apply -f k8s/namespace.yml
    kubectl apply -f k8s/configmap.yml
    kubectl apply -f k8s/secret.yml
    kubectl apply -f k8s/sqlserver.yml
    kubectl apply -f k8s/redis.yml
    kubectl apply -f k8s/api.yml
    kubectl apply -f k8s/frontend.yml

    # Wait for deployments to be ready
    echo "⏳ Waiting for deployments to be ready..."
    kubectl wait --for=condition=available --timeout=300s deployment/guestflow-api -n guestflow
    kubectl wait --for=condition=available --timeout=300s deployment/guestflow-frontend -n guestflow

    echo "✅ Kubernetes deployment completed!"
    echo "🌐 Frontend URL: http://app.guestflow.com"
    echo "🔗 API URL: http://api.guestflow.com/api/v1.0"
    echo "📊 Monitoring: http://monitoring.guestflow.com"
else
    echo "Next steps for $ENVIRONMENT deployment:"
    echo "1. Upload the package to your server"
    echo "2. Extract: tar -xzf deploy-$ENVIRONMENT-*.tar.gz"
    echo "3. Configure your web server to serve the files"
    echo ""
    echo "For specific deployment targets, see:"
    echo "- scripts/deploy-azure.sh"
    echo "- scripts/deploy-aws.sh"
    echo "- scripts/deploy-ftp.sh"
fi

