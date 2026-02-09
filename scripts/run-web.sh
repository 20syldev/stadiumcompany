#!/bin/bash

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

echo "Starting server API..."
cd "$SCRIPT_DIR/../server"
dotnet run &
SERVER_PID=$!
echo "Server started with PID $SERVER_PID"

# Wait for server to be ready
echo "Waiting for server API to be ready..."
until curl -s http://localhost:5040/api/questionnaire/user/1 > /dev/null; do
    sleep 1
done
echo "Server API is ready!"

echo "Starting web app..."
cd "$SCRIPT_DIR/../web"
dotnet run &
WEB_PID=$!
echo "Web app started with PID $WEB_PID"

# Wait for processes
wait $SERVER_PID $WEB_PID