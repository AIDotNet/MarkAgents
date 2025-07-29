#!/bin/bash

echo "Building MarkAgent with integrated frontend..."

# Build the multi-stage Docker image
docker build -t markagent:latest .

# Run the container
echo "Starting MarkAgent server..."
docker run -d \
  --name markagent \
  -p 5157:80 \
  -v markagent-data:/app/data \
  markagent:latest

echo "MarkAgent is now running at http://localhost:5157"