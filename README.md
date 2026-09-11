# VetSync API — FIAP Sprint 3

API RESTful para gestao de clinica veterinaria, com busca de clinicas veterinarias reais por geolocalizacao (OpenStreetMap), desenvolvida com ASP.NET Core 8, Oracle Database e Entity Framework Core.

## Equipe

| Nome | RM |
|------|----|
| Arthur Brito | RM 562085 |
| Luiz Felipe Flosi | RM 563197 |
| Pedro Brum | RM 561780 |

---

## Novidades da Sprint 3

- Busca de clinicas veterinarias reais por geolocalizacao: `GET /api/clinicas/proximas`
  consulta em tempo real a Overpass API do OpenStreetMap - uma base de dados
  geografica publica e colaborativa - e retorna as clinicas veterinarias
  realmente cadastradas dentro de um raio a partir da localizacao informada.
  Nenhum dado e ficticio ou pre-cadastrado pela aplicacao: se nao houver
  clinica mapeada na regiao, a resposta vem vazia.
- Pagina de demonstracao (`/buscar-clinicas.html`) que pede a localizacao
  real do navegador (Geolocation API / GPS) e chama a API automaticamente,
  mostrando as clinicas reais encontradas perto de voce.
- Monitoramento e Observabilidade: Health Checks, logging estruturado
  (Serilog) e tracing/metricas (OpenTelemetry).
- Testes Automatizados: suite de testes unitarios (xUnit + Moq) e de
  integracao (xUnit + WebApplicationFactory), seguindo o padrao AAA
  (Arrange, Act, Assert).

---

## Tecnologias

- ASP.NET Core 8 (Controllers)
- Entity Framework Core 8 + Oracle.EntityFrameworkCore
- Oracle Database XE / XEPDB1
- AutoMapper 12
- Swagger / OpenAPI (Swashbuckle)
- Overpass API (OpenStreetMap) - fonte de dados real de clinicas veterinarias, sem chave de API
- Serilog (logging estruturado, console + arquivo)
- Health Checks (Microsoft.Extensions.Diagnostics.HealthChecks)
- OpenTelemetry (tracing e metricas, exportador console)
- xUnit + Moq + FluentAssertions (testes unitarios)
- Microsoft.AspNetCore.Mvc.Testing + EF Core InMemory (testes de integracao)

## Pre-requisitos

- .NET 8 SDK
- Oracle Database (local ou via Docker)
- Conexao com a internet (necessaria para a busca de clinicas via OpenStreetMap)

### Oracle via Docker (recomendado)

```bash
docker run -d \
  --name oracle-xe \
  -p 1521:1521 \
  -e ORACLE_PASSWORD=fiap1234 \
  gvenzl/oracle-xe:21-slim
```

Aguarde ~60 segundos e conecte com:

- User: system
- Password: fiap1234
- Host: localhost:1521/XEPDB1

---

## Instalacao e Execucao

### 1. Clonar o repositorio

```bash
git clone https://github.com/seu-usuario/VetSync-Dotnet.git
cd VetSync-Dotnet
```

### 2. Configurar a connection string

Edite `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "OracleConnection": "User Id=system;Password=fiap1234;Data Source=localhost:1521/XEPDB1;"
  }
}
```

### 3. Aplicar migrations

```bash
dotnet ef database update
```

### 4. Executar

```bash
dotnet run
```

- Swagger UI: http://localhost:5000
- Pagina de busca de clinicas reais (pede sua localizacao): http://localhost:5000/buscar-clinicas.html
- Health check: http://localhost:5000/health

---

## Clinicas Veterinarias Proximas (Geolocalizacao real)

### Como funciona

1. O cliente (a pagina buscar-clinicas.html, o Swagger, um app mobile, etc.) informa
   latitude/longitude - no caso da pagina de demonstracao, essas coordenadas vem
   de verdade do GPS do navegador via navigator.geolocation.getCurrentPosition().
2. A API monta uma consulta em Overpass QL pedindo todos os elementos com a tag
   amenity=veterinary dentro do raio informado, e envia via HTTP POST para a
   Overpass API (https://overpass-api.de/api/interpreter, com um espelho de
   fallback caso o principal esteja fora do ar).
3. A resposta (JSON com os elementos do OpenStreetMap: nome, endereco, telefone,
   coordenadas) e convertida para o formato da API, e a distancia exata ate o
   usuario e recalculada com a formula de Haversine (GeoLocationService), ja
   que o filtro around: do Overpass e aproximado.
4. O resultado e ordenado da clinica mais proxima para a mais distante e devolvido.

Nao existe nenhuma tabela local de clinicas "parceiras" ou ficticias - o dado
vem sempre, em tempo real, da fonte publica.

### GET /api/clinicas/proximas

| Parametro | Tipo | Obrigatorio | Padrao | Descricao |
|-----------|------|:---:|:---:|-----------|
| latitude | double | sim | - | Latitude do usuario (-90 a 90) |
| longitude | double | sim | - | Longitude do usuario (-180 a 180) |
| raioKm | double | nao | 10 | Raio de busca em quilometros (max. 50) |

Exemplo de requisicao:
Exemplo de resposta (200 OK) - dados reais retornados pelo OpenStreetMap para essa coordenada (o conteudo exato varia conforme o que esta mapeado na regiao no momento da consulta):

```json
[
  {
    "id": "node/1234567890",
    "nome": "Clinica Veterinaria Pet Amigo",
    "endereco": "Av. Paulista, 1000 - Bela Vista - Sao Paulo",
    "telefone": "+55 11 5555-1234",
    "siteOuRedeSocial": "https://www.petamigo.com.br",
    "latitude": -23.5605,
    "longitude": -46.6540,
    "distanciaKm": 0.87,
    "fonte": "OpenStreetMap",
    "linkMapa": "https://www.openstreetmap.org/node/1234567890"
  }
]
```

Se nenhuma clinica estiver mapeada na regiao dentro do raio, a resposta e 200 OK
com uma lista vazia [] - isso e o comportamento correto e esperado (nao e erro).

Erros:

| Status | Quando ocorre |
|--------|---------------|
| 400 Bad Request | Latitude fora de -90/90, longitude fora de -180/180, ou raioKm <= 0 ou > 50 |
| 503 Service Unavailable | A Overpass API (OpenStreetMap) esta temporariamente fora do ar |

### Pagina de demonstracao (/buscar-clinicas.html)

Ao abrir essa pagina no navegador:

1. O navegador pede permissao de localizacao ao usuario (prompt nativo do navegador/SO).
2. Se autorizado, a pagina captura a latitude/longitude reais (GPS/Wi-Fi/IP, conforme o dispositivo).
3. Chama automaticamente GET /api/clinicas/proximas com essas coordenadas.
4. Mostra a lista de clinicas reais retornadas, com distancia, endereco, telefone e link para o mapa.

Atencao: geolocalizacao do navegador exige HTTPS (ou localhost). Rodando localmente via dotnet run ou docker-compose, funciona normalmente em http://localhost.

---

## Monitoramento e Observabilidade

### Health Checks

| Endpoint | Finalidade |
|----------|------------|
| GET /health | Status geral em JSON detalhado (nome de cada verificacao, status, duracao e erro, se houver) |
| GET /health/live | Liveness - o processo da API esta de pe (nao depende de recursos externos) |
| GET /health/ready | Readiness - as dependencias (Oracle + Overpass API) estao disponiveis para receber trafego |

Verificacoes registradas:

| Nome | O que verifica | Se falhar |
|------|-----------------|-----------|
| self | O processo da API esta rodando | Unhealthy |
| oracle-database | Conectividade real com o banco Oracle (via EF Core) | Unhealthy |
| overpass-api | Disponibilidade do servico externo (Overpass API / OpenStreetMap) usado na busca de clinicas | Degraded (nao derruba o readiness — a API continua servindo as demais rotas mesmo se essa dependencia externa estiver fora do ar) |

Exemplo de resposta de GET /health:

```json
{
  "status": "Healthy",
  "totalDurationMs": 145.32,
  "checks": [
    { "name": "self", "status": "Healthy", "durationMs": 0.01, "description": "API em execucao", "error": null },
    { "name": "oracle-database", "status": "Healthy", "durationMs": 11.2, "description": null, "error": null },
    { "name": "overpass-api", "status": "Healthy", "durationMs": 132.8, "description": "Overpass API (OpenStreetMap) respondendo normalmente.", "error": null }
  ]
}
```

### Logging Estruturado (Serilog)

- Toda requisicao HTTP e logada (metodo, rota, status code e tempo de resposta) via UseSerilogRequestLogging.
- Saida em console e em arquivo (logs/vetsync-YYYYMMDD.log, com rotacao diaria e retencao de 14 dias).
- O ClinicasController e o OverpassVeterinaryClinicSearchService logam eventos de negocio relevantes (buscas realizadas, coordenadas invalidas, falhas ao consultar o OpenStreetMap) com propriedades estruturadas (Latitude, Longitude, RaioKm, Endpoint, etc.).

### Tracing e Metricas (OpenTelemetry)

- Instrumentacao automatica de ASP.NET Core, HttpClient (inclusive as chamadas a Overpass API) e Entity Framework Core.
- Metricas de runtime, HTTP e ASP.NET Core expostas via AddRuntimeInstrumentation / AddAspNetCoreInstrumentation.
- Exportador padrao: Console, pronto para trocar por um exportador OTLP (Application Insights, Jaeger, Grafana Tempo, etc.) apenas alterando o WithTracing/WithMetrics em Program.cs.

---

## Testes Automatizados

tests/
+-- VetApi.Tests.Unit/ -> xUnit + Moq + FluentAssertions
¦ +-- Services/
¦ ¦ +-- GeoLocationServiceTests.cs calculo de distancia (Haversine)
¦ ¦ +-- OverpassVeterinaryClinicSearchServiceTests.cs parsing/logica do servico externo
¦ ¦ +-- FakeHttpMessageHandler.cs duble de transporte HTTP
¦ +-- Controllers/
¦ +-- ClinicasControllerTests.cs validacao e status codes do controller
+-- VetApi.Tests.Integration/ -> xUnit + WebApplicationFactory + EF InMemory
+-- CustomWebApplicationFactory.cs troca so o Oracle por EF InMemory
+-- IntegrationTestCollection.cs Collection Fixture (servidor compartilhado)
+-- ClinicasEndpointsTests.cs chama a Overpass API REAL pela internet
+-- HealthChecksTests.cs testes dos endpoints /health
### Sobre "mockado" vs. dados reais - como cada camada de teste funciona

- GeoLocationServiceTests: testa matematica pura (Haversine), sem dependencias.
- OverpassVeterinaryClinicSearchServiceTests: testa a logica de parsing/filtro/ordenacao do servico isolando apenas o transporte HTTP com um FakeHttpMessageHandler (pratica padrao para nao depender de rede em todo teste unitario) - o payload usado e uma amostra fiel do formato real de resposta da Overpass API, nao um dado de negocio inventado.
- ClinicasControllerTests: mocka o IVeterinaryClinicSearchService (Moq) para testar isoladamente a validacao de entrada e os status codes do controller.
- ClinicasEndpointsTests (integracao): nao mocka nada - o teste sobe a aplicacao real via WebApplicationFactory e faz uma chamada HTTP de ponta a ponta que sai de verdade para a internet e consulta a Overpass API real. Por isso, requer conexao com a internet para rodar, e pode falhar por instabilidade externa (nesse caso, rode novamente).

A aplicacao em execucao normal (dotnet run / Docker) sempre usa a implementacao real (OverpassVeterinaryClinicSearchService) - nao ha nenhum modo "mockado" no codigo de producao.

### Executar os testes

```bash
# Todos os testes (unitarios + integracao - a integracao precisa de internet)
dotnet test

# Apenas os testes unitarios (rapidos, sem rede)
dotnet test tests/VetApi.Tests.Unit

# Apenas os testes de integracao (precisa de internet)
dotnet test tests/VetApi.Tests.Integration

# Com relatorio de cobertura
dotnet test --collect:"XPlat Code Coverage"
```

---

## Rotas da API (demais entidades)

### Tutores /api/tutores

| Metodo | Rota | Descricao | Status |
|--------|------|-----------|--------|
| GET | /api/tutores | Lista todos | 200 |
| GET | /api/tutores/{id} | Busca por ID | 200 / 404 |
| GET | /api/tutores/email/{email} | Busca por email | 200 / 404 |
| GET | /api/tutores/ativos | Lista ativos | 200 |
| GET | /api/tutores/buscar?nome=x&ativo=true | Busca avancada | 200 |
| GET | /api/tutores/{id}/pets | Pets do tutor | 200 / 404 |
| POST | /api/tutores | Cadastrar tutor | 201 / 400 |
| PUT | /api/tutores/{id} | Atualizar tutor | 204 / 400 / 404 |
| DELETE | /api/tutores/{id} | Remover tutor | 204 / 404 |

### Pets /api/pets

| Metodo | Rota | Descricao | Status |
|--------|------|-----------|--------|
| GET | /api/pets | Lista todos | 200 |
| GET | /api/pets/{id} | Busca por ID | 200 / 404 |
| GET | /api/pets/especie/{especie} | Filtra por especie | 200 |
| GET | /api/pets/ativos | Lista ativos | 200 |
| GET | /api/pets/buscar?nome=x&especie=y&raca=z | Busca avancada | 200 |
| GET | /api/pets/{id}/jornada | Jornada completa do pet | 200 / 404 |
| POST | /api/pets | Cadastrar pet | 201 / 400 / 404 |
| PUT | /api/pets/{id} | Atualizar pet | 204 / 400 / 404 |
| DELETE | /api/pets/{id} | Remover pet | 204 / 404 |

### Consultas /api/consultas

| Metodo | Rota | Descricao | Status |
|--------|------|-----------|--------|
| GET | /api/consultas | Lista todas | 200 |
| GET | /api/consultas/{id} | Busca por ID | 200 / 404 |
| GET | /api/consultas/status/{status} | Filtra por status | 200 |
| GET | /api/consultas/periodo?de=...&ate=... | Filtra por periodo | 200 / 400 |
| GET | /api/consultas/pet/{petId} | Consultas de um pet | 200 / 404 |
| POST | /api/consultas | Agendar consulta | 201 / 400 / 404 |
| PUT | /api/consultas/{id} | Atualizar consulta | 204 / 400 / 404 |
| DELETE | /api/consultas/{id} | Remover consulta | 204 / 404 |

### Vacinacoes /api/vacinacoes

| Metodo | Rota | Descricao | Status |
|--------|------|-----------|--------|
| GET | /api/vacinacoes | Lista todas | 200 |
| GET | /api/vacinacoes/{id} | Busca por ID | 200 / 404 |
| GET | /api/vacinacoes/pet/{petId} | Vacinas de um pet | 200 / 404 |
| GET | /api/vacinacoes/proximas-doses?ate=... | Proximas doses | 200 |
| POST | /api/vacinacoes | Registrar vacinacao | 201 / 400 / 404 |
| DELETE | /api/vacinacoes/{id} | Remover vacinacao | 204 / 404 |

### Exames /api/exames

| Metodo | Rota | Descricao | Status |
|--------|------|-----------|--------|
| GET | /api/exames | Lista todos | 200 |
| GET | /api/exames/{id} | Busca por ID | 200 / 404 |
| GET | /api/exames/pet/{petId} | Exames de um pet | 200 / 404 |
| GET | /api/exames/tipo/{tipo} | Filtra por tipo | 200 |
| POST | /api/exames | Registrar exame | 201 / 400 / 404 |
| PUT | /api/exames/{id} | Atualizar resultado | 204 / 400 / 404 |
| DELETE | /api/exames/{id} | Remover exame | 204 / 404 |

### Clinicas /api/clinicas

| Metodo | Rota | Descricao | Status |
|--------|------|-----------|--------|
| GET | /api/clinicas/proximas?latitude=..&longitude=..&raioKm=10 | Busca clinicas reais por proximidade (OpenStreetMap) | 200 / 400 / 503 |

---

## Estrutura do Projeto

VetSync-Dotnet/
+-- Controllers/
¦ +-- TutoresController.cs
¦ +-- PetsController.cs
¦ +-- ConsultasController.cs
¦ +-- VacinacoesController.cs
¦ +-- ExamesController.cs
¦ +-- ClinicasController.cs busca real por geolocalizacao
+-- Services/
¦ +-- IGeoLocationService.cs / GeoLocationService.cs calculo de distancia (Haversine)
¦ +-- IVeterinaryClinicSearchService.cs /
¦ OverpassVeterinaryClinicSearchService.cs busca real via OpenStreetMap
+-- Data/
¦ +-- AppDbContext.cs
+-- DTOs/
¦ +-- TutorDtos.cs
¦ +-- PetDtos.cs
¦ +-- ConsultaDtos.cs
¦ +-- JornadaDtos.cs VacinacaoDto + ExameDto
¦ +-- JornadaPetDto.cs Jornada completa
¦ +-- ClinicaVeterinariaDto.cs resposta do endpoint de proximidade
+-- Mappings/
¦ +-- MappingProfile.cs
+-- Migrations/
¦ +-- 20240101000000_InitialCreate.cs
¦ +-- AppDbContextModelSnapshot.cs
+-- Models/
¦ +-- Tutor.cs
¦ +-- Pet.cs
¦ +-- Consulta.cs
¦ +-- Vacinacao.cs
¦ +-- Exame.cs
+-- wwwroot/
¦ +-- buscar-clinicas.html pagina de demonstracao (pede geolocalizacao real)
+-- tests/
¦ +-- VetApi.Tests.Unit/
¦ +-- VetApi.Tests.Integration/
+-- logs/ gerado em runtime pelo Serilog
+-- appsettings.json
+-- VetApi.csproj
+-- VetApi.sln
+-- Program.cs
---

## Comandos EF Core

```bash
dotnet ef migrations add NomeDaMigration
dotnet ef database update
dotnet ef migrations remove
```

---

## Subindo tudo com Docker (recomendado para apresentacao)

### Pre-requisito

Docker Desktop instalado, com acesso a internet (necessario para a Overpass API)

### 1 comando para rodar tudo

```bash
docker-compose up --build
```

Isso vai:

- Baixar e iniciar o Oracle XE automaticamente
- Fazer o build da API
- Aplicar as migrations no banco
- Subir a API

Acesse:

- Swagger UI: http://localhost:8080
- Busca de clinicas reais (pede localizacao): http://localhost:8080/buscar-clinicas.html
- Health Check: http://localhost:8080/health

Na primeira vez, o Oracle demora ~2 minutos para inicializar. A API vai aguardar automaticamente e aplicar as migrations assim que o banco estiver pronto.

### Parar tudo

```bash
docker-compose down
```

### Parar e apagar o banco (reset completo)

```bash
docker-compose down -v
```
