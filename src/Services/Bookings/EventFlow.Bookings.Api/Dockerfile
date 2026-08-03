FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["src/Shared/EventFlow.Contracts/EventFlow.Contracts.csproj", "src/Shared/EventFlow.Contracts/"]
COPY ["src/Services/Bookings/EventFlow.Bookings.Api/EventFlow.Bookings.Api.csproj", "src/Services/Bookings/EventFlow.Bookings.Api/"]
COPY ["src/Services/Bookings/EventFlow.Bookings.Application/EventFlow.Bookings.Application.csproj", "src/Services/Bookings/EventFlow.Bookings.Application/"]
COPY ["src/Services/Bookings/EventFlow.Bookings.Domain/EventFlow.Bookings.Domain.csproj", "src/Services/Bookings/EventFlow.Bookings.Domain/"]
COPY ["src/Services/Bookings/EventFlow.Bookings.Infrastructure/EventFlow.Bookings.Infrastructure.csproj", "src/Services/Bookings/EventFlow.Bookings.Infrastructure/"]
RUN dotnet restore "src/Services/Bookings/EventFlow.Bookings.Api/EventFlow.Bookings.Api.csproj"

COPY . .
WORKDIR "/src/src/Services/Bookings/EventFlow.Bookings.Api"
RUN dotnet publish "EventFlow.Bookings.Api.csproj" \
    -c "$BUILD_CONFIGURATION" \
    --no-restore \
    -o /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "EventFlow.Bookings.Api.dll"]
