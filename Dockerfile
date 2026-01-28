# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

# Copy the project file
COPY ["Rise.API/Rise.API/Rise.API.csproj", "Rise.API/"]

# Restore dependencies
RUN dotnet restore "Rise.API/Rise.API.csproj"

# Copy the entire Rise.API directory
COPY ["Rise.API/", "."]

# Build the application
RUN dotnet build "Rise.API/Rise.API/Rise.API.csproj" -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish

RUN dotnet publish "Rise.API/Rise.API/Rise.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0

WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Copy published files from publish stage
COPY --from=publish /app/publish .

# Expose port
EXPOSE 5000

# Set environment variables
ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_CLI_TELEMETRY_OPTOUT=1

# Run the application
ENTRYPOINT ["dotnet", "Rise.API.dll"]
