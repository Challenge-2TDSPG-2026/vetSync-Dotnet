namespace VetApi.DTOs;

public class ClinicaVeterinariaDto
{
    public string Id { get; set; } = string.Empty;

    public string Nome { get; set; } = string.Empty;

    public string? Endereco { get; set; }

    public string? Telefone { get; set; }

    public string? SiteOuRedeSocial { get; set; }

    public double Latitude { get; set; }
    public double Longitude { get; set; }

    public double DistanciaKm { get; set; }

    public string Fonte { get; set; } = "OpenStreetMap";

    public string LinkMapa { get; set; } = string.Empty;
}
