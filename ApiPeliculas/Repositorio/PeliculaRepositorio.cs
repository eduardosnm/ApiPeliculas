using ApiPeliculas.Data;
using ApiPeliculas.Modelos;
using ApiPeliculas.Repositorio.IRepositorio;
using Microsoft.EntityFrameworkCore;

namespace ApiPeliculas.Repositorio;

public class PeliculaRepositorio : IPeliculaRepositorio
{
    private readonly ApplicationDbContext _db;

    public PeliculaRepositorio(ApplicationDbContext db)
    {
        _db = db;
    }
    
    public ICollection<Pelicula> GetPeliculas()
    {
        return _db.Pelicula.OrderBy(c => c.Nombre).ToList();
    }

    public Pelicula GetPelicula(int peliculaId)
    {
        return _db.Pelicula.FirstOrDefault(c => c.Id == peliculaId);
    }

    public bool ExistePelicula(string nombre)
    {
        bool valor = _db.Pelicula.Any(c => c.Nombre.ToLower().Trim() == nombre.ToLower().Trim());
        return valor;
    }

    public bool ExistePelicula(int id)
    {
        return _db.Pelicula.Any(c => c.Id == id);
    }

    public bool CrearPelicula(Pelicula pelicula)
    {
        pelicula.FechaCreacion = DateTime.Now;
        _db.Pelicula.Add(pelicula);
        return Guardar();
    }

    public bool ActualizarPelicula(Pelicula pelicula)
    {
        pelicula.FechaCreacion = DateTime.Now;
        _db.Pelicula.Update(pelicula);
        return Guardar();
    }

    public bool BorrarPelicula(Pelicula pelicula)
    {
        _db.Pelicula.Remove(pelicula);
        return Guardar();
    }

    public ICollection<Pelicula> GetPeliculasPorCategoria(int categoriaId)
    {
        return _db.Pelicula.Include(ca => ca.Categoria).Where(ca => ca.categoriaId == categoriaId).ToList();
    }

    public ICollection<Pelicula> GetPeliculasPorNombre(string nombre)
    {
        IQueryable<Pelicula> query = _db.Pelicula;

        if (!string.IsNullOrEmpty(nombre))
        {
            query = query.Where(e => e.Nombre.Contains(nombre) || e.Descripcion.Contains(nombre));
        }

        return query.ToList();
    }

    public bool Guardar()
    {
        return _db.SaveChanges() >= 0 ? true : false;
    }
}