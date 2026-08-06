# Stage 1: Build Angular frontend
FROM node:22-alpine AS frontend-builder

WORKDIR /src/flower-shop.client

# Copy package files
COPY flower-shop.client/package*.json ./

# Install dependencies
RUN npm ci

# Copy source
COPY flower-shop.client/ .

# Build Angular app
RUN npm run build

# Stage 2: Build .NET API
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS backend-builder

WORKDIR /src

# Copy entire solution
COPY . .

# Restore NuGet packages for backend only
RUN dotnet restore Flower-shop.Server/Flower-shop.Server.csproj

# Build the API project in Release mode
RUN dotnet build Flower-shop.Server/Flower-shop.Server.csproj -c Release --no-restore

# Publish the API
RUN dotnet publish Flower-shop.Server/Flower-shop.Server.csproj -c Release -o /app/publish

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0

WORKDIR /app

# Copy published API from backend-builder
COPY --from=backend-builder /app/publish .

# Copy built Angular frontend from frontend-builder
COPY --from=frontend-builder /src/flower-shop.client/dist ./wwwroot

EXPOSE 7185

HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD curl --fail http://localhost:7185/health || exit 1

ENTRYPOINT ["dotnet", "Flower-shop.Server.dll"]
