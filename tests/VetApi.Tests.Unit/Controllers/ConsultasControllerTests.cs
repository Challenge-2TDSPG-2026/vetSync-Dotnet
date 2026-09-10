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

public class ConsultasControllerTests
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

    private static ConsultasController CriarController(AppDbContext context) =>
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
    public async Task GetAll_QuandoExistemConsultasCadastradas_RetornaOkComTodasAsConsultas()
    {
        // Arrange
        var context = CriarContexto();
        var pet = await CriarPetAsync(context);
        context.Consultas.Add(new Consulta { PetId = pet.Id, Veterinario = "Dr. João Silva", DataConsulta = DateTime.UtcNow, Status = "Agendada" });
        await context.SaveChangesAsync();
        var controller = CriarController(context);

        // Act
        var resultado = await controller.GetAll();

        // Assert
        var okResult = resultado.Should().BeOfType<OkObjectResult>().Subject;
        var consultas = okResult.Value.Should().BeAssignableTo<IEnumerable<ConsultaDto>>().Subject;
        consultas.Should().ContainSingle(c => c.Veterinario == "Dr. João Silva");
    }

    [Fact]
    public async Task GetById_ConsultaExistente_RetornaOkComAConsultaCorreta()
    {
        // Arrange
        var context = CriarContexto();
        var pet = await CriarPetAsync(context);
        var consulta = new Consulta { PetId = pet.Id, Veterinario = "Dra. Ana Costa", DataConsulta = DateTime.UtcNow, Status = "Agendada" };
        context.Consultas.Add(consulta);
        await context.SaveChangesAsync();
        var controller = CriarController(context);

        // Act
        var resultado = await controller.GetById(consulta.Id);

        // Assert
        var okResult = resultado.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeOfType<ConsultaDto>().Which.Veterinario.Should().Be("Dra. Ana Costa");
    }

    [Fact]
    public async Task GetById_ConsultaInexistente_RetornaNotFound()
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
    public async Task GetByStatus_ComFiltroEmCaixaDiferente_RetornaSomenteConsultasDoStatus()
    {
        // Arrange
        var context = CriarContexto();
        var pet = await CriarPetAsync(context);
        context.Consultas.AddRange(
            new Consulta { PetId = pet.Id, Veterinario = "Dr. João Silva", DataConsulta = DateTime.UtcNow, Status = "Agendada" },
            new Consulta { PetId = pet.Id, Veterinario = "Dr. João Silva", DataConsulta = DateTime.UtcNow, Status = "Realizada" });
        await context.SaveChangesAsync();
        var controller = CriarController(context);

        // Act
        var resultado = await controller.GetByStatus("REALIZADA");

        // Assert
        var okResult = resultado.Should().BeOfType<OkObjectResult>().Subject;
        var consultas = okResult.Value.Should().BeAssignableTo<IEnumerable<ConsultaDto>>().Subject;
        consultas.Should().ContainSingle(c => c.Status == "Realizada");
    }

    [Fact]
    public async Task GetByPeriodo_DataInicialMaiorQueFinal_RetornaBadRequest()
    {
        // Arrange
        var context = CriarContexto();
        var controller = CriarController(context);

        // Act
        var resultado = await controller.GetByPeriodo(DateTime.UtcNow, DateTime.UtcNow.AddDays(-5));

        // Assert
        resultado.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task GetByPeriodo_ComPeriodoValido_RetornaConsultasDentroDoIntervalo()
    {
        // Arrange
        var context = CriarContexto();
        var pet = await CriarPetAsync(context);
        var dataDentro = new DateTime(2025, 6, 10);
        var dataFora = new DateTime(2025, 1, 1);
        context.Consultas.AddRange(
            new Consulta { PetId = pet.Id, Veterinario = "Dr. João Silva", DataConsulta = dataDentro, Status = "Agendada" },
            new Consulta { PetId = pet.Id, Veterinario = "Dr. João Silva", DataConsulta = dataFora, Status = "Agendada" });
        await context.SaveChangesAsync();
        var controller = CriarController(context);

        // Act
        var resultado = await controller.GetByPeriodo(new DateTime(2025, 6, 1), new DateTime(2025, 6, 30));

        // Assert
        var okResult = resultado.Should().BeOfType<OkObjectResult>().Subject;
        var consultas = okResult.Value.Should().BeAssignableTo<IEnumerable<ConsultaDto>>().Subject;
        consultas.Should().ContainSingle(c => c.DataConsulta == dataDentro);
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
    public async Task GetByPet_PetExistente_RetornaConsultasOrdenadasPorDataDecrescente()
    {
        // Arrange
        var context = CriarContexto();
        var pet = await CriarPetAsync(context);
        context.Consultas.AddRange(
            new Consulta { PetId = pet.Id, Veterinario = "Dr. João Silva", DataConsulta = new DateTime(2025, 1, 1), Status = "Realizada" },
            new Consulta { PetId = pet.Id, Veterinario = "Dr. João Silva", DataConsulta = new DateTime(2025, 6, 1), Status = "Agendada" });
        await context.SaveChangesAsync();
        var controller = CriarController(context);

        // Act
        var resultado = await controller.GetByPet(pet.Id);

        // Assert
        var okResult = resultado.Should().BeOfType<OkObjectResult>().Subject;
        var consultas = okResult.Value.Should().BeAssignableTo<IEnumerable<ConsultaDto>>().Subject.ToList();
        consultas.Should().HaveCount(2);
        consultas.First().DataConsulta.Should().Be(new DateTime(2025, 6, 1));
    }

    [Fact]
    public async Task Create_PetIdInexistente_RetornaNotFoundSemPersistirConsulta()
    {
        // Arrange
        var context = CriarContexto();
        var controller = CriarController(context);
        var dto = new CreateConsultaDto { PetId = 999, Veterinario = "Dr. João Silva", DataConsulta = DateTime.UtcNow };

        // Act
        var resultado = await controller.Create(dto);

        // Assert
        resultado.Should().BeOfType<NotFoundObjectResult>();
        (await context.Consultas.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task Create_DadosValidos_RetornaCreatedEPersisteAConsultaNoBanco()
    {
        // Arrange
        var context = CriarContexto();
        var pet = await CriarPetAsync(context);
        var controller = CriarController(context);
        var dto = new CreateConsultaDto { PetId = pet.Id, Veterinario = "Dra. Ana Costa", DataConsulta = DateTime.UtcNow, Motivo = "Check-up anual" };

        // Act
        var resultado = await controller.Create(dto);

        // Assert
        var createdResult = resultado.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.ActionName.Should().Be(nameof(ConsultasController.GetById));
        createdResult.Value.Should().BeOfType<ConsultaDto>().Which.Veterinario.Should().Be("Dra. Ana Costa");
        (await context.Consultas.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task Create_ModelStateInvalido_RetornaBadRequestSemPersistirConsulta()
    {
        // Arrange
        var context = CriarContexto();
        var controller = CriarController(context);
        controller.ModelState.AddModelError("Veterinario", "Veterinário é obrigatório");
        var dto = new CreateConsultaDto { PetId = 1, DataConsulta = DateTime.UtcNow };

        // Act
        var resultado = await controller.Create(dto);

        // Assert
        resultado.Should().BeOfType<BadRequestObjectResult>();
        (await context.Consultas.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task Update_ConsultaInexistente_RetornaNotFound()
    {
        // Arrange
        var context = CriarContexto();
        var controller = CriarController(context);
        var dto = new UpdateConsultaDto { Veterinario = "Dr. João Silva", DataConsulta = DateTime.UtcNow, Status = "Realizada" };

        // Act
        var resultado = await controller.Update(999, dto);

        // Assert
        resultado.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Update_StatusInvalido_RetornaBadRequest()
    {
        // Arrange
        var context = CriarContexto();
        var pet = await CriarPetAsync(context);
        var consulta = new Consulta { PetId = pet.Id, Veterinario = "Dr. João Silva", DataConsulta = DateTime.UtcNow, Status = "Agendada" };
        context.Consultas.Add(consulta);
        await context.SaveChangesAsync();
        var controller = CriarController(context);
        var dto = new UpdateConsultaDto { Veterinario = "Dr. João Silva", DataConsulta = DateTime.UtcNow, Status = "StatusInvalido" };

        // Act
        var resultado = await controller.Update(consulta.Id, dto);

        // Assert
        resultado.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Update_DadosValidos_AtualizaERetornaNoContent()
    {
        // Arrange
        var context = CriarContexto();
        var pet = await CriarPetAsync(context);
        var consulta = new Consulta { PetId = pet.Id, Veterinario = "Dr. João Silva", DataConsulta = DateTime.UtcNow, Status = "Agendada" };
        context.Consultas.Add(consulta);
        await context.SaveChangesAsync();
        var controller = CriarController(context);
        var dto = new UpdateConsultaDto { Veterinario = "Dr. João Silva", DataConsulta = DateTime.UtcNow, Diagnostico = "Animal saudável", Status = "Realizada" };

        // Act
        var resultado = await controller.Update(consulta.Id, dto);

        // Assert
        resultado.Should().BeOfType<NoContentResult>();
        var atualizada = await context.Consultas.FindAsync(consulta.Id);
        atualizada!.Status.Should().Be("Realizada");
        atualizada.Diagnostico.Should().Be("Animal saudável");
    }

    [Fact]
    public async Task Delete_ConsultaExistente_RemoveDoBancoERetornaNoContent()
    {
        // Arrange
        var context = CriarContexto();
        var pet = await CriarPetAsync(context);
        var consulta = new Consulta { PetId = pet.Id, Veterinario = "Dr. João Silva", DataConsulta = DateTime.UtcNow, Status = "Agendada" };
        context.Consultas.Add(consulta);
        await context.SaveChangesAsync();
        var controller = CriarController(context);

        // Act
        var resultado = await controller.Delete(consulta.Id);

        // Assert
        resultado.Should().BeOfType<NoContentResult>();
        (await context.Consultas.FindAsync(consulta.Id)).Should().BeNull();
    }

    [Fact]
    public async Task Delete_ConsultaInexistente_RetornaNotFound()
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
