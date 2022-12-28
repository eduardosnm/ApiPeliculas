using Microsoft.AspNetCore.Identity;

namespace ApiPeliculas.Modelos;

public class AppUsuario : IdentityUser
{
    //Añadir campos personalizados
    public string Nombre { get; set; }
}