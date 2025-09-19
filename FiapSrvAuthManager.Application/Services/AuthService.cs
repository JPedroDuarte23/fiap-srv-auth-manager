using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FiapSrvAuthManager.Application.DTOs;
using FiapSrvAuthManager.Application.Exceptions;
using FiapSrvAuthManager.Application.Interface;
using FiapSrvAuthManager.Domain.Entities;
using FiapSrvAuthManager.Domain.Enums;
using FiapSrvAuthManager.Domain.Interface;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace FiapCloudGames.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _repository;
    private readonly IConfiguration _config;
    private readonly ILogger<AuthService> _logger;
    private readonly string _jwtSigningKey;

    public AuthService(IUserRepository repository, IConfiguration config, ILogger<AuthService> logger, string jwtSigningKey)
    {
        _repository = repository;
        _config = config;
        _logger = logger;
        _jwtSigningKey = jwtSigningKey;
    }

    public async Task<TokenDto> AuthenticateAsync(AuthenticateDto dto)
    {
        _logger.LogInformation("Tentando autenticar usuário com e-mail {Email}", dto.Email);

        var user = await _repository.GetByEmailAsync(dto.Email);

        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
        {
            _logger.LogWarning("Falha na autenticação para o usuário com e-mail {Email}", dto.Email);
            throw new UnauthorizedException("E-mail ou senha inválidos");
        }
            
        _logger.LogInformation("Usuário {Email} autenticado com sucesso", dto.Email);
        return new TokenDto(GenerateJwt(user));
    }

    public async Task<PlayerDto> RegisterPlayerAsync(RegisterPlayerDto dto)
    {
        _logger.LogInformation("Iniciando registro de novo jogador com e-mail {Email}", dto.Email);
        var usuarioExiste = await _repository.GetByEmailAsync(dto.Email);

        if (usuarioExiste != null)
        {
            _logger.LogWarning("Tentativa de cadastro com e-mail já existente: {Email}", dto.Email);
            throw new ConflictException("E-mail já cadastrado.");
        }

        var player = new Player
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Username = dto.Username,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            BornDate = dto.BornDate,
            Cpf = dto.Cpf,
            Library = new List<Guid>(),
            Cart = new List<Guid>(),
            Wishlist = new List<Guid>(),
            Role = UserRole.Player,
            CreatedAt = DateTime.UtcNow
        };
        try
        {
            await _repository.CreateAsync(player);

            _logger.LogInformation("Jogador {Email} cadastrado com sucesso", dto.Email);

            return new PlayerDto(player);
        } 
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao cadastrar jogador com e-mail {Email}", dto.Email);
            throw new ModifyDatabaseException(ex.Message);   
        }


    }

    public async Task<PublisherDto> RegisterPublisherAsync(RegisterPublisherDto dto)
    {
        _logger.LogInformation("Iniciando registro de nova publisher com e-mail {Email}", dto.Email);

        var usuarioExiste = await _repository.GetByEmailAsync(dto.Email);

        if (usuarioExiste != null)
        {
            _logger.LogWarning("Tentativa de cadastro de publisher com e-mail já existente: {Email}", dto.Email);
            throw new ConflictException("E-mail já cadastrado.");
        }

        var publisher = new Publisher
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Username = dto.Username,
            PublisherName = dto.PublisherName,
            CompanyName = dto.CompanyName,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            BornDate = dto.BornDate,
            Cnpj = dto.Cnpj,
            GamesPublished = new List<Guid>(),
            TeamMembers = new List<Guid>(),
            Role = UserRole.Publisher,
            CreatedAt = DateTime.UtcNow
        };

        try
        {
            await _repository.CreateAsync(publisher);

            _logger.LogInformation("Publisher {Email} cadastrada com sucesso", dto.Email);

            return new PublisherDto(publisher);
        }
        catch (Exception ex) 
        {
            _logger.LogError(ex, "Erro ao cadastrar publisher com e-mail {Email}", dto.Email);
            throw new ModifyDatabaseException(ex.Message);
        }
    }

    private string GenerateJwt(User user)
    {
        var keyBytes = Convert.FromBase64String(_jwtSigningKey);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
            new Claim(ClaimTypes.Name, user.Id.ToString()),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        }),
            Expires = DateTime.UtcNow.AddHours(3),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(keyBytes),
                SecurityAlgorithms.HmacSha256Signature
            ),
            Issuer = _config["Jwt:Issuer"],
            Audience = _config["Jwt:Audience"]
        };

        var handler = new JwtSecurityTokenHandler();
        var token = handler.CreateToken(tokenDescriptor);
        return handler.WriteToken(token);
    }
}
