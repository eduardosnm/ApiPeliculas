using ApiPeliculas.Modelos;
using Microsoft.EntityFrameworkCore;

namespace ApiPeliculas.Data;


public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
        
    }
    
    //Agregar modelos aqui
    public DbSet<Categoria> Categoria { get; set; }
    public DbSet<Pelicula> Pelicula { get; set; }
}