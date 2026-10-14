# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /src

# Copy solution and project files for layer caching
COPY QRHub.sln ./
COPY QRHub.Domain/QRHub.Domain.csproj QRHub.Domain/
COPY QRHub.Application/QRHub.Application.csproj QRHub.Application/
COPY QRHub.Infrastructure/QRHub.Infrastructure.csproj QRHub.Infrastructure/
COPY QRHub.Api/QRHub.Api.csproj QRHub.Api/

# Restore dependencies
RUN dotnet restore QRHub.sln

# Copy the rest of the source code
COPY . .

# Publish release build
WORKDIR /src/QRHub.Api
RUN dotnet publish QRHub.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

# Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS runtime
WORKDIR /app

# Non-root user for security
RUN addgroup -S appgroup && adduser -S appuser -G appgroup
USER appuser

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080

ENTRYPOINT ["dotnet", "QRHub.Api.dll"]
