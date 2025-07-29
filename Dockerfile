# Multi-stage Dockerfile for building both frontend and backend

# Frontend build stage
FROM node:18-alpine AS frontend-build
WORKDIR /src/web
COPY web/package*.json ./
COPY web/bun.lock ./
RUN npm ci
COPY web/ ./
RUN npm run build

# Backend build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS backend-build
WORKDIR /src
COPY src/MarkAgent.Host/MarkAgent.Host.csproj src/MarkAgent.Host/
RUN dotnet restore src/MarkAgent.Host/MarkAgent.Host.csproj
COPY . .
WORKDIR /src/src/MarkAgent.Host
RUN dotnet publish MarkAgent.Host.csproj -c Release -o /app/publish /p:UseAppHost=false

# Final stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Copy backend publish output
COPY --from=backend-build /app/publish .

# Create wwwroot directory and copy frontend build
RUN mkdir -p /app/wwwroot
COPY --from=frontend-build /src/web/dist /app/wwwroot

# Set permissions
RUN chmod -R 755 /app/wwwroot

EXPOSE 80
ENTRYPOINT ["dotnet", "MarkAgent.Host.dll"]