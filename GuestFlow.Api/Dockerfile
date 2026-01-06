# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["GuestFlow.sln", "."]
COPY ["GuestFlow.Domain/GuestFlow.Domain.csproj", "GuestFlow.Domain/"]
COPY ["GuestFlow.Application/GuestFlow.Application.csproj", "GuestFlow.Application/"]
COPY ["GuestFlow.Persistence/GuestFlow.Persistence.csproj", "GuestFlow.Persistence/"]
COPY ["GuestFlow.Api/GuestFlow.Api.csproj", "GuestFlow.Api/"]

RUN dotnet restore "GuestFlow.Api/GuestFlow.Api.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/GuestFlow.Api"
RUN dotnet build "GuestFlow.Api.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "GuestFlow.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Create non-root user
RUN groupadd -r appuser && useradd -r -g appuser appuser

# Create logs directory
RUN mkdir -p /app/logs && chown -R appuser:appuser /app/logs

# Copy published app
COPY --from=publish --chown=appuser:appuser /app/publish .

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=60s --retries=3 \
    CMD curl -f http://localhost:5146/health || exit 1

EXPOSE 5000
ENTRYPOINT ["dotnet", "GuestFlow.Api.dll"]
