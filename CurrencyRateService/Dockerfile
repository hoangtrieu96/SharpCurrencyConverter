### Stage 1: Build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app

# Copy the .csproj file and restore as distinct layers
COPY CurrencyRateService/CurrencyRateService.csproj ./CurrencyRateService/
WORKDIR /app/CurrencyRateService
RUN dotnet restore CurrencyRateService.csproj

# Copy the rest of the code
COPY CurrencyRateService/. ./

# Build the application
RUN dotnet publish -c Release -o /app/out

### Stage 2: Create the runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build-env /app/out .

# Expose the port
EXPOSE 8080

# Set the entry point
ENTRYPOINT [ "dotnet", "CurrencyRateService.dll" ]
