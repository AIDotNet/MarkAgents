@echo off
echo Building MarkAgent with integrated frontend...

REM Build the multi-stage Docker image
docker build -t markagent:latest .

REM Run the container
echo Starting MarkAgent server...
docker run -d ^
  --name markagent ^
  -p 5157:80 ^
  -v markagent-data:/app/data ^
  markagent:latest

echo MarkAgent is now running at http://localhost:5157
pause