using FluentAssertions;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Moq;
using VetSync.Domain.Documents;
using VetSync.Infrastructure.Mongo;
using Xunit;

namespace VetSync.Tests.Unit.Infrastructure;

public class MongoJornadaEventoRepositoryTests
{
    private readonly Mock<IMongoCollection<JornadaEvento>> _collection = new();
    private readonly MongoJornadaEventoRepository _sut;

    public MongoJornadaEventoRepositoryTests()
    {
        var database = new Mock<IMongoDatabase>();
        database.Setup(d => d.GetCollection<JornadaEvento>("jornada_eventos", It.IsAny<MongoCollectionSettings>()))
            .Returns(_collection.Object);

        var client = new Mock<IMongoClient>();
        client.Setup(c => c.GetDatabase("vetsync", It.IsAny<MongoDatabaseSettings>()))
            .Returns(database.Object);

        var settings = Options.Create(new MongoSettings { Database = "vetsync", JornadaCollection = "jornada_eventos" });
        _sut = new MongoJornadaEventoRepository(client.Object, settings);
    }

    [Fact]
    public async Task AddAsync_EventoValido_InsereUmDocumentoNaColecao()
    {
        // Arrange
        var evento = new JornadaEvento { PetId = 1, PetNome = "Rex", TipoEvento = JornadaEvento.VacinaAplicada };

        // Act
        await _sut.AddAsync(evento);

        // Assert
        _collection.Verify(c => c.InsertOneAsync(
            evento, It.IsAny<InsertOneOptions>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task EnsureIndexesAsync_Chamado_CriaIndiceCompostoPetEData()
    {
        // Arrange
        CreateIndexModel<JornadaEvento>? capturado = null;
        var indices = new Mock<IMongoIndexManager<JornadaEvento>>();
        indices.Setup(i => i.CreateOneAsync(
                It.IsAny<CreateIndexModel<JornadaEvento>>(),
                It.IsAny<CreateOneIndexOptions>(),
                It.IsAny<CancellationToken>()))
            .Callback<CreateIndexModel<JornadaEvento>, CreateOneIndexOptions, CancellationToken>((m, _, _) => capturado = m)
            .ReturnsAsync("IDX_JORNADA_PET_DATA");
        _collection.SetupGet(c => c.Indexes).Returns(indices.Object);

        // Act
        await _sut.EnsureIndexesAsync();

        // Assert
        capturado.Should().NotBeNull();
        capturado!.Options.Name.Should().Be("IDX_JORNADA_PET_DATA");
    }
}