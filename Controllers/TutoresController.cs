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
[Tags("Tutores")]
public class TutoresController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<TutoresController> _logger;

    public TutoresController(AppDbContext context, IMapper mapper, ILogger<TutoresController> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>Lista todos os tutores</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<TutorDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var tutores = await _context.Tutores.ToListAsync();
        return Ok(_mapper.Map<IEnumerable<TutorDto>>(tutores));
    }

    /// <summary>Busca tutor por ID</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(TutorDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var tutor = await _context.Tutores.FindAsync(id);
        if (tutor is null)
            return NotFound(new { message = $"Tutor com ID {id} não encontrado." });

        return Ok(_mapper.Map<TutorDto>(tutor));
    }

    /// <summary>Busca tutor por email</summary>
    [HttpGet("email/{email}")]
    [ProducesResponseType(typeof(TutorDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByEmail(string email)
    {
        var tutor = await _context.Tutores
            .FirstOrDefaultAsync(t => t.Email.ToLower() == email.ToLower());

        if (tutor is null)
            return NotFound(new { message = "Tutor não encontrado." });

        return Ok(_mapper.Map<TutorDto>(tutor));
    }

    /// <summary>Lista tutores ativos</summary>
    [HttpGet("ativos")]
    [ProducesResponseType(typeof(IEnumerable<TutorDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAtivos()
    {
        var tutores = await _context.Tutores.Where(t => t.Ativo).ToListAsync();
        return Ok(_mapper.Map<IEnumerable<TutorDto>>(tutores));
    }

    /// <summary>Busca tutores por nome</summary>
    [HttpGet("buscar")]
    [ProducesResponseType(typeof(IEnumerable<TutorDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Buscar([FromQuery] string? nome, [FromQuery] bool? ativo)
    {
        var query = _context.Tutores.AsQueryable();

        if (!string.IsNullOrEmpty(nome))
            query = query.Where(t => t.Nome.ToLower().Contains(nome.ToLower()));

        if (ativo.HasValue)
            query = query.Where(t => t.Ativo == ativo.Value);

        return Ok(_mapper.Map<IEnumerable<TutorDto>>(await query.ToListAsync()));
    }

    /// <summary>Lista todos os pets do tutor</summary>
    [HttpGet("{id:int}/pets")]
    [ProducesResponseType(typeof(IEnumerable<PetDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPets(int id)
    {
        var existe = await _context.Tutores.AnyAsync(t => t.Id == id);
        if (!existe)
            return NotFound(new { message = $"Tutor com ID {id} não encontrado." });

        var pets = await _context.Pets
            .Include(p => p.Tutor)
            .Where(p => p.TutorId == id)
            .ToListAsync();

        return Ok(_mapper.Map<IEnumerable<PetDto>>(pets));
    }

    /// <summary>Cadastra um novo tutor</summary>
    [HttpPost]
    [ProducesResponseType(typeof(TutorDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateTutorDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var emailExiste = await _context.Tutores.AnyAsync(t => t.Email.ToLower() == dto.Email.ToLower());
        if (emailExiste)
        {
            _logger.LogWarning("Tentativa de cadastrar tutor com email já existente: {Email}.", dto.Email);
            return BadRequest(new { message = "Já existe um tutor com este email." });
        }

        var tutor = _mapper.Map<Tutor>(dto);
        _context.Tutores.Add(tutor);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Tutor {TutorId} ({TutorNome}) cadastrado.", tutor.Id, tutor.Nome);

        return CreatedAtAction(nameof(GetById), new { id = tutor.Id }, _mapper.Map<TutorDto>(tutor));
    }

    /// <summary>Atualiza dados de um tutor</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateTutorDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var tutor = await _context.Tutores.FindAsync(id);
        if (tutor is null)
        {
            _logger.LogWarning("Tentativa de atualizar Tutor {TutorId} inexistente.", id);
            return NotFound(new { message = $"Tutor com ID {id} não encontrado." });
        }

        var emailExiste = await _context.Tutores.AnyAsync(t => t.Email.ToLower() == dto.Email.ToLower() && t.Id != id);
        if (emailExiste)
            return BadRequest(new { message = "Email já em uso por outro tutor." });

        _mapper.Map(dto, tutor);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Tutor {TutorId} atualizado.", id);

        return NoContent();
    }

    /// <summary>Remove um tutor</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var tutor = await _context.Tutores.FindAsync(id);
        if (tutor is null)
        {
            _logger.LogWarning("Tentativa de remover Tutor {TutorId} inexistente.", id);
            return NotFound(new { message = $"Tutor com ID {id} não encontrado." });
        }

        _context.Tutores.Remove(tutor);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Tutor {TutorId} removido.", id);

        return NoContent();
    }
}