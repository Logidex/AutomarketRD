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

EXPOSE 8080

COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "AutoMarket.API.dll"]