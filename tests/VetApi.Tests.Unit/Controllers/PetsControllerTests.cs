using AutoMapper;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using VetApi.Controllers;
using VetApi.Data;
using VetApi.DTOs;
using VetApi.Mappings;
using VetApi.Models;
using Xunit;

namespace VetApi.Tests.Unit.Controllers;

public class PetsControllerTests
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

    private static PetsController CriarController(AppDbContext context) =>
        new(context, Mapper, NullLogger<PetsController>.Instance);

    private static async Task<Tutor> CriarTutorAsync(AppDbContext context)
    {
        var tutor = new Tutor { Nome = "Maria Souza", Email = $"{Guid.NewGuid():N}@vetsync.example" };
        context.Tutores.Add(tutor);
        await context.SaveChangesAsync();
        return tutor;
    }

    [Fact]
    public async Task GetAll_QuandoExistemPetsCadastrados_RetornaOkComTodosOsPets()
    {
        // Arrange
        var context = CriarContexto();
        var tutor = await CriarTutorAsync(context);
        context.Pets.Add(new Pet { Nome = "Rex", Especie = "Cachorro", TutorId = tutor.Id });
        await context.SaveChangesAsync();
        var controller = CriarController(context);

        // Act
        var resultado = await controller.GetAll();

        // Assert
        var okResult = resultado.Should().BeOfType<OkObjectResult>().Subject;
        var pets = okResult.Value.Should().BeAssignableTo<IEnumerable<PetDto>>().Subject;
        pets.Should().ContainSingle(p => p.Nome == "Rex");
    }

    [Fact]
    public async Task GetById_PetExistente_RetornaOkComOPetCorreto()
    {
        // Arrange
        var context = CriarContexto();
        var tutor = await CriarTutorAsync(context);
        var pet = new Pet { Nome = "Bidu", Especie = "Cachorro", TutorId = tutor.Id };
        context.Pets.Add(pet);
        await context.SaveChangesAsync();
        var controller = CriarController(context);

        // Act
        var resultado = await controller.GetById(pet.Id);

        // Assert
        var okResult = resultado.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeOfType<PetDto>().Which.Nome.Should().Be("Bidu");
    }

    [Fact]
    public async Task GetById_PetInexistente_RetornaNotFound()
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
    public async Task GetByEspecie_ComFiltroEmCaixaDiferente_RetornaSomentePetsDaEspecie()
    {
        // Arrange
        var context = CriarContexto();
        var tutor = await CriarTutorAsync(context);
        context.Pets.AddRange(
            new Pet { Nome = "Rex", Especie = "Cachorro", TutorId = tutor.Id },
            new Pet { Nome = "Miau", Especie = "Gato", TutorId = tutor.Id });
        await context.SaveChangesAsync();
        var controller = CriarController(context);

        // Act
        var resultado = await controller.GetByEspecie("GATO");

        // Assert
        var okResult = resultado.Should().BeOfType<OkObjectResult>().Subject;
        var pets = okResult.Value.Should().BeAssignableTo<IEnumerable<PetDto>>().Subject;
        pets.Should().ContainSingle(p => p.Nome == "Miau");
    }

    [Fact]
    public async Task Create_TutorIdInexistente_RetornaNotFoundSemPersistirPet()
    {
        // Arrange
        var context = CriarContexto();
        var controller = CriarController(context);
        var dto = new CreatePetDto { Nome = "Thor", Especie = "Cachorro", TutorId = 999 };

        // Act
        var resultado = await controller.Create(dto);

        // Assert
        resultado.Should().BeOfType<NotFoundObjectResult>();
        (await context.Pets.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task Create_DadosValidos_RetornaCreatedEPersisteOPetNoBanco()
    {
        // Arrange
        var context = CriarContexto();
        var tutor = await CriarTutorAsync(context);
        var controller = CriarController(context);
        var dto = new CreatePetDto { Nome = "Luna", Especie = "Gato", TutorId = tutor.Id };

        // Act
        var resultado = await controller.Create(dto);

        // Assert
        var createdResult = resultado.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.ActionName.Should().Be(nameof(PetsController.GetById));
        createdResult.Value.Should().BeOfType<PetDto>().Which.Nome.Should().Be("Luna");
        (await context.Pets.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task Create_ModelStateInvalido_RetornaBadRequestSemPersistirPet()
    {
        // Arrange
        var context = CriarContexto();
        var controller = CriarController(context);
        controller.ModelState.AddModelError("Nome", "Nome é obrigatório");
        var dto = new CreatePetDto { Especie = "Cachorro", TutorId = 1 };

        // Act
        var resultado = await controller.Create(dto);

        // Assert
        resultado.Should().BeOfType<BadRequestObjectResult>();
        (await context.Pets.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task Update_PetInexistente_RetornaNotFound()
    {
        // Arrange
        var context = CriarContexto();
        var controller = CriarController(context);
        var dto = new UpdatePetDto { Nome = "Rex", Especie = "Cachorro" };

        // Act
        var resultado = await controller.Update(999, dto);

        // Assert
        resultado.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Update_PetExistente_AtualizaOsDadosERetornaNoContent()
    {
        // Arrange
        var context = CriarContexto();
        var tutor = await CriarTutorAsync(context);
        var pet = new Pet { Nome = "Rex", Especie = "Cachorro", TutorId = tutor.Id };
        context.Pets.Add(pet);
        await context.SaveChangesAsync();
        var controller = CriarController(context);
        var dto = new UpdatePetDto { Nome = "Rex Atualizado", Especie = "Cachorro", Ativo = true };

        // Act
        var resultado = await controller.Update(pet.Id, dto);

        // Assert
        resultado.Should().BeOfType<NoContentResult>();
        var atualizado = await context.Pets.FindAsync(pet.Id);
        atualizado!.Nome.Should().Be("Rex Atualizado");
    }

    [Fact]
    public async Task Delete_PetExistente_RemoveDoBancoERetornaNoContent()
    {
        // Arrange
        var context = CriarContexto();
        var tutor = await CriarTutorAsync(context);
        var pet = new Pet { Nome = "Rex", Especie = "Cachorro", TutorId = tutor.Id };
        context.Pets.Add(pet);
        await context.SaveChangesAsync();
        var controller = CriarController(context);

        // Act
        var resultado = await controller.Delete(pet.Id);

        // Assert
        resultado.Should().BeOfType<NoContentResult>();
        (await context.Pets.FindAsync(pet.Id)).Should().BeNull();
    }

    [Fact]
    public async Task Delete_PetInexistente_RetornaNotFound()
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