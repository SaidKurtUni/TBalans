# ── Build aşaması ──────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Proje dosyalarını kopyala ve bağımlılıkları yükle
COPY TBalans.Domain/TBalans.Domain.csproj       TBalans.Domain/
COPY TBalans.Application/TBalans.Application.csproj TBalans.Application/
COPY TBalans.Infrastructure/TBalans.Infrastructure.csproj TBalans.Infrastructure/
COPY TBalans.Api/TBalans.Api.csproj             TBalans.Api/
COPY TBalans.sln .

# NuGet paketlerini geri yükle (katmanlı caching)
RUN dotnet restore TBalans.Api/TBalans.Api.csproj

# Tüm kaynak kodları kopyala
COPY . .

# Release yapısını derle
WORKDIR /src/TBalans.Api
RUN dotnet publish TBalans.Api.csproj -c Release -o /app/publish --no-restore

# ── Runtime aşaması ────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Derlenen dosyaları kopyala
COPY --from=build /app/publish .

# SQLite veritabanı için kalıcı klasör
RUN mkdir -p /app/data

# Render PORT env değişkenini kullan
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# Uygulamayı başlat
ENTRYPOINT ["dotnet", "TBalans.Api.dll"]
