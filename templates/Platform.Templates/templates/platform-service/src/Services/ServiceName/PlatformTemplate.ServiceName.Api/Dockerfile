FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore "src/Services/ServiceName/PlatformTemplate.ServiceName.Api/PlatformTemplate.ServiceName.Api.csproj"
RUN dotnet publish "src/Services/ServiceName/PlatformTemplate.ServiceName.Api/PlatformTemplate.ServiceName.Api.csproj" \
    --configuration Release \
    --output /app/publish \
    --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "PlatformTemplate.ServiceName.Api.dll"]
