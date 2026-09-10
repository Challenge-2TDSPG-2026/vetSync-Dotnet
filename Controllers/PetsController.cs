using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VetApi.Data;
using VetApi.DTOs;
using VetApi.Models;

namespace VetApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Tags("Pets")]
public class PetsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<PetsController> _logger;

    public PetsController(AppDbContext context, IMapper mapper, ILogger<PetsController> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>Lista todos os pets</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<PetDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var pets = await _context.Pets.Include(p => p.Tutor).ToListAsync();
        return Ok(_mapper.Map<IEnumerable<PetDto>>(pets));
    }

    /// <summary>Busca pet por ID</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(PetDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var pet = await _context.Pets.Include(p => p.Tutor).FirstOrDefaultAsync(p => p.Id == id);
        if (pet is null)
            return NotFound(new { message = $"Pet com ID {id} não encontrado." });

        return Ok(_mapper.Map<PetDto>(pet));
    }

    /// <summary>Filtra pets por espécie</summary>
    [HttpGet("especie/{especie}")]
    [ProducesResponseType(typeof(IEnumerable<PetDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByEspecie(string especie)
    {
        var pets = await _context.Pets
            .Include(p => p.Tutor)
            .Where(p => p.Especie.ToLower() == especie.ToLower())
            .ToListAsync();

        return Ok(_mapper.Map<IEnumerable<PetDto>>(pets));
    }

    /// <summary>Lista pets ativos</summary>
    [HttpGet("ativos")]
    [ProducesResponseType(typeof(IEnumerable<PetDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAtivos()
    {
        var pets = await _context.Pets.Include(p => p.Tutor).Where(p => p.Ativo).ToListAsync();
        return Ok(_mapper.Map<IEnumerable<PetDto>>(pets));
    }

    /// <summary>Busca avançada de pets por nome, espécie, raça ou status</summary>
    [HttpGet("buscar")]
    [ProducesResponseType(typeof(IEnumerable<PetDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Buscar([FromQuery] string? nome, [FromQuery] string? especie, [FromQuery] string? raca, [FromQuery] bool? ativo)
    {
        var query = _context.Pets.Include(p => p.Tutor).AsQueryable();

        if (!string.IsNullOrEmpty(nome))
            query = query.Where(p => p.Nome.ToLower().Contains(nome.ToLower()));

        if (!string.IsNullOrEmpty(especie))
            query = query.Where(p => p.Especie.ToLower() == especie.ToLower());

        if (!string.IsNullOrEmpty(raca))
            query = query.Where(p => p.Raca != null && p.Raca.ToLower().Contains(raca.ToLower()));

        if (ativo.HasValue)
            query = query.Where(p => p.Ativo == ativo.Value);

        return Ok(_mapper.Map<IEnumerable<PetDto>>(await query.ToListAsync()));
    }

    /// <summary>Retorna a jornada completa do pet (consultas, vacinas, exames)</summary>
    [HttpGet("{id:int}/jornada")]
    [ProducesResponseType(typeof(JornadaPetDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetJornada(int id)
    {
        var pet = await _context.Pets
            .Include(p => p.Tutor)
            .Include(p => p.Consultas)
            .Include(p => p.Vacinacoes)
            .Include(p => p.Exames)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (pet is null)
            return NotFound(new { message = $"Pet com ID {id} não encontrado." });

        var jornada = new JornadaPetDto
        {
            Pet = _mapper.Map<PetDto>(pet),
            Tutor = _mapper.Map<TutorDto>(pet.Tutor),
            Consultas = _mapper.Map<List<ConsultaDto>>(pet.Consultas.OrderByDescending(c => c.DataConsulta)),
            Vacinacoes = _mapper.Map<List<VacinacaoDto>>(pet.Vacinacoes.OrderByDescending(v => v.DataAplicacao)),
            Exames = _mapper.Map<List<ExameDto>>(pet.Exames.OrderByDescending(e => e.DataExame))
        };

        return Ok(jornada);
    }

    /// <summary>Cadastra um novo pet</summary>
    [HttpPost]
    [ProducesResponseType(typeof(PetDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create([FromBody] CreatePetDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var tutorExiste = await _context.Tutores.AnyAsync(t => t.Id == dto.TutorId);
        if (!tutorExiste)
        {
            _logger.LogWarning("Tentativa de cadastrar pet para TutorId {TutorId} inexistente.", dto.TutorId);
            return NotFound(new { message = "Tutor não encontrado." });
        }

        var pet = _mapper.Map<Pet>(dto);
        _context.Pets.Add(pet);
        await _context.SaveChangesAsync();

        var created = await _context.Pets.Include(p => p.Tutor).FirstAsync(p => p.Id == pet.Id);

        _logger.LogInformation("Pet {PetId} ({PetNome}) cadastrado para o Tutor {TutorId}.", pet.Id, pet.Nome, pet.TutorId);

        return CreatedAtAction(nameof(GetById), new { id = pet.Id }, _mapper.Map<PetDto>(created));
    }

    /// <summary>Atualiza dados de um pet</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdatePetDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var pet = await _context.Pets.FindAsync(id);
        if (pet is null)
        {
            _logger.LogWarning("Tentativa de atualizar Pet {PetId} inexistente.", id);
            return NotFound(new { message = $"Pet com ID {id} não encontrado." });
        }

        _mapper.Map(dto, pet);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Pet {PetId} atualizado.", id);

        return NoContent();
    }

    /// <summary>Remove um pet</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var pet = await _context.Pets.FindAsync(id);
        if (pet is null)
        {
            _logger.LogWarning("Tentativa de remover Pet {PetId} inexistente.", id);
            return NotFound(new { message = $"Pet com ID {id} não encontrado." });
        }

        _context.Pets.Remove(pet);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Pet {PetId} removido.", id);

        return NoContent();
    }
}