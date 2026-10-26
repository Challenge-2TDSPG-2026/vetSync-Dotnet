namespace VetSync.Application.Abstractions;

public interface IGeoLocationService
{
    double CalcularDistanciaKm(double latitudeOrigem, double longitudeOrigem, double latitudeDestino, double longitudeDestino);

    bool CoordenadasValidas(double latitude, double longitude);
}
