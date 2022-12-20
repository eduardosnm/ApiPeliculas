using System.ComponentModel.DataAnnotations;

namespace ApiPeliculas.Modelos.Dtos;

public class CategoriaDto
{
    public int Id { get; set; }
    
    [Required(ErrorMessage = "El nombre es obligatorio")] 
    [MaxLength(100, ErrorMessage = "El numero maximo de cararcteres es de 100!")]
    public string Nombre { get; set; }
}