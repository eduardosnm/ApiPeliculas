using ApiPeliculas.Modelos;
using ApiPeliculas.Modelos.Dtos;
using ApiPeliculas.Repositorio.IRepositorio;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace ApiPeliculas.Controllers;

[ApiController]
// [Route("api/[controller]")] una opcion
[Route("api/categorias")]
public class CategoriasController : ControllerBase
{
    private readonly ICategoriaRepositorio _ctRepo;
    private readonly IMapper _mapper;

    public CategoriasController(ICategoriaRepositorio ctRepo, IMapper mapper)
    {
        _ctRepo = ctRepo;
        _mapper = mapper;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetCategorias()
    {
        var listaCategorias = _ctRepo.GetCategorias();

        var listaCategoriasDto = new List<CategoriaDto>();

        foreach (var lista in listaCategorias)
        {
            listaCategoriasDto.Add(_mapper.Map<CategoriaDto>(lista));
        }

        return Ok(listaCategoriasDto);
    }
    
    [HttpGet("{categoriaId:int}", Name = "GetCategoria")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetCategoria(int categoriaId)
    {
        var itemCategoria = _ctRepo.GetCategoria(categoriaId);

        if (itemCategoria == null)
        {
            return NotFound();
        }

        var itemCategoriaDto = _mapper.Map<CategoriaDto>(itemCategoria);

        return Ok(itemCategoriaDto);
    }
    
    [HttpPost]
    [ProducesResponseType(201, Type = typeof(CategoriaDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public IActionResult CrearCategoria([FromBody] CrearCategoriaDto crearCategoriaDto)
    {
        // valida las restricciones que pusimos en el dto
        if (!ModelState.IsValid || crearCategoriaDto == null)
        {
            return BadRequest(ModelState);
        }

        if (_ctRepo.ExisteCategoria(crearCategoriaDto.Nombre))
        {
            ModelState.AddModelError("", "La categoria ya existe");
            return StatusCode(404, ModelState);
        }

        var categoria = _mapper.Map<Categoria>(crearCategoriaDto);
        if (!_ctRepo.CrearCategoria(categoria))
        {
            ModelState.AddModelError("", $"Algo salio mal guardando el registro {categoria.Nombre}");
            return StatusCode(500, ModelState);
        }

        return CreatedAtRoute("GetCategoria", new { categoriaId = categoria.Id }, categoria);
    }
    
    [HttpPatch("{categoriaId:int}", Name = "ActualizarPatchCategoria")]
    [ProducesResponseType(201, Type = typeof(CategoriaDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public IActionResult ActualizarPatchCategoria(int categoriaId, [FromBody] CategoriaDto categoriaDto)
    {
        // valida las restricciones que pusimos en el dto
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (categoriaDto == null || categoriaId != categoriaDto.Id)
        {
            return BadRequest(ModelState);
        }
        
        var categoria = _mapper.Map<Categoria>(categoriaDto);
        if (!_ctRepo.ActualizarCategoria(categoria))
        {
            ModelState.AddModelError("", $"Algo salio mal guardando el registro {categoria.Nombre}");
            return StatusCode(500, ModelState);
        }

        return NoContent();
    }
    
    [HttpDelete("{categoriaId:int}", Name = "BorrarCategoria")]
    [ProducesResponseType(201, Type = typeof(CategoriaDto))]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public IActionResult BorrarCategoria(int categoriaId)
    {
        // valida las restricciones que pusimos en el dto
        if (!_ctRepo.ExisteCategoria(categoriaId))
        {
            return BadRequest(ModelState);
        }

        var categoria = _ctRepo.GetCategoria(categoriaId);
        if (!_ctRepo.BorrarCategoria(categoria))
        {
            ModelState.AddModelError("", $"Algo salio mal borrando el registro {categoria.Nombre}");
            return StatusCode(500, ModelState);
        }

        return NoContent();
    }

}