using ApiPeliculas.Modelos;

namespace ApiPeliculas.Repositorio.IRepositorio;

public interface IPeliculaRepositorio
{
    ICollection<Pelicula> GetPeliculas();

    Pelicula GetPelicula(int peliculaId);

    bool ExistePelicula(string nombre);
    
    bool ExistePelicula(int id);

    bool CrearPelicula(Pelicula pelicula);
    
    bool ActualizarPelicula(Pelicula pelicula);
    
    bool BorrarPelicula(Pelicula pelicula);
    
    //Metodos para buscar peliculas segun categoria y nombre de pelicula
    ICollection<Pelicula> GetPeliculasPorCategoria(int categoriaId);

    ICollection<Pelicula> GetPeliculasPorNombre(string nombre);

    bool Guardar();
}