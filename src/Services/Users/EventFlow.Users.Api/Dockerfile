FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["src/Services/Users/EventFlow.Users.Api/EventFlow.Users.Api.csproj", "src/Services/Users/EventFlow.Users.Api/"]
COPY ["src/Services/Users/EventFlow.Users.Application/EventFlow.Users.Application.csproj", "src/Services/Users/EventFlow.Users.Application/"]
COPY ["src/Services/Users/EventFlow.Users.Domain/EventFlow.Users.Domain.csproj", "src/Services/Users/EventFlow.Users.Domain/"]
COPY ["src/Services/Users/EventFlow.Users.Infrastructure/EventFlow.Users.Infrastructure.csproj", "src/Services/Users/EventFlow.Users.Infrastructure/"]
RUN dotnet restore "src/Services/Users/EventFlow.Users.Api/EventFlow.Users.Api.csproj"

COPY . .
WORKDIR "/src/src/Services/Users/EventFlow.Users.Api"
RUN dotnet publish "EventFlow.Users.Api.csproj" \
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
ENTRYPOINT ["dotnet", "EventFlow.Users.Api.dll"]
