FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY LedgerFlow.sln ./
COPY src/LedgerFlow.API/LedgerFlow.API.csproj src/LedgerFlow.API/
COPY src/LedgerFlow.Application/LedgerFlow.Application.csproj src/LedgerFlow.Application/
COPY src/LedgerFlow.Domain/LedgerFlow.Domain.csproj src/LedgerFlow.Domain/
COPY src/LedgerFlow.Infrastructure/LedgerFlow.Infrastructure.csproj src/LedgerFlow.Infrastructure/
COPY src/LedgerFlow.Outbox/LedgerFlow.Outbox.csproj src/LedgerFlow.Outbox/
RUN dotnet restore src/LedgerFlow.API/LedgerFlow.API.csproj

COPY src/ src/
RUN dotnet publish src/LedgerFlow.API/LedgerFlow.API.csproj \
    --configuration Release \
    --output /app/publish \
    --no-restore \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
RUN apt-get update \
    && apt-get install --yes --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app/publish .
HEALTHCHECK --interval=10s --timeout=5s --retries=5 \
    CMD curl --fail http://localhost:8080/health || exit 1
ENTRYPOINT ["dotnet", "LedgerFlow.API.dll"]