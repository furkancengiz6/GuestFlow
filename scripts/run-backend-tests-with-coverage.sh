#!/bin/bash
# GuestFlow Backend Test Coverage Script
# Runs backend tests with code coverage reporting

OUTPUT_FORMAT="${1:-opencover,cobertura,json}"
OUTPUT_PATH="${2:-./coverage/backend-coverage}"
THRESHOLD="${3:-0}"

echo "🧪 Running backend tests with code coverage..."

# Create coverage directory if it doesn't exist
mkdir -p "$(dirname "$OUTPUT_PATH")"

# Run tests with coverage
dotnet test \
    GuestFlow.Application.Tests/GuestFlow.Application.Tests.csproj \
    --collect:"XPlat Code Coverage" \
    --settings:GuestFlow.Application.Tests/coverlet.runsettings \
    --results-directory:"$(dirname "$OUTPUT_PATH")" \
    --logger:"trx;LogFileName=test-results.trx" \
    --logger:"console;verbosity=normal"

if [ $? -eq 0 ]; then
    echo "✅ Tests passed!"
    
    # Find coverage files
    COVERAGE_FILES=$(find "$(dirname "$OUTPUT_PATH")" -name "coverage.*" -type f \( -name "*.xml" -o -name "*.json" \))
    
    if [ -n "$COVERAGE_FILES" ]; then
        echo "📊 Coverage reports generated:"
        echo "$COVERAGE_FILES" | while read -r file; do
            echo "   - $file"
        done
        
        # Try to find cobertura file for summary
        COBERTURA_FILE=$(find "$(dirname "$OUTPUT_PATH")" -name "*cobertura*" -type f | head -n 1)
        if [ -n "$COBERTURA_FILE" ]; then
            echo ""
            echo "📈 Coverage Summary:"
            echo "   Coverage report: $COBERTURA_FILE"
        fi
    fi
else
    echo "❌ Tests failed!"
    exit 1
fi

echo ""
echo "💡 Tip: Open coverage/index.html in a browser to view detailed coverage report"
