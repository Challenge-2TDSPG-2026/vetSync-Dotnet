using AutoMapper;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VetApi.Controllers;
using VetApi.Data;
using VetApi.DTOs;
using VetApi.Mappings;
using VetApi.Models;
using Xunit;

namespace VetApi.Tests.Unit.Controllers;

public class VacinacoesControllerTests
{
    private static readonly IMapper Mapper =
        new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>()).CreateMapper();

    private static AppDbContext CriarContexto()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    private static VacinacoesController CriarController(AppDbContext context) =>
        new(context, Mapper);

    private static async Task<Pet> CriarPetAsync(AppDbContext context)
    {
        var tutor = new Tutor { Nome = "Maria Souza", Email = $"{Guid.NewGuid():N}@vetsync.example" };
        context.Tutores.Add(tutor);
        await context.SaveChangesAsync();

        var pet = new Pet { Nome = "Rex", Especie = "Cachorro", TutorId = tutor.Id };
        context.Pets.Add(pet);
        await context.SaveChangesAsync();

        return pet;
    }

    [Fact]
    public async Task GetAll_QuandoExistemVacinacoesCadastradas_RetornaOkComTodasAsVacinacoes()
    {
        // Arrange
        var context = CriarContexto();
        var pet = await CriarPetAsync(context);
        context.Vacinacoes.Add(new Vacinacao { PetId = pet.Id, Vacina = "V10", DataAplicacao = DateTime.UtcNow });
        await context.SaveChangesAsync();
        var controller = CriarController(context);

        // Act
        var resultado = await controller.GetAll();

        // Assert
        var okResult = resultado.Should().BeOfType<OkObjectResult>().Subject;
        var vacinas = okResult.Value.Should().BeAssignableTo<IEnumerable<VacinacaoDto>>().Subject;
        vacinas.Should().ContainSingle(v => v.Vacina == "V10");
    }

    [Fact]
    public async Task GetById_VacinacaoExistente_RetornaOkComAVacinacaoCorreta()
    {
        // Arrange
        var context = CriarContexto();
        var pet = await CriarPetAsync(context);
        var vacina = new Vacinacao { PetId = pet.Id, Vacina = "Antirrábica", DataAplicacao = DateTime.UtcNow };
        context.Vacinacoes.Add(vacina);
        await context.SaveChangesAsync();
        var controller = CriarController(context);

        // Act
        var resultado = await controller.GetById(vacina.Id);

        // Assert
        var okResult = resultado.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeOfType<VacinacaoDto>().Which.Vacina.Should().Be("Antirrábica");
    }

    [Fact]
    public async Task GetById_VacinacaoInexistente_RetornaNotFound()
    {
        // Arrange
        var context = CriarContexto();
        var controller = CriarController(context);

        // Act
        var resultado = await controller.GetById(999);

        // Assert
        resultado.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetByPet_PetInexistente_RetornaNotFound()
    {
        // Arrange
        var context = CriarContexto();
        var controller = CriarController(context);

        // Act
        var resultado = await controller.GetByPet(999);

        // Assert
        resultado.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetByPet_PetExistente_RetornaVacinacoesOrdenadasPorDataDecrescente()
    {
        // Arrange
        var context = CriarContexto();
        var pet = await CriarPetAsync(context);
        context.Vacinacoes.AddRange(
            new Vacinacao { PetId = pet.Id, Vacina = "V10", DataAplicacao = new DateTime(2025, 1, 1) },
            new Vacinacao { PetId = pet.Id, Vacina = "Antirrábica", DataAplicacao = new DateTime(2025, 6, 1) });
        await context.SaveChangesAsync();
        var controller = CriarController(context);

        // Act
        var resultado = await controller.GetByPet(pet.Id);

        // Assert
        var okResult = resultado.Should().BeOfType<OkObjectResult>().Subject;
        var vacinas = okResult.Value.Should().BeAssignableTo<IEnumerable<VacinacaoDto>>().Subject.ToList();
        vacinas.Should().HaveCount(2);
        vacinas.First().Vacina.Should().Be("Antirrábica");
    }

    [Fact]
    public async Task GetProximasDoses_SemParametro_UsaPadraoDe30DiasERetornaVacinacoesDentroDoPrazo()
    {
        // Arrange
        var context = CriarContexto();
        var pet = await CriarPetAsync(context);
        context.Vacinacoes.AddRange(
            new Vacinacao { PetId = pet.Id, Vacina = "V10", DataAplicacao = DateTime.UtcNow, ProximaDose = DateTime.UtcNow.AddDays(10) },
            new Vacinacao { PetId = pet.Id, Vacina = "Antirrábica", DataAplicacao = DateTime.UtcNow, ProximaDose = DateTime.UtcNow.AddDays(90) });
        await context.SaveChangesAsync();
        var controller = CriarController(context);

        // Act
        var resultado = await controller.GetProximasDoses(null);

        // Assert
        var okResult = resultado.Should().BeOfType<OkObjectResult>().Subject;
        var vacinas = okResult.Value.Should().BeAssignableTo<IEnumerable<VacinacaoDto>>().Subject;
        vacinas.Should().ContainSingle(v => v.Vacina == "V10");
    }

    [Fact]
    public async Task GetProximasDoses_ComDataInformada_RetornaSomenteVacinacoesAteADataLimite()
    {
        // Arrange
        var context = CriarContexto();
        var pet = await CriarPetAsync(context);
        context.Vacinacoes.AddRange(
            new Vacinacao { PetId = pet.Id, Vacina = "V10", DataAplicacao = DateTime.UtcNow, ProximaDose = new DateTime(2025, 6, 1) },
            new Vacinacao { PetId = pet.Id, Vacina = "Antirrábica", DataAplicacao = DateTime.UtcNow, ProximaDose = new DateTime(2026, 1, 1) });
        await context.SaveChangesAsync();
        var controller = CriarController(context);

        // Act
        var resultado = await controller.GetProximasDoses(new DateTime(2025, 12, 31));

        // Assert
        var okResult = resultado.Should().BeOfType<OkObjectResult>().Subject;
        var vacinas = okResult.Value.Should().BeAssignableTo<IEnumerable<VacinacaoDto>>().Subject;
        vacinas.Should().ContainSingle(v => v.Vacina == "V10");
    }

    [Fact]
    public async Task Create_PetIdInexistente_RetornaNotFoundSemPersistirVacinacao()
    {
        // Arrange
        var context = CriarContexto();
        var controller = CriarController(context);
        var dto = new CreateVacinacaoDto { PetId = 999, Vacina = "V10", DataAplicacao = DateTime.UtcNow };

        // Act
        var resultado = await controller.Create(dto);

        // Assert
        resultado.Should().BeOfType<NotFoundObjectResult>();
        (await context.Vacinacoes.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task Create_DadosValidos_RetornaCreatedEPersisteAVacinacaoNoBanco()
    {
        // Arrange
        var context = CriarContexto();
        var pet = await CriarPetAsync(context);
        var controller = CriarController(context);
        var dto = new CreateVacinacaoDto { PetId = pet.Id, Vacina = "V10", Fabricante = "Zoetis", DataAplicacao = DateTime.UtcNow };

        // Act
        var resultado = await controller.Create(dto);

        // Assert
        var createdResult = resultado.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.ActionName.Should().Be(nameof(VacinacoesController.GetById));
        createdResult.Value.Should().BeOfType<VacinacaoDto>().Which.Vacina.Should().Be("V10");
        (await context.Vacinacoes.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task Create_ModelStateInvalido_RetornaBadRequestSemPersistirVacinacao()
    {
        // Arrange
        var context = CriarContexto();
        var controller = CriarController(context);
        controller.ModelState.AddModelError("Vacina", "Vacina é obrigatória");
        var dto = new CreateVacinacaoDto { PetId = 1, DataAplicacao = DateTime.UtcNow };

        // Act
        var resultado = await controller.Create(dto);

        // Assert
        resultado.Should().BeOfType<BadRequestObjectResult>();
        (await context.Vacinacoes.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task Delete_VacinacaoExistente_RemoveDoBancoERetornaNoContent()
    {
        // Arrange
        var context = CriarContexto();
        var pet = await CriarPetAsync(context);
        var vacina = new Vacinacao { PetId = pet.Id, Vacina = "V10", DataAplicacao = DateTime.UtcNow };
        context.Vacinacoes.Add(vacina);
        await context.SaveChangesAsync();
        var controller = CriarController(context);

        // Act
        var resultado = await controller.Delete(vacina.Id);

        // Assert
        resultado.Should().BeOfType<NoContentResult>();
        (await context.Vacinacoes.FindAsync(vacina.Id)).Should().BeNull();
    }

    [Fact]
    public async Task Delete_VacinacaoInexistente_RetornaNotFound()
    {
        // Arrange
        var context = CriarContexto();
        var controller = CriarController(context);

        // Act
        var resultado = await controller.Delete(999);

        // Assert
        resultado.Should().BeOfType<NotFoundObjectResult>();
    }
}
