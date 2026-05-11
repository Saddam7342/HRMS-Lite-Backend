# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj files and restore
COPY ["src/HRMS.API/HRMS.API.csproj", "src/HRMS.API/"]
COPY ["src/HRMS.Application/HRMS.Application.csproj", "src/HRMS.Application/"]
COPY ["src/HRMS.Infrastructure/HRMS.Infrastructure.csproj", "src/HRMS.Infrastructure/"]
COPY ["src/HRMS.Persistence/HRMS.Persistence.csproj", "src/HRMS.Persistence/"]
COPY ["src/HRMS.Shared/HRMS.Shared.csproj", "src/HRMS.Shared/"]
COPY ["src/HRMS.Domain/HRMS.Domain.csproj", "src/HRMS.Domain/"]

RUN dotnet restore "src/HRMS.API/HRMS.API.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/src/HRMS.API"
RUN dotnet build "HRMS.API.csproj" -c Release -o /app/build

# Publish Stage
FROM build AS publish
RUN dotnet publish "HRMS.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Railway/Render dynamic port binding
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Optimization for low memory environments
ENV DOTNET_GCHeapHardLimitPercent=80
ENV COMPlus_gcServer=0

# Create uploads folder and set permissions
RUN mkdir -p /app/wwwroot/uploads && chmod -R 777 /app/wwwroot/uploads

# Use the dynamic PORT variable if provided by Railway
ENTRYPOINT ["sh", "-c", "dotnet HRMS.API.dll --urls http://0.0.0.0:${PORT:-8080}"]

