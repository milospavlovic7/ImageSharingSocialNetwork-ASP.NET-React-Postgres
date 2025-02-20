# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Kopiranje fajlova i build
COPY *.csproj ./
RUN dotnet restore

COPY . ./
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:5000

# Kopiranje build output-a
COPY --from=build /app/publish .

# Kopiranje sertifikata
COPY certs/mydevelopmentcert.pfx /app/certs/
RUN chmod 644 /app/certs/mydevelopmentcert.pfx

# Ekspozicija porta
EXPOSE 5000
ENTRYPOINT ["dotnet", "AspNetCorePostgreSQLDockerApp.dll"]
