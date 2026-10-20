using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace VetSync.Api.Controllers;

/// <summary>Base dos controllers de recursos: JSON, autenticação obrigatória e rota padrão.</summary>
[ApiController]
[Authorize]
[Route("api/[controller]")]
[Produces("application/json")]
public abstract class ApiControllerBase : ControllerBase
{
}
