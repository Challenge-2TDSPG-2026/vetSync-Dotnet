namespace VetApi.Services;

public class ClinicaVeterinariaEncontrada
{
    public string Id { get; set; } = string.Empty;

    public string Nome { get; set; } = "Clínica Veterinária";
    public string? Endereco { get; set; }
    public string? Telefone { get; set; }
    public string? SiteOuRedeSocial { get; set; }

    public double Latitude { get; set; }
    public double Longitude { get; set; }

    public double DistanciaKm { get; set; }

    public string Fonte { get; } = "OpenStreetMap";

    public string LinkMapa => $"https://www.openstreetmap.org/{Id}";
}

public interface IVeterinaryClinicSearchService
{
    Task<List<ClinicaVeterinariaEncontrada>> BuscarProximasAsync(
        double latitude, double longitude, double raioKm, CancellationToken cancellationToken = default);
}
