using Microsoft.AspNetCore.Http;
using VetSync.Application.Common;
using VetSync.Application.DTOs;

namespace VetSync.Api.Hateoas;

/// <summary>Links HATEOAS dos eventos da linha do tempo do pet (MongoDB).</summary>
public static class TimelineLinks
{
    public static JornadaEventoDto WithLinks(this JornadaEventoDto dto, HttpRequest req)
    {
        var pet = $"{req.PathBase}/api/pets/{dto.PetId}";
        dto.Links = new List<LinkDto>
        {
            new($"{pet}/timeline", "self", "GET"),
            new(pet, "pet", "GET"),
            new($"{pet}/jornada", "jornada", "GET")
        };
        return dto;
    }
}