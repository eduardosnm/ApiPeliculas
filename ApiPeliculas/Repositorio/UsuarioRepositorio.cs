using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ApiPeliculas.Data;
using ApiPeliculas.Modelos;
using ApiPeliculas.Modelos.Dtos;
using ApiPeliculas.Repositorio.IRepositorio;
using Microsoft.IdentityModel.Tokens;
using MD5CryptoServiceProvider = XSystem.Security.Cryptography.MD5CryptoServiceProvider;

namespace ApiPeliculas.Repositorio;

public class UsuarioRepositorio : IUsuarioRepositorio
{
    private readonly ApplicationDbContext _db;
    private string claveSecreta;

    public UsuarioRepositorio(ApplicationDbContext db, IConfiguration configuration)
    {
        _db = db;
        claveSecreta = configuration.GetValue<string>("ApiSettings:Secreta");
    }
    
    public ICollection<Usuario> GetUsuarios()
    {
        return _db.Usuario.OrderBy(u => u.NombreUsuario).ToList();
    }

    public Usuario GetUsuario(int usuarioId)
    {
        return _db.Usuario.FirstOrDefault(u => u.Id == usuarioId);
    }

    public bool IsUniqueUser(string usuario)
    {
        var usuarioDb = _db.Usuario.FirstOrDefault(u => u.NombreUsuario == usuario);
        return usuarioDb == null;
    }

    public async Task<UsuarioLoginRespuestaDto> Login(UsuarioLoginDto usuarioLoginDto)
    {
        var passwordEncriptado = obtenermd5(usuarioLoginDto.Password);

        var usuario = _db.Usuario.FirstOrDefault(
            u => u.NombreUsuario.ToLower() == usuarioLoginDto.NombreUsuario.ToLower()
            && u.Password == passwordEncriptado
            );

        if (usuario == null)
        {
            return new UsuarioLoginRespuestaDto()
            {
                Token = "",
                Usuario = null
            };
        }

        var manejadorToken = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(claveSecreta);

        var tokenDescriptor = new SecurityTokenDescriptor()
        {
            Subject = new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, usuario.NombreUsuario.ToString()),
                new Claim(ClaimTypes.Role, usuario.Role)
            }),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = new(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = manejadorToken.CreateToken(tokenDescriptor);
        UsuarioLoginRespuestaDto usuarioLoginRespuestaDto = new UsuarioLoginRespuestaDto()
        {
            Token = manejadorToken.WriteToken(token),
            Usuario = usuario
        };
        
        return usuarioLoginRespuestaDto;
    }

    public async Task<Usuario> Registro(UsuarioRegistroDto usuarioRegistroDto)
    {
        var passwordEncriptado = obtenermd5(usuarioRegistroDto.Password);

        Usuario usuario = new Usuario()
        {
            NombreUsuario = usuarioRegistroDto.NombreUsuario,
            Password = passwordEncriptado,
            Nombre = usuarioRegistroDto.Nombre,
            Role = usuarioRegistroDto.Role
        };

        _db.Usuario.Add(usuario);
        await _db.SaveChangesAsync();
        usuario.Password = passwordEncriptado;
        return usuario;
    }

    private string obtenermd5(string password)
    {
        MD5CryptoServiceProvider x = new MD5CryptoServiceProvider();
        byte[] data = System.Text.Encoding.UTF8.GetBytes(password);
        data = x.ComputeHash(data);
        string resp = "";
        for (int i = 0; i < data.Length; i++)
        {
            resp += data[i].ToString("x2").ToLower();
        }

        return resp;
    }
}