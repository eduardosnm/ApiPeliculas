using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ApiPeliculas.Data;
using ApiPeliculas.Modelos;
using ApiPeliculas.Modelos.Dtos;
using ApiPeliculas.Repositorio.IRepositorio;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using MD5CryptoServiceProvider = XSystem.Security.Cryptography.MD5CryptoServiceProvider;

namespace ApiPeliculas.Repositorio;

public class UsuarioRepositorio : IUsuarioRepositorio
{
    private readonly ApplicationDbContext _db;
    private string claveSecreta;
    private readonly UserManager<AppUsuario> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IMapper _mapper;

    public UsuarioRepositorio(
        ApplicationDbContext db, 
        IConfiguration configuration, 
        UserManager<AppUsuario> userManager, 
        RoleManager<IdentityRole> roleManager,
        IMapper mapper
        )
    {
        _db = db;
        claveSecreta = configuration.GetValue<string>("ApiSettings:Secreta");
        _userManager = userManager;
        _roleManager = roleManager;
        _mapper = mapper;
    }
    
    public ICollection<AppUsuario> GetUsuarios()
    {
        return _db.AppUsuario.OrderBy(u => u.UserName).ToList();
    }

    public AppUsuario GetUsuario(string usuarioId)
    {
        return _db.AppUsuario.FirstOrDefault(u => u.Id == usuarioId);
    }

    public bool IsUniqueUser(string usuario)
    {
        var usuarioDb = _db.AppUsuario.FirstOrDefault(u => u.UserName == usuario);
        return usuarioDb == null;
    }

    public async Task<UsuarioLoginRespuestaDto> Login(UsuarioLoginDto usuarioLoginDto)
    {
        var usuario = _db.AppUsuario.FirstOrDefault(
            u => u.UserName.ToLower() == usuarioLoginDto.NombreUsuario.ToLower()
            );

        bool isValida = await _userManager.CheckPasswordAsync(usuario, usuarioLoginDto.Password);

        if (usuario == null || isValida == false)
        {
            return new UsuarioLoginRespuestaDto()
            {
                Token = "",
                Usuario = null
            };
        }

        var roles = await _userManager.GetRolesAsync(usuario);

        var manejadorToken = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(claveSecreta);

        var tokenDescriptor = new SecurityTokenDescriptor()
        {
            Subject = new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, usuario.UserName.ToString()),
                new Claim(ClaimTypes.Role, roles.FirstOrDefault())
            }),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = new(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = manejadorToken.CreateToken(tokenDescriptor);
        UsuarioLoginRespuestaDto usuarioLoginRespuestaDto = new UsuarioLoginRespuestaDto()
        {
            Token = manejadorToken.WriteToken(token),
            Usuario = _mapper.Map<UsuarioDatosDto>(usuario)
        };
        
        return usuarioLoginRespuestaDto;
    }

    public async Task<UsuarioDatosDto> Registro(UsuarioRegistroDto usuarioRegistroDto)
    {
        AppUsuario usuario = new AppUsuario()
        {
            UserName = usuarioRegistroDto.NombreUsuario,
            Email = usuarioRegistroDto.NombreUsuario,
            NormalizedEmail = usuarioRegistroDto.NombreUsuario.ToUpper(),
            Nombre = usuarioRegistroDto.Nombre
        };

        var result = await _userManager.CreateAsync(usuario, usuarioRegistroDto.Password);
        if (result.Succeeded)
        {
            //Solo la primera vez y es para crear los roles
            if (!_roleManager.RoleExistsAsync("admin").GetAwaiter().GetResult())
            {
                await _roleManager.CreateAsync(new IdentityRole("admin"));
                await _roleManager.CreateAsync(new IdentityRole("registrado"));
            }

            await _userManager.AddToRoleAsync(usuario, "registrado");
            var usuarioRetornado = _db.AppUsuario.FirstOrDefault(u => u.UserName == usuarioRegistroDto.NombreUsuario);
            //Opcion 1
            // return new UsuarioDatosDto()
            // {
            //     Id = usuarioRetornado.Id,
            //     UserName = usuarioRetornado.UserName,
            //     Nombre = usuarioRetornado.Nombre
            // };

            return _mapper.Map<UsuarioDatosDto>(usuarioRetornado);
        }

        return new UsuarioDatosDto();
    }
}