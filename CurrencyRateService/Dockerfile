### Stage 1: Build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app

# Define an argument for the base path with a default value
# Need this for the CI because it is calling from the repo level
ARG BASE_PATH=.
ENV BASE_PATH=${BASE_PATH}

# Restore separately so it can cache
COPY ${BASE_PATH}/*.csproj ./
RUN dotnet restore

# Copy the rest of the code
COPY ${BASE_PATH}/. ./

# Build the application
RUN dotnet publish -c Release -o /app/out

### Stage 2: Create the run time image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build-env /app/out .

# Expose the port
EXPOSE 8080

# Set the entry point
ENTRYPOINT [ "dotnet", "CurrencyRateService.dll" ]

