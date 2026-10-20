using Microsoft.Extensions.Options;
using MongoDB.Driver;
using VetSync.Domain.Documents;
using VetSync.Domain.Repositories;

namespace VetSync.Infrastructure.Mongo;

/// <summary>Persistência NoSQL (documentos) da linha do tempo do pet.</summary>
public class MongoJornadaEventoRepository : IJornadaEventoRepository
{
    private readonly IMongoCollection<JornadaEvento> _collection;

    public MongoJornadaEventoRepository(IMongoClient client, IOptions<MongoSettings> options)
    {
        var settings = options.Value;
        _collection = client.GetDatabase(settings.Database)
            .GetCollection<JornadaEvento>(settings.JornadaCollection);
    }

    public Task AddAsync(JornadaEvento evento, CancellationToken cancellationToken = default)
        => _collection.InsertOneAsync(evento, cancellationToken: cancellationToken);

    public async Task<IReadOnlyList<JornadaEvento>> GetByPetAsync(int petId, int limite, CancellationToken cancellationToken = default)
        => await _collection.Find(e => e.PetId == petId)
            .SortByDescending(e => e.OcorridoEm)
            .Limit(limite)
            .ToListAsync(cancellationToken);

    /// <summary>Cria o índice (PetId, OcorridoEm desc) usado pela consulta da linha do tempo.</summary>
    public Task EnsureIndexesAsync(CancellationToken cancellationToken = default)
    {
        var keys = Builders<JornadaEvento>.IndexKeys
            .Ascending(e => e.PetId)
            .Descending(e => e.OcorridoEm);

        return _collection.Indexes.CreateOneAsync(
            new CreateIndexModel<JornadaEvento>(keys, new CreateIndexOptions { Name = "IDX_JORNADA_PET_DATA" }),
            cancellationToken: cancellationToken);
    }
}
