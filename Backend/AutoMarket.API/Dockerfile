# =========================================
# ETAPA 1: BUILD
# =========================================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copiar el archivo de solución y restaurar dependencias
COPY ["AutoMarketRD.sln", "./"]
COPY ["AutoMarket.API/AutoMarket.API.csproj", "AutoMarket.API/"]
COPY ["AutoMarket.Application/AutoMarket.Application.csproj", "AutoMarket.Application/"]
COPY ["AutoMarket.Core/AutoMarket.Core.csproj", "AutoMarket.Core/"]
COPY ["AutoMarket.Infrastructure/AutoMarket.Infrastructure.csproj", "AutoMarket.Infrastructure/"]

# Restaurar paquetes
RUN dotnet restore "AutoMarket.API/AutoMarket.API.csproj"

# Copiar todo el código
COPY . .

# Compilar en modo Release
WORKDIR "/src/AutoMarket.API"
RUN dotnet build "AutoMarket.API.csproj" -c Release -o /app/build

# =========================================
# ETAPA 2: PUBLISH
# =========================================
FROM build AS publish
RUN dotnet publish "AutoMarket.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# =========================================
# ETAPA 3: RUNTIME
# =========================================
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Variables de entorno por defecto
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080

# curl no viene por defecto en la imagen aspnet; es necesario para el healthcheck
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

# Usuario no-root (UID fijo 1001) para ejecutar la API sin privilegios
RUN useradd --create-home --shell /usr/sbin/nologin --uid 1001 appuser

COPY --from=publish /app/publish .
RUN chown -R appuser:appuser /app

USER appuser

HEALTHCHECK --interval=30s --timeout=10s --start-period=40s --retries=3 \
  CMD curl -f http://localhost:8080/health/ready || exit 1

EXPOSE 8080

ENTRYPOINT ["dotnet", "AutoMarket.API.dll"]