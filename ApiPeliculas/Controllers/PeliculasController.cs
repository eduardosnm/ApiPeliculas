using ApiPeliculas.Modelos;
using ApiPeliculas.Modelos.Dtos;
using ApiPeliculas.Repositorio.IRepositorio;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiPeliculas.Controllers;

[Route("api/peliculas")]
[ApiController]
public class PeliculasController : ControllerBase
{
    
    private readonly IPeliculaRepositorio _pelRepo;
    private readonly IMapper _mapper;

    public PeliculasController(IPeliculaRepositorio ctRepo, IMapper mapper)
    {
        _pelRepo = ctRepo;
        _mapper = mapper;
    }

    [AllowAnonymous]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetPeliculas()
    {
        var listaPeliculas = _pelRepo.GetPeliculas();

        var listaPeliculasDto = _mapper.Map<List<PeliculaDto>>(listaPeliculas);

        return Ok(listaPeliculasDto);
    }
    
    [AllowAnonymous]
    [HttpGet("{peliculaId:int}", Name = "GetPelicula")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetPelicula(int peliculaId)
    {
        var itemPelicula = _pelRepo.GetPelicula(peliculaId);

        if (itemPelicula == null)
        {
            return NotFound();
        }

        var itemPeliculaDto = _mapper.Map<PeliculaDto>(itemPelicula);

        return Ok(itemPeliculaDto);
    }
    
    [Authorize(Roles = "admin")]
    [HttpPost]
    [ProducesResponseType(201, Type = typeof(PeliculaDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public IActionResult CrearPelicula([FromBody] PeliculaDto peliculaDto)
    {
        // valida las restricciones que pusimos en el dto
        if (!ModelState.IsValid || peliculaDto == null)
        {
            return BadRequest(ModelState);
        }

        if (_pelRepo.ExistePelicula(peliculaDto.Nombre))
        {
            ModelState.AddModelError("", "La pelicula ya existe");
            return StatusCode(404, ModelState);
        }

        var pelicula = _mapper.Map<Pelicula>(peliculaDto);
        if (!_pelRepo.CrearPelicula(pelicula))
        {
            ModelState.AddModelError("", $"Algo salio mal guardando el registro {pelicula.Nombre}");
            return StatusCode(500, ModelState);
        }

        return CreatedAtRoute("GetPelicula", new { peliculaId = pelicula.Id }, pelicula);
    }
    
    [Authorize(Roles = "admin")]
    [HttpPatch("{peliculaId:int}", Name = "ActualizarPatchPelicula")]
    [ProducesResponseType(204, Type = typeof(PeliculaDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public IActionResult ActualizarPatchPelicula(int peliculaId, [FromBody] PeliculaDto peliculaDto)
    {
        // valida las restricciones que pusimos en el dto
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (peliculaDto == null || peliculaId != peliculaDto.Id)
        {
            return BadRequest(ModelState);
        }
        
        var pelicula = _mapper.Map<Pelicula>(peliculaDto);
        if (!_pelRepo.ActualizarPelicula(pelicula))
        {
            ModelState.AddModelError("", $"Algo salio mal guardando el registro {pelicula.Nombre}");
            return StatusCode(500, ModelState);
        }

        return NoContent();
    }
    
    [Authorize(Roles = "admin")]
    [HttpDelete("{peliculaId:int}", Name = "BorrarPelicula")]
    [ProducesResponseType(201, Type = typeof(PeliculaDto))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public IActionResult BorrarPelicula(int peliculaId)
    {
        // valida las restricciones que pusimos en el dto
        if (!_pelRepo.ExistePelicula(peliculaId))
        {
            return NotFound();
        }

        var pelicula = _pelRepo.GetPelicula(peliculaId);
        if (!_pelRepo.BorrarPelicula(pelicula))
        {
            ModelState.AddModelError("", $"Algo salio mal borrando el registro {pelicula.Nombre}");
            return StatusCode(500, ModelState);
        }

        return NoContent();
    }
    
    [HttpGet("peliculas-por-categoria/{categoriaId:int}")]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetPeliculasPorCategoria(int categoriaId)
    {
        var listaPeliculas = _pelRepo.GetPeliculasPorCategoria(categoriaId);

        if (listaPeliculas == null)
        {
            return NotFound();
        }

        var listaPeliculasDto = _mapper.Map<List<PeliculaDto>>(listaPeliculas);

        return Ok(listaPeliculasDto);
    }
    
    [HttpGet("buscar")]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Buscar(string nombre)
    {
        try
        {
            var resultado = _pelRepo.GetPeliculasPorNombre(nombre);

            if (!resultado.Any())
            {
                return NotFound();
            }
            
            return Ok(resultado);
        }
        catch (Exception e)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, "Error recuperando datos");
        }
    }
}