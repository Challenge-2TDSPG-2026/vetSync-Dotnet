using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using VetSync.Application.Common;
using VetSync.Application.DTOs;

namespace VetSync.Api.Hateoas;

/// <summary>
/// Monta os links HATEOAS (RFC 5988-like) de cada recurso. Mantido fora dos controllers
/// para que a regra de "quais ações são oferecidas" fique em um único lugar.
/// </summary>
public static class ResourceLinks
{
    private const string Get = "GET";
    private const string Post = "POST";
    private const string Put = "PUT";
    private const string Delete = "DELETE";

    // ---------- Recursos ----------

    public static TutorDto WithLinks(this TutorDto dto, HttpRequest req)
    {
        var self = Url(req, "tutores", dto.Id);
        dto.Links = new List<LinkDto>
        {
            new(self, "self", Get),
            new(self, "update", Put),
            new(self, "delete", Delete),
            new(Url(req, "pets") + $"?tutorId={dto.Id}", "pets", Get)
        };
        return dto;
    }

    public static PetDto WithLinks(this PetDto dto, HttpRequest req)
    {
        var self = Url(req, "pets", dto.Id);
        dto.Links = new List<LinkDto>
        {
            new(self, "self", Get),
            new(self, "update", Put),
            new(self, "delete", Delete),
            new(Url(req, "tutores", dto.TutorId), "tutor", Get),
            new(self + "/jornada", "jornada", Get),
            new(self + "/timeline", "timeline", Get),
            new(Url(req, "consultas") + $"?petId={dto.Id}", "consultas", Get),
            new(Url(req, "vacinacoes") + $"?petId={dto.Id}", "vacinacoes", Get),
            new(Url(req, "exames") + $"?petId={dto.Id}", "exames", Get)
        };
        return dto;
    }

    public static ConsultaDto WithLinks(this ConsultaDto dto, HttpRequest req)
    {
        var self = Url(req, "consultas", dto.Id);
        dto.Links = new List<LinkDto>
        {
            new(self, "self", Get),
            new(self, "update", Put),
            new(self, "delete", Delete),
            new(Url(req, "pets", dto.PetId), "pet", Get)
        };
        return dto;
    }

    public static VacinacaoDto WithLinks(this VacinacaoDto dto, HttpRequest req)
    {
        var self = Url(req, "vacinacoes", dto.Id);
        dto.Links = new List<LinkDto>
        {
            new(self, "self", Get),
            new(self, "delete", Delete),
            new(Url(req, "pets", dto.PetId), "pet", Get)
        };
        return dto;
    }

    public static ExameDto WithLinks(this ExameDto dto, HttpRequest req)
    {
        var self = Url(req, "exames", dto.Id);
        dto.Links = new List<LinkDto>
        {
            new(self, "self", Get),
            new(self, "update", Put),
            new(self, "delete", Delete),
            new(Url(req, "pets", dto.PetId), "pet", Get)
        };
        return dto;
    }

    // ---------- Coleções paginadas ----------

    /// <summary>Aplica links por item e os links de navegação (self/first/prev/next/last) da página.</summary>
    public static PagedResult<T> WithPageLinks<T>(this PagedResult<T> result, HttpRequest req, Action<T> linkItem)
    {
        foreach (var item in result.Items)
            linkItem(item);

        var links = new List<LinkDto>
        {
            new(PageUrl(req, result.Page, result.PageSize), "self", Get),
            new(PageUrl(req, 1, result.PageSize), "first", Get)
        };

        if (result.HasPrevious)
            links.Add(new LinkDto(PageUrl(req, result.Page - 1, result.PageSize), "prev", Get));
        if (result.HasNext)
            links.Add(new LinkDto(PageUrl(req, result.Page + 1, result.PageSize), "next", Get));

        links.Add(new LinkDto(PageUrl(req, Math.Max(1, result.TotalPages), result.PageSize), "last", Get));

        result.Links = links;
        return result;
    }

    public static PagedResult<TutorDto> WithLinks(this PagedResult<TutorDto> r, HttpRequest req)
        => r.WithPageLinks(req, i => i.WithLinks(req));

    public static PagedResult<PetDto> WithLinks(this PagedResult<PetDto> r, HttpRequest req)
        => r.WithPageLinks(req, i => i.WithLinks(req));

    public static PagedResult<ConsultaDto> WithLinks(this PagedResult<ConsultaDto> r, HttpRequest req)
        => r.WithPageLinks(req, i => i.WithLinks(req));

    public static PagedResult<VacinacaoDto> WithLinks(this PagedResult<VacinacaoDto> r, HttpRequest req)
        => r.WithPageLinks(req, i => i.WithLinks(req));

    public static PagedResult<ExameDto> WithLinks(this PagedResult<ExameDto> r, HttpRequest req)
        => r.WithPageLinks(req, i => i.WithLinks(req));

    // ---------- Helpers ----------

    private static string Url(HttpRequest req, string recurso, int? id = null)
        => $"{req.PathBase}/api/{recurso}" + (id.HasValue ? $"/{id.Value}" : string.Empty);

    /// <summary>Reconstrói a URL atual preservando os filtros e trocando apenas page/pageSize.</summary>
    private static string PageUrl(HttpRequest req, int page, int pageSize)
    {
        var parametros = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        foreach (var (chave, valor) in req.Query)
        {
            if (chave.Equals("page", StringComparison.OrdinalIgnoreCase) ||
                chave.Equals("pageSize", StringComparison.OrdinalIgnoreCase))
                continue;
            parametros[chave] = valor.ToString();
        }

        parametros["page"] = page.ToString();
        parametros["pageSize"] = pageSize.ToString();

        return QueryHelpers.AddQueryString($"{req.PathBase}{req.Path}", parametros);
    }
}
