using VetSync.Domain.Exceptions;
using VetSync.Domain.Repositories;

namespace VetSync.Application.Common;

/// <summary>Parâmetros comuns de paginação/ordenação (query string): ?page=1&amp;pageSize=10&amp;sortBy=nome&amp;sortDir=asc.</summary>
public class PageQuery
{
    /// <summary>Número da página (começa em 1).</summary>
    public int Page { get; set; } = 1;

    /// <summary>Itens por página (máximo 100).</summary>
    public int PageSize { get; set; } = 10;

    /// <summary>Campo de ordenação (ver campos aceitos por recurso na documentação).</summary>
    public string? SortBy { get; set; }

    /// <summary>Direção da ordenação: asc ou desc.</summary>
    public string? SortDir { get; set; }

    /// <summary>Converte para <see cref="PageRequest"/> validando o campo e a direção de ordenação.</summary>
    public PageRequest ToPageRequest(IReadOnlyCollection<string> camposOrdenaveis, string campoPadrao, bool descPadrao = false)
    {
        var sortBy = string.IsNullOrWhiteSpace(SortBy) ? campoPadrao : SortBy.Trim();
        var campo = camposOrdenaveis.FirstOrDefault(c => c.Equals(sortBy, StringComparison.OrdinalIgnoreCase));
        if (campo is null)
            throw new BusinessRuleException(
                $"Campo de ordenação inválido: '{sortBy}'. Aceitos: {string.Join(", ", camposOrdenaveis)}.");

        bool descending;
        if (string.IsNullOrWhiteSpace(SortDir))
            descending = string.IsNullOrWhiteSpace(SortBy) ? descPadrao : false;
        else if (SortDir.Equals("asc", StringComparison.OrdinalIgnoreCase))
            descending = false;
        else if (SortDir.Equals("desc", StringComparison.OrdinalIgnoreCase))
            descending = true;
        else
            throw new BusinessRuleException("Direção de ordenação inválida. Use 'asc' ou 'desc'.");

        return new PageRequest { Page = Page, PageSize = PageSize, SortBy = campo, Descending = descending };
    }
}
