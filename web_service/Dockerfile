# --- —борка приложени€ ---
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["web_service.csproj", "."]
RUN dotnet restore "./web_service.csproj"
COPY . .
RUN dotnet build "./web_service.csproj" -c $BUILD_CONFIGURATION -o /app/build
RUN dotnet publish "./web_service.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# --- –елизный образ с приложением ---
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
# локаль и UTF-8
RUN apt-get update \
 && apt-get install -y locales \
 && sed -i 's/# ru_RU.UTF-8 UTF-8/ru_RU.UTF-8 UTF-8/' /etc/locale.gen \
 && locale-gen ru_RU.UTF-8 \
 && update-locale LANG=ru_RU.UTF-8
ENV LANG=ru_RU.UTF-8 \
    LANGUAGE=ru_RU:ru \
    LC_ALL=ru_RU.UTF-8
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
HEALTHCHECK --interval=5s --timeout=3s --retries=3 CMD curl -f http://localhost:8080/health || exit 1
ENTRYPOINT ["dotnet", "web_service.dll"]