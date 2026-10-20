namespace VetSync.Domain.Repositories;

/// <summary>Parâmetros de paginação e ordenação de uma consulta.</summary>
public class PageRequest
{
    public const int TamanhoMaximo = 100;

    private int _page = 1;
    private int _pageSize = 10;

    public int Page
    {
        get => _page;
        set => _page = value < 1 ? 1 : value;
    }

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value < 1 ? 10 : Math.Min(value, TamanhoMaximo);
    }

    public string? SortBy { get; set; }
    public bool Descending { get; set; }

    public int Skip => (Page - 1) * PageSize;
}

/// <summary>Página de resultados + total de registros que atendem ao filtro.</summary>
public record PagedList<T>(IReadOnlyList<T> Items, int TotalCount);
