#!/usr/bin/env node

/**
 * Generate build information file
 * This script creates a build-info.json file with build metadata
 */

import { writeFileSync } from 'fs'
import { execSync } from 'child_process'

const buildInfo = {
  buildDate: new Date().toISOString(),
  gitCommit: execSync('git rev-parse HEAD').toString().trim(),
  gitBranch: execSync('git rev-parse --abbrev-ref HEAD').toString().trim(),
  version: process.env.VITE_APP_VERSION || '1.0.0',
  environment: process.env.VITE_ENV || 'production',
  nodeVersion: process.version,
}

writeFileSync(
  'dist/build-info.json',
  JSON.stringify(buildInfo, null, 2)
)

console.log('Build info generated:', buildInfo)

