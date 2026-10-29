# Summary
<details open><summary>Summary</summary>

|||
|:---|:---|
| Generated on: | 29/10/2026 - 17:01:59 |
| Coverage date: | 29/10/2026 - 17:01:45 - 29/10/2026 - 17:01:52 |
| Parser: | MultiReport (2x Cobertura) |
| Assemblies: | 4 |
| Classes: | 105 |
| Files: | 66 |
| **Line coverage:** | 81% (1808 of 2231) |
| Covered lines: | 1808 |
| Uncovered lines: | 423 |
| Coverable lines: | 2231 |
| Total lines: | 4157 |
| **Branch coverage:** | 76.4% (289 of 378) |
| Covered branches: | 289 |
| Total branches: | 378 |
| **Method coverage:** | [Feature is only available for sponsors](https://reportgenerator.io/pro) |

</details>

## Coverage
<details><summary>VetSync.Api - 93.7%</summary>

|**Name**|**Line**|**Branch**|
|:---|---:|---:|
|**VetSync.Api**|**93.7%**|**90.9%**|
|Program|94.9%|100%|
|VetSync.Api.Controllers.AuthController|100%||
|VetSync.Api.Controllers.ClinicasController|100%||
|VetSync.Api.Controllers.ConsultasController|100%||
|VetSync.Api.Controllers.ExamesController|73.3%||
|VetSync.Api.Controllers.PetsController|64%||
|VetSync.Api.Controllers.TutoresController|100%||
|VetSync.Api.Controllers.VacinacoesController|54.5%||
|VetSync.Api.Extensions.ServiceCollectionExtensions|100%|100%|
|VetSync.Api.Hateoas.ResourceLinks|100%|100%|
|VetSync.Api.Hateoas.TimelineLinks|100%||
|VetSync.Api.Middleware.CorrelationIdMiddleware|82.1%|50%|
|VetSync.Api.Middleware.CorrelationIdMiddlewareExtensions|100%||
|VetSync.Api.Middleware.GlobalExceptionHandler|97%|93.7%|

</details>
<details><summary>VetSync.Application - 96.3%</summary>

|**Name**|**Line**|**Branch**|
|:---|---:|---:|
|**VetSync.Application**|**96.3%**|**75%**|
|VetSync.Application.Abstractions.ClinicaVeterinariaEncontrada|100%||
|VetSync.Application.Abstractions.TokenResult|100%||
|VetSync.Application.Abstractions.VeterinaryClinicSearchException|100%||
|VetSync.Application.Common.LinkDto|100%||
|VetSync.Application.Common.LinkedResourceDto|100%||
|VetSync.Application.Common.PagedResult`1|100%|50%|
|VetSync.Application.Common.PageQuery|100%|91.6%|
|VetSync.Application.DependencyInjection|100%||
|VetSync.Application.DTOs.AuthResponseDto|100%||
|VetSync.Application.DTOs.ClinicaVeterinariaDto|100%||
|VetSync.Application.DTOs.ConsultaDto|100%||
|VetSync.Application.DTOs.ConsultaQuery|100%||
|VetSync.Application.DTOs.CreateConsultaDto|100%||
|VetSync.Application.DTOs.CreateExameDto|100%||
|VetSync.Application.DTOs.CreatePetDto|100%||
|VetSync.Application.DTOs.CreateTutorDto|100%||
|VetSync.Application.DTOs.CreateVacinacaoDto|100%||
|VetSync.Application.DTOs.ExameDto|100%||
|VetSync.Application.DTOs.ExameQuery|100%||
|VetSync.Application.DTOs.JornadaEventoDto|100%||
|VetSync.Application.DTOs.JornadaPetDto|100%||
|VetSync.Application.DTOs.LoginDto|100%||
|VetSync.Application.DTOs.PetDto|100%||
|VetSync.Application.DTOs.PetQuery|100%||
|VetSync.Application.DTOs.RegisterDto|100%||
|VetSync.Application.DTOs.TutorDto|100%||
|VetSync.Application.DTOs.TutorQuery|100%||
|VetSync.Application.DTOs.UpdateConsultaDto|100%||
|VetSync.Application.DTOs.UpdateExameDto|100%||
|VetSync.Application.DTOs.UpdatePetDto|100%||
|VetSync.Application.DTOs.UpdateTutorDto|100%||
|VetSync.Application.DTOs.VacinacaoDto|100%||
|VetSync.Application.DTOs.VacinacaoQuery|100%||
|VetSync.Application.Mappings.MappingExtensions|100%|70.8%|
|VetSync.Application.Services.AuthService|100%|100%|
|VetSync.Application.Services.ClinicaService|100%|100%|
|VetSync.Application.Services.ConsultaService|98.7%|75%|
|VetSync.Application.Services.ExameService|89.2%|50%|
|VetSync.Application.Services.JornadaEventPublisher|100%||
|VetSync.Application.Services.PetService|88.5%|72.2%|
|VetSync.Application.Services.TutorService|98%|77.2%|
|VetSync.Application.Services.VacinacaoService|84.6%|68.7%|

</details>
<details><summary>VetSync.Domain - 100%</summary>

|**Name**|**Line**|**Branch**|
|:---|---:|---:|
|**VetSync.Domain**|**100%**|**100%**|
|VetSync.Domain.Documents.JornadaEvento|100%||
|VetSync.Domain.Entities.Consulta|100%|100%|
|VetSync.Domain.Entities.Exame|100%||
|VetSync.Domain.Entities.Pet|100%|100%|
|VetSync.Domain.Entities.Roles|100%||
|VetSync.Domain.Entities.Tutor|100%||
|VetSync.Domain.Entities.Usuario|100%|100%|
|VetSync.Domain.Entities.Vacinacao|100%|100%|
|VetSync.Domain.Exceptions.AuthenticationFailedException|100%||
|VetSync.Domain.Exceptions.BusinessRuleException|100%||
|VetSync.Domain.Exceptions.ConflictException|100%||
|VetSync.Domain.Exceptions.DomainException|100%||
|VetSync.Domain.Exceptions.NotFoundException|100%||
|VetSync.Domain.Repositories.ConsultaFilter|100%||
|VetSync.Domain.Repositories.ExameFilter|100%||
|VetSync.Domain.Repositories.PagedList`1|100%||
|VetSync.Domain.Repositories.PageRequest|100%|100%|
|VetSync.Domain.Repositories.PetFilter|100%||
|VetSync.Domain.Repositories.TutorFilter|100%||
|VetSync.Domain.Repositories.VacinacaoFilter|100%||

</details>
<details><summary>VetSync.Infrastructure - 59%</summary>

|**Name**|**Line**|**Branch**|
|:---|---:|---:|
|**VetSync.Infrastructure**|**59%**|**69%**|
|VetSync.Infrastructure.DependencyInjection|92.5%|50%|
|VetSync.Infrastructure.Mongo.MongoHealthCheck|93.7%||
|VetSync.Infrastructure.Mongo.MongoJornadaEventoRepository|78.9%||
|VetSync.Infrastructure.Mongo.MongoSettings|100%||
|VetSync.Infrastructure.Persistence.AppDbContext|63.6%||
|VetSync.Infrastructure.Persistence.Configurations.ConsultaConfiguration|100%||
|VetSync.Infrastructure.Persistence.Configurations.ExameConfiguration|100%||
|VetSync.Infrastructure.Persistence.Configurations.PetConfiguration|100%||
|VetSync.Infrastructure.Persistence.Configurations.TutorConfiguration|100%||
|VetSync.Infrastructure.Persistence.Configurations.UsuarioConfiguration|100%||
|VetSync.Infrastructure.Persistence.Configurations.VacinacaoConfiguration|100%||
|VetSync.Infrastructure.Persistence.DatabaseInitializer|55.3%|58.3%|
|VetSync.Infrastructure.Persistence.Migrations.AddUsuarios|0%||
|VetSync.Infrastructure.Persistence.Migrations.AppDbContextModelSnapshot|0%||
|VetSync.Infrastructure.Persistence.Migrations.InitialCreate|0%||
|VetSync.Infrastructure.Persistence.Repositories.ConsultaRepository|80%|62.5%|
|VetSync.Infrastructure.Persistence.Repositories.EfRepository`1|100%|100%|
|VetSync.Infrastructure.Persistence.Repositories.ExameRepository|89.4%|70%|
|VetSync.Infrastructure.Persistence.Repositories.PetRepository|66.6%|60%|
|VetSync.Infrastructure.Persistence.Repositories.TutorRepository|62.8%|57.1%|
|VetSync.Infrastructure.Persistence.Repositories.UnitOfWork|100%||
|VetSync.Infrastructure.Persistence.Repositories.UsuarioRepository|100%||
|VetSync.Infrastructure.Persistence.Repositories.VacinacaoRepository|82.3%|66.6%|
|VetSync.Infrastructure.Security.JwtSettings|100%||
|VetSync.Infrastructure.Security.JwtTokenGenerator|100%||
|VetSync.Infrastructure.Security.Pbkdf2PasswordHasher|77.7%|50%|
|VetSync.Infrastructure.Services.GeoLocationService|100%|100%|
|VetSync.Infrastructure.Services.Health.OverpassApiHealthCheck|100%|100%|
|VetSync.Infrastructure.Services.OverpassVeterinaryClinicSearchService|96.6%|80%|

</details>
