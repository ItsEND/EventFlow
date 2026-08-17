FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["src/Shared/EventFlow.Contracts/EventFlow.Contracts.csproj", "src/Shared/EventFlow.Contracts/"]
COPY ["src/Services/Events/EventFlow.Events.Api/EventFlow.Events.Api.csproj", "src/Services/Events/EventFlow.Events.Api/"]
COPY ["src/Services/Events/EventFlow.Events.Application/EventFlow.Events.Application.csproj", "src/Services/Events/EventFlow.Events.Application/"]
COPY ["src/Services/Events/EventFlow.Events.Domain/EventFlow.Events.Domain.csproj", "src/Services/Events/EventFlow.Events.Domain/"]
COPY ["src/Services/Events/EventFlow.Events.Infrastructure/EventFlow.Events.Infrastructure.csproj", "src/Services/Events/EventFlow.Events.Infrastructure/"]
RUN dotnet restore "src/Services/Events/EventFlow.Events.Api/EventFlow.Events.Api.csproj"

COPY . .
WORKDIR "/src/src/Services/Events/EventFlow.Events.Api"
RUN dotnet publish "EventFlow.Events.Api.csproj" \
    -c "$BUILD_CONFIGURATION" \
    --no-restore \
    -o /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
RUN apt-get update \
    && apt-get install -y --no-install-recommends libgssapi-krb5-2 \
    && rm -rf /var/lib/apt/lists/*
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "EventFlow.Events.Api.dll"]
