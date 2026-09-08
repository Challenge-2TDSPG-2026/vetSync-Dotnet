using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace VetApi.Services;

public class VeterinaryClinicSearchException : Exception
{
    public VeterinaryClinicSearchException(string message, Exception? inner = null) : base(message, inner) { }
}

public class OverpassVeterinaryClinicSearchService : IVeterinaryClinicSearchService
{
    private readonly HttpClient _httpClient;
    private readonly IGeoLocationService _geoLocationService;
    private readonly ILogger<OverpassVeterinaryClinicSearchService> _logger;
    private readonly string[] _endpoints;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public OverpassVeterinaryClinicSearchService(
        HttpClient httpClient,
        IGeoLocationService geoLocationService,
        IConfiguration configuration,
        ILogger<OverpassVeterinaryClinicSearchService> logger)
    {
        _httpClient = httpClient;
        _geoLocationService = geoLocationService;
        _logger = logger;

        _endpoints = configuration.GetSection("Overpass:Endpoints").Get<string[]>()
                     ?? new[]
                     {
                         "https://overpass-api.de/api/interpreter",
                         "https://overpass.kumi.systems/api/interpreter"
                     };
    }

    public async Task<List<ClinicaVeterinariaEncontrada>> BuscarProximasAsync(
        double latitude, double longitude, double raioKm, CancellationToken cancellationToken = default)
    {
        var raioMetros = (int)Math.Round(raioKm * 1000);
        var query = MontarConsultaOverpass(latitude, longitude, raioMetros);

        Exception? ultimoErro = null;

        foreach (var endpoint in _endpoints)
        {
            try
            {
                _logger.LogInformation(
                    "Consultando clínicas veterinárias reais no OpenStreetMap ({Endpoint}) num raio de {RaioKm}km",
                    endpoint, raioKm);

                using var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
                {
                    Content = new FormUrlEncodedContent(new Dictionary<string, string> { ["data"] = query })
                };

                using var response = await _httpClient.SendAsync(request, cancellationToken);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync(cancellationToken);
                var overpassResponse = JsonSerializer.Deserialize<OverpassResponse>(json, JsonOptions);

                var resultado = MapearEOrdenarPorDistancia(overpassResponse, latitude, longitude, raioKm);

                _logger.LogInformation(
                    "{Quantidade} clínica(s) veterinária(s) real(is) encontrada(s) via OpenStreetMap", resultado.Count);

                return resultado;
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
            {
                ultimoErro = ex;
                _logger.LogWarning(ex, "Falha ao consultar {Endpoint}, tentando próximo espelho (se houver)", endpoint);
            }
        }

        throw new VeterinaryClinicSearchException(
            "Não foi possível consultar a base de dados geográfica (OpenStreetMap/Overpass API) no momento.", ultimoErro);
    }

    private static string MontarConsultaOverpass(double latitude, double longitude, int raioMetros)
    {
        var lat = latitude.ToString(CultureInfo.InvariantCulture);
        var lon = longitude.ToString(CultureInfo.InvariantCulture);

        return $@"
[out:json][timeout:25];
(
  node[""amenity""=""veterinary""](around:{raioMetros},{lat},{lon});
  way[""amenity""=""veterinary""](around:{raioMetros},{lat},{lon});
  relation[""amenity""=""veterinary""](around:{raioMetros},{lat},{lon});
);
out center tags;";
    }

    private List<ClinicaVeterinariaEncontrada> MapearEOrdenarPorDistancia(
        OverpassResponse? response, double latitude, double longitude, double raioKm)
    {
        if (response?.Elements is null || response.Elements.Count == 0)
            return new List<ClinicaVeterinariaEncontrada>();

        var resultado = new List<ClinicaVeterinariaEncontrada>();

        foreach (var el in response.Elements)
        {
            var (lat, lon) = ObterCoordenadas(el);
            if (lat is null || lon is null)
                continue;

            var distancia = _geoLocationService.CalcularDistanciaKm(latitude, longitude, lat.Value, lon.Value);
            if (distancia > raioKm)
                continue;

            var tags = el.Tags ?? new Dictionary<string, string>();

            resultado.Add(new ClinicaVeterinariaEncontrada
            {
                Id = $"{el.Type}/{el.Id}",
                Nome = tags.GetValueOrDefault("name", "Clínica Veterinária (sem nome cadastrado)"),
                Endereco = MontarEndereco(tags),
                Telefone = tags.GetValueOrDefault("phone") ?? tags.GetValueOrDefault("contact:phone"),
                SiteOuRedeSocial = tags.GetValueOrDefault("website") ?? tags.GetValueOrDefault("contact:website"),
                Latitude = lat.Value,
                Longitude = lon.Value,
                DistanciaKm = Math.Round(distancia, 2)
            });
        }

        return resultado.OrderBy(c => c.DistanciaKm).ToList();
    }

    private static (double? lat, double? lon) ObterCoordenadas(OverpassElement el)
    {
        if (el.Lat.HasValue && el.Lon.HasValue)
            return (el.Lat, el.Lon);

        if (el.Center is not null)
            return (el.Center.Lat, el.Center.Lon);

        return (null, null);
    }

    private static string? MontarEndereco(Dictionary<string, string> tags)
    {
        var rua = tags.GetValueOrDefault("addr:street");
        var numero = tags.GetValueOrDefault("addr:housenumber");
        var bairro = tags.GetValueOrDefault("addr:suburb");
        var cidade = tags.GetValueOrDefault("addr:city");

        var partes = new List<string>();

        if (!string.IsNullOrWhiteSpace(rua))
            partes.Add(numero is not null ? $"{rua}, {numero}" : rua);
        if (!string.IsNullOrWhiteSpace(bairro))
            partes.Add(bairro);
        if (!string.IsNullOrWhiteSpace(cidade))
            partes.Add(cidade);

        return partes.Count > 0 ? string.Join(" - ", partes) : null;
    }


    private class OverpassResponse
    {
        [JsonPropertyName("elements")]
        public List<OverpassElement>? Elements { get; set; }
    }

    private class OverpassElement
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("lat")]
        public double? Lat { get; set; }

        [JsonPropertyName("lon")]
        public double? Lon { get; set; }

        [JsonPropertyName("center")]
        public OverpassCenter? Center { get; set; }

        [JsonPropertyName("tags")]
        public Dictionary<string, string>? Tags { get; set; }
    }

    private class OverpassCenter
    {
        [JsonPropertyName("lat")]
        public double Lat { get; set; }

        [JsonPropertyName("lon")]
        public double Lon { get; set; }
    }
}
