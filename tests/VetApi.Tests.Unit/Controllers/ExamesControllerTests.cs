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

public class ExamesControllerTests
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

    private static ExamesController CriarController(AppDbContext context) =>
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
    public async Task GetAll_QuandoExistemExamesCadastrados_RetornaOkComTodosOsExames()
    {
        // Arrange
        var context = CriarContexto();
        var pet = await CriarPetAsync(context);
        context.Exames.Add(new Exame { PetId = pet.Id, TipoExame = "Hemograma completo", DataExame = DateTime.UtcNow });
        await context.SaveChangesAsync();
        var controller = CriarController(context);

        // Act
        var resultado = await controller.GetAll();

        // Assert
        var okResult = resultado.Should().BeOfType<OkObjectResult>().Subject;
        var exames = okResult.Value.Should().BeAssignableTo<IEnumerable<ExameDto>>().Subject;
        exames.Should().ContainSingle(e => e.TipoExame == "Hemograma completo");
    }

    [Fact]
    public async Task GetById_ExameExistente_RetornaOkComOExameCorreto()
    {
        // Arrange
        var context = CriarContexto();
        var pet = await CriarPetAsync(context);
        var exame = new Exame { PetId = pet.Id, TipoExame = "Raio-X", DataExame = DateTime.UtcNow };
        context.Exames.Add(exame);
        await context.SaveChangesAsync();
        var controller = CriarController(context);

        // Act
        var resultado = await controller.GetById(exame.Id);

        // Assert
        var okResult = resultado.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeOfType<ExameDto>().Which.TipoExame.Should().Be("Raio-X");
    }

    [Fact]
    public async Task GetById_ExameInexistente_RetornaNotFound()
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
    public async Task GetByPet_PetExistente_RetornaExamesOrdenadosPorDataDecrescente()
    {
        // Arrange
        var context = CriarContexto();
        var pet = await CriarPetAsync(context);
        context.Exames.AddRange(
            new Exame { PetId = pet.Id, TipoExame = "Hemograma completo", DataExame = new DateTime(2025, 1, 1) },
            new Exame { PetId = pet.Id, TipoExame = "Raio-X", DataExame = new DateTime(2025, 6, 1) });
        await context.SaveChangesAsync();
        var controller = CriarController(context);

        // Act
        var resultado = await controller.GetByPet(pet.Id);

        // Assert
        var okResult = resultado.Should().BeOfType<OkObjectResult>().Subject;
        var exames = okResult.Value.Should().BeAssignableTo<IEnumerable<ExameDto>>().Subject.ToList();
        exames.Should().HaveCount(2);
        exames.First().TipoExame.Should().Be("Raio-X");
    }

    [Fact]
    public async Task GetByTipo_ComFiltroParcialEEmCaixaDiferente_RetornaSomenteExamesCorrespondentes()
    {
        // Arrange
        var context = CriarContexto();
        var pet = await CriarPetAsync(context);
        context.Exames.AddRange(
            new Exame { PetId = pet.Id, TipoExame = "Hemograma completo", DataExame = DateTime.UtcNow },
            new Exame { PetId = pet.Id, TipoExame = "Raio-X torácico", DataExame = DateTime.UtcNow });
        await context.SaveChangesAsync();
        var controller = CriarController(context);

        // Act
        var resultado = await controller.GetByTipo("RAIO-X");

        // Assert
        var okResult = resultado.Should().BeOfType<OkObjectResult>().Subject;
        var exames = okResult.Value.Should().BeAssignableTo<IEnumerable<ExameDto>>().Subject;
        exames.Should().ContainSingle(e => e.TipoExame == "Raio-X torácico");
    }

    [Fact]
    public async Task Create_PetIdInexistente_RetornaNotFoundSemPersistirExame()
    {
        // Arrange
        var context = CriarContexto();
        var controller = CriarController(context);
        var dto = new CreateExameDto { PetId = 999, TipoExame = "Hemograma completo", DataExame = DateTime.UtcNow };

        // Act
        var resultado = await controller.Create(dto);

        // Assert
        resultado.Should().BeOfType<NotFoundObjectResult>();
        (await context.Exames.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task Create_DadosValidos_RetornaCreatedEPersisteOExameNoBanco()
    {
        // Arrange
        var context = CriarContexto();
        var pet = await CriarPetAsync(context);
        var controller = CriarController(context);
        var dto = new CreateExameDto { PetId = pet.Id, TipoExame = "Hemograma completo", DataExame = DateTime.UtcNow, Laboratorio = "LabVet SP" };

        // Act
        var resultado = await controller.Create(dto);

        // Assert
        var createdResult = resultado.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.ActionName.Should().Be(nameof(ExamesController.GetById));
        createdResult.Value.Should().BeOfType<ExameDto>().Which.TipoExame.Should().Be("Hemograma completo");
        (await context.Exames.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task Create_ModelStateInvalido_RetornaBadRequestSemPersistirExame()
    {
        // Arrange
        var context = CriarContexto();
        var controller = CriarController(context);
        controller.ModelState.AddModelError("TipoExame", "Tipo de exame é obrigatório");
        var dto = new CreateExameDto { PetId = 1, DataExame = DateTime.UtcNow };

        // Act
        var resultado = await controller.Create(dto);

        // Assert
        resultado.Should().BeOfType<BadRequestObjectResult>();
        (await context.Exames.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task Update_ExameInexistente_RetornaNotFound()
    {
        // Arrange
        var context = CriarContexto();
        var controller = CriarController(context);
        var dto = new UpdateExameDto { TipoExame = "Hemograma completo", DataExame = DateTime.UtcNow };

        // Act
        var resultado = await controller.Update(999, dto);

        // Assert
        resultado.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Update_DadosValidos_AtualizaERetornaNoContent()
    {
        // Arrange
        var context = CriarContexto();
        var pet = await CriarPetAsync(context);
        var exame = new Exame { PetId = pet.Id, TipoExame = "Hemograma completo", DataExame = DateTime.UtcNow };
        context.Exames.Add(exame);
        await context.SaveChangesAsync();
        var controller = CriarController(context);
        var dto = new UpdateExameDto { TipoExame = "Hemograma completo", DataExame = DateTime.UtcNow, Resultado = "Todos os índices dentro do esperado" };

        // Act
        var resultado = await controller.Update(exame.Id, dto);

        // Assert
        resultado.Should().BeOfType<NoContentResult>();
        var atualizado = await context.Exames.FindAsync(exame.Id);
        atualizado!.Resultado.Should().Be("Todos os índices dentro do esperado");
    }

    [Fact]
    public async Task Delete_ExameExistente_RemoveDoBancoERetornaNoContent()
    {
        // Arrange
        var context = CriarContexto();
        var pet = await CriarPetAsync(context);
        var exame = new Exame { PetId = pet.Id, TipoExame = "Hemograma completo", DataExame = DateTime.UtcNow };
        context.Exames.Add(exame);
        await context.SaveChangesAsync();
        var controller = CriarController(context);

        // Act
        var resultado = await controller.Delete(exame.Id);

        // Assert
        resultado.Should().BeOfType<NoContentResult>();
        (await context.Exames.FindAsync(exame.Id)).Should().BeNull();
    }

    [Fact]
    public async Task Delete_ExameInexistente_RetornaNotFound()
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
