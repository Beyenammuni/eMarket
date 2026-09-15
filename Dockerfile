# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY eMarket.slnx ./
COPY src/eMarket.Api/eMarket.Api.csproj src/eMarket.Api/
COPY src/eMarket.Application/eMarket.Application.csproj src/eMarket.Application/
COPY src/eMarket.Domain/eMarket.Domain.csproj src/eMarket.Domain/
COPY src/eMarket.Infrastructure/eMarket.Infrastructure.csproj src/eMarket.Infrastructure/
COPY src/eMarket.SharedKernel/eMarket.SharedKernel.csproj src/eMarket.SharedKernel/
COPY tests/eMarket.UnitTests/eMarket.UnitTests.csproj tests/eMarket.UnitTests/
COPY tests/eMarket.IntegrationTests/eMarket.IntegrationTests.csproj tests/eMarket.IntegrationTests/
COPY tests/eMarket.ArchitectureTests/eMarket.ArchitectureTests.csproj tests/eMarket.ArchitectureTests/
RUN dotnet restore eMarket.slnx

COPY . .
RUN dotnet publish src/eMarket.Api/eMarket.Api.csproj -c Release -o /app/publish --no-restore /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_HTTP_PORTS=8080 \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
EXPOSE 8080

COPY --from=build /app/publish .

# .NET official images expose a non-root app user.
USER $APP_UID
ENTRYPOINT ["dotnet", "eMarket.Api.dll"]
