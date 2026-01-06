#!/bin/bash

# Validate build output
# This script checks if the build was successful and all required files exist

set -e

BUILD_DIR=${1:-dist}

echo "🔍 Validating build in: $BUILD_DIR"

# Check if build directory exists
if [ ! -d "$BUILD_DIR" ]; then
  echo "❌ Error: Build directory not found: $BUILD_DIR"
  exit 1
fi

# Check required files
REQUIRED_FILES=("index.html" "assets")

for file in "${REQUIRED_FILES[@]}"; do
  if [ ! -e "$BUILD_DIR/$file" ]; then
    echo "❌ Error: Required file/directory not found: $file"
    exit 1
  fi
done

# Check if index.html contains expected content
if ! grep -q "<!DOCTYPE html>" "$BUILD_DIR/index.html"; then
  echo "❌ Error: index.html is invalid"
  exit 1
fi

# Check build info
if [ -f "$BUILD_DIR/build-info.json" ]; then
  echo "✅ Build info found"
  cat "$BUILD_DIR/build-info.json"
else
  echo "⚠️  Warning: build-info.json not found"
fi

echo "✅ Build validation passed"

