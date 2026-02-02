#!/bin/bash

echo "===================================="
echo "GuestFlow Demo Launcher"
echo "===================================="
echo ""
echo "Starting demo environment..."
echo ""

# Set environment variables
export ASPNETCORE_ENVIRONMENT=Development
export SeedDemoData=true

echo "[1/3] Starting Backend with Demo Data..."
echo ""
echo "IMPORTANT: Save the demo user credentials from the console output!"
echo ""

# Start backend in background
cd GuestFlow.Api
dotnet run &
BACKEND_PID=$!

sleep 15

echo "[2/3] Database seeding in progress..."
sleep 10

echo "[3/3] Starting Frontend..."
cd ../GuestFlow.Frontend
npm run dev &
FRONTEND_PID=$!

echo ""
echo "===================================="
echo "Demo environment is running!"
echo "===================================="
echo ""
echo "Services:"
echo "- Backend API: http://localhost:5146"
echo "- Swagger UI: http://localhost:5146/swagger"
echo "- Frontend: http://localhost:5173"
echo ""
echo "IMPORTANT: Check the terminal for demo user credentials!"
echo ""
echo "Press Ctrl+C to stop all services"
echo ""

# Wait for interrupt
trap "kill $BACKEND_PID $FRONTEND_PID; exit" INT
wait
