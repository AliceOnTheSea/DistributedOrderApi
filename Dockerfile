# Stage 1: Build & Restore
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy solution and project definitions for optimized caching
COPY DistributedOrderApi.sln ./
COPY src/DistributedOrderApi.Domain/*.csproj ./src/DistributedOrderApi.Domain/
COPY src/DistributedOrderApi.Application/*.csproj ./src/DistributedOrderApi.Application/
COPY src/DistributedOrderApi.Infrastructure/*.csproj ./src/DistributedOrderApi.Infrastructure/
COPY src/DistributedOrderApi.Api/*.csproj ./src/DistributedOrderApi.Api/
COPY tests/DistributedOrderApi.UnitTests/*.csproj ./tests/DistributedOrderApi.UnitTests/
COPY tests/DistributedOrderApi.IntegrationTests/*.csproj ./tests/DistributedOrderApi.IntegrationTests/

RUN dotnet restore

# Copy all source files and publish release assembly
COPY . .
WORKDIR /app/src/DistributedOrderApi.Api
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Production Runtime Target
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

# Non-root security user
RUN adduser --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "DistributedOrderApi.Api.dll"]
