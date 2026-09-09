# VetSync API — FIAP Sprint 3

API RESTful para gestao de clinica veterinaria, com busca de clinicas veterinarias reais por geolocalizacao (OpenStreetMap), desenvolvida com ASP.NET Core 8, Oracle Database e Entity Framework Core.

## Equipe

| Nome | RM |
|------|----|
| Arthur Brito | RM 562085 |
| Luiz Felipe Flosi | RM 563197 |
| Pedro Brum | RM 561780 |

---

## Como rodar (forma mais rapida e garantida)

O jeito mais simples e confiavel de rodar o projeto, sem precisar instalar .NET, Oracle ou configurar nada manualmente, e via Docker Compose:

```bash
docker-compose up --build
```

Isso sobe o Oracle e a API juntos, ja configurados um para o outro. Veja a secao
"Subindo tudo com Docker" mais abaixo para detalhes e URLs de acesso.

Se preferir rodar localmente com `dotnet run` (sem Docker), veja a secao
"Instalacao e Execucao" abaixo - mas atencao: o projeto usa **.NET 8**, e maquinas
que so tem o .NET 9 instalado (comum em instalacoes recentes) vao dar erro
"You must install or update .NET" ao tentar rodar. Nesse caso, instale o
ASP.NET Core Runtime 8.0 em https://dotnet.microsoft.com/download/dotnet/8.0
antes de rodar `dotnet run`.

---

## Novidades da Sprint 3
