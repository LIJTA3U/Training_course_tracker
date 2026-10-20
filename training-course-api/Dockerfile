# =============================
# 1. Сборка (SDK)
# =============================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Сначала только файл проекта — слой с восстановлением пакетов кешируется.
COPY src/TrainingCourseApi/TrainingCourseApi.csproj src/TrainingCourseApi/
RUN dotnet restore src/TrainingCourseApi/TrainingCourseApi.csproj

# Затем весь остальной исходный код.
COPY src/ src/
RUN dotnet publish src/TrainingCourseApi/TrainingCourseApi.csproj \
    --configuration $BUILD_CONFIGURATION \
    --output /app/publish \
    --no-restore

# =============================
# 2. Финальный образ (ASP.NET Core Runtime)
# =============================
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

ENV ASPNETCORE_HTTP_PORTS=8080 \
    DOTNET_gcServer=1 \
    DOTNET_TieredPGO=1

EXPOSE 8080

# Запуск от непривилегированного пользователя.
USER $APP_UID

COPY --from=build --chown=$APP_UID:$APP_UID /app/publish .

ENTRYPOINT ["dotnet", "TrainingCourseApi.dll"]