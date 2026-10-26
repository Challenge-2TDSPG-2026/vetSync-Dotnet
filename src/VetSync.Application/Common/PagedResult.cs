using VetSync.Domain.Repositories;

namespace VetSync.Application.Common;

/// <summary>Resposta paginada padrão da API, com metadados e links de navegação (HATEOAS).</summary>
public class PagedResult<T> : LinkedResourceDto
{
    public IReadOnlyList<T> Items { get; set; } = Array.Empty<T>();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPrevious => Page > 1;
    public bool HasNext => Page < TotalPages;

    public static PagedResult<T> Create(IReadOnlyList<T> items, int totalCount, PageRequest page)
        => new() { Items = items, TotalCount = totalCount, Page = page.Page, PageSize = page.PageSize };
}
