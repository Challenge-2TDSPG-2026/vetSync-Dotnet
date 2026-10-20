FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Restaura primeiro (cache de camadas): copia só os .csproj e a solution
COPY VetSync.sln .
COPY src/VetSync.Domain/VetSync.Domain.csproj src/VetSync.Domain/
COPY src/VetSync.Application/VetSync.Application.csproj src/VetSync.Application/
COPY src/VetSync.Infrastructure/VetSync.Infrastructure.csproj src/VetSync.Infrastructure/
COPY src/VetSync.Api/VetSync.Api.csproj src/VetSync.Api/
COPY tests/VetSync.Tests.Unit/VetSync.Tests.Unit.csproj tests/VetSync.Tests.Unit/
COPY tests/VetSync.Tests.Integration/VetSync.Tests.Integration.csproj tests/VetSync.Tests.Integration/
RUN dotnet restore VetSync.sln

COPY . .
RUN dotnet publish src/VetSync.Api/VetSync.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

COPY entrypoint.sh .
RUN chmod +x entrypoint.sh

# Não executa como root (boa prática exigida também em DevOps/ACI)
RUN useradd --system --create-home --uid 10001 vetsync \
    && mkdir -p /app/logs && chown -R vetsync:vetsync /app
USER vetsync

ENTRYPOINT ["./entrypoint.sh"]
