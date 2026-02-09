#!/bin/bash

echo "Starting server..."
cd server
dotnet run &
SERVER_PID=$!
echo "Server started with PID $SERVER_PID"

echo "Starting desktop app..."
cd ../app
dotnet run &
APP_PID=$!
echo "Desktop app started with PID $APP_PID"

# Wait for processes
wait $SERVER_PID $APP_PID