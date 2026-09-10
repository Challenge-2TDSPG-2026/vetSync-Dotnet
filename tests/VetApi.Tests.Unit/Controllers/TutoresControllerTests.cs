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

public class TutoresControllerTests
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

    private static TutoresController CriarController(AppDbContext context) =>
        new(context, Mapper, NullLogger<TutoresController>.Instance);

    [Fact]
    public async Task GetAll_QuandoExistemTutores_RetornaOkComTodosOsTutores()
    {
        // Arrange
        var context = CriarContexto();
        context.Tutores.Add(new Tutor { Nome = "Maria Souza", Email = "maria@vetsync.example" });
        await context.SaveChangesAsync();
        var controller = CriarController(context);

        // Act
        var resultado = await controller.GetAll();

        // Assert
        var okResult = resultado.Should().BeOfType<OkObjectResult>().Subject;
        var tutores = okResult.Value.Should().BeAssignableTo<IEnumerable<TutorDto>>().Subject;
        tutores.Should().ContainSingle(t => t.Nome == "Maria Souza");
    }

    [Fact]
    public async Task GetById_TutorExistente_RetornaOkComOTutorCorreto()
    {
        // Arrange
        var context = CriarContexto();
        var tutor = new Tutor { Nome = "João Lima", Email = "joao@vetsync.example" };
        context.Tutores.Add(tutor);
        await context.SaveChangesAsync();
        var controller = CriarController(context);

        // Act
        var resultado = await controller.GetById(tutor.Id);

        // Assert
        var okResult = resultado.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeOfType<TutorDto>().Which.Email.Should().Be("joao@vetsync.example");
    }

    [Fact]
    public async Task GetById_TutorInexistente_RetornaNotFound()
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
    public async Task Create_DadosValidos_RetornaCreatedEPersisteOTutorNoBanco()
    {
        // Arrange
        var context = CriarContexto();
        var controller = CriarController(context);
        var dto = new CreateTutorDto { Nome = "Ana Paula", Email = "ana@vetsync.example" };

        // Act
        var resultado = await controller.Create(dto);

        // Assert
        var createdResult = resultado.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.Value.Should().BeOfType<TutorDto>().Which.Email.Should().Be("ana@vetsync.example");
        (await context.Tutores.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task Create_EmailJaCadastrado_RetornaBadRequestSemDuplicarTutor()
    {
        // Arrange
        var context = CriarContexto();
        context.Tutores.Add(new Tutor { Nome = "Ana Paula", Email = "ana@vetsync.example" });
        await context.SaveChangesAsync();
        var controller = CriarController(context);
        var dto = new CreateTutorDto { Nome = "Ana Paula Duplicada", Email = "ANA@vetsync.example" };

        // Act
        var resultado = await controller.Create(dto);

        // Assert
        resultado.Should().BeOfType<BadRequestObjectResult>();
        (await context.Tutores.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task Create_ModelStateInvalido_RetornaBadRequestSemPersistirTutor()
    {
        // Arrange
        var context = CriarContexto();
        var controller = CriarController(context);
        controller.ModelState.AddModelError("Email", "Email é obrigatório");
        var dto = new CreateTutorDto { Nome = "Sem Email" };

        // Act
        var resultado = await controller.Create(dto);

        // Assert
        resultado.Should().BeOfType<BadRequestObjectResult>();
        (await context.Tutores.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task Update_TutorInexistente_RetornaNotFound()
    {
        // Arrange
        var context = CriarContexto();
        var controller = CriarController(context);
        var dto = new UpdateTutorDto { Nome = "Alguém", Email = "alguem@vetsync.example" };

        // Act
        var resultado = await controller.Update(999, dto);

        // Assert
        resultado.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Update_EmailJaUsadoPorOutroTutor_RetornaBadRequest()
    {
        // Arrange
        var context = CriarContexto();
        context.Tutores.Add(new Tutor { Nome = "Tutor Um", Email = "um@vetsync.example" });
        var tutorDois = new Tutor { Nome = "Tutor Dois", Email = "dois@vetsync.example" };
        context.Tutores.Add(tutorDois);
        await context.SaveChangesAsync();
        var controller = CriarController(context);
        var dto = new UpdateTutorDto { Nome = "Tutor Dois", Email = "um@vetsync.example" };

        // Act
        var resultado = await controller.Update(tutorDois.Id, dto);

        // Assert
        resultado.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Delete_TutorExistente_RemoveDoBancoERetornaNoContent()
    {
        // Arrange
        var context = CriarContexto();
        var tutor = new Tutor { Nome = "Removível", Email = "remover@vetsync.example" };
        context.Tutores.Add(tutor);
        await context.SaveChangesAsync();
        var controller = CriarController(context);

        // Act
        var resultado = await controller.Delete(tutor.Id);

        // Assert
        resultado.Should().BeOfType<NoContentResult>();
        (await context.Tutores.FindAsync(tutor.Id)).Should().BeNull();
    }

    [Fact]
    public async Task Delete_TutorInexistente_RetornaNotFound()
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