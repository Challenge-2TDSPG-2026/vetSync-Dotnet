namespace VetApi.Services;

public class GeoLocationService : IGeoLocationService
{
    private const double RaioTerraKm = 6371.0;

    public double CalcularDistanciaKm(double latitudeOrigem, double longitudeOrigem, double latitudeDestino, double longitudeDestino)
    {
        var dLat = ParaRadianos(latitudeDestino - latitudeOrigem);
        var dLon = ParaRadianos(longitudeDestino - longitudeOrigem);

        var lat1 = ParaRadianos(latitudeOrigem);
        var lat2 = ParaRadianos(latitudeDestino);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2) * Math.Cos(lat1) * Math.Cos(lat2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return RaioTerraKm * c;
    }

    public bool CoordenadasValidas(double latitude, double longitude)
    {
        return latitude is >= -90 and <= 90 && longitude is >= -180 and <= 180;
    }

    private static double ParaRadianos(double graus) => graus * Math.PI / 180.0;
}
