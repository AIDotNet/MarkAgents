# Backend Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Build frontend
FROM node:23-alpine AS frontend-build
WORKDIR /src/web
COPY web/package*.json ./
RUN npm i
COPY web/ .
RUN npm run build

# Build backend
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS backend-build
WORKDIR /src
COPY ["src/MarkAgent.Host/MarkAgent.Host.csproj", "src/MarkAgent.Host/"]
RUN dotnet restore "src/MarkAgent.Host/MarkAgent.Host.csproj"
COPY . .
WORKDIR "/src/src/MarkAgent.Host"
RUN dotnet build "MarkAgent.Host.csproj" -c Release -o /app/build

FROM backend-build AS publish
RUN dotnet publish "MarkAgent.Host.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Copy frontend build to wwwroot
COPY --from=frontend-build /src/web/dist /app/wwwroot

ENTRYPOINT ["dotnet", "MarkAgent.Host.dll"]