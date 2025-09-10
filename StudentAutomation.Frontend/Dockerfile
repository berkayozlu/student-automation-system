# Use the official .NET 9 SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["StudentAutomation.Frontend.csproj", "./"]
RUN dotnet restore "StudentAutomation.Frontend.csproj"

# Copy everything else and build
COPY . .
RUN dotnet build "StudentAutomation.Frontend.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "StudentAutomation.Frontend.csproj" -c Release -o /app/publish

# Use the official .NET 9 runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

COPY --from=publish /app/publish .

# Expose port
EXPOSE 8080

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "StudentAutomation.Frontend.dll"]
