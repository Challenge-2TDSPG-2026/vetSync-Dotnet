namespace VetSync.Infrastructure.Mongo;

public class MongoSettings
{
    public const string SectionName = "Mongo";

    public string ConnectionString { get; set; } = "mongodb://localhost:27017";
    public string Database { get; set; } = "vetsync";
    public string JornadaCollection { get; set; } = "jornada_eventos";
    public int ServerSelectionTimeoutSeconds { get; set; } = 3;
}
