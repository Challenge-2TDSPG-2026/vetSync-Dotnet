namespace VetSync.Application.Common;

/// <summary>Link HATEOAS: aponta para uma ação relacionada ao recurso.</summary>
public class LinkDto
{
    public string Href { get; set; } = string.Empty;
    public string Rel { get; set; } = string.Empty;
    public string Method { get; set; } = "GET";

    public LinkDto() { }

    public LinkDto(string href, string rel, string method)
    {
        Href = href;
        Rel = rel;
        Method = method;
    }
}

/// <summary>Base dos DTOs de consulta: carrega os links HATEOAS do recurso.</summary>
public abstract class LinkedResourceDto
{
    public List<LinkDto> Links { get; set; } = new();
}
