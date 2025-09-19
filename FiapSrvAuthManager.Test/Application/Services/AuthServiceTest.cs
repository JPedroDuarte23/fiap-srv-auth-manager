using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using FiapSrvAuthManager.Application;
using FiapSrvAuthManager.Application.Exceptions;
using FiapSrvAuthManager.Application.Interface;
using FiapSrvAuthManager.Application.Services;
using FiapSrvAuthManager.Domain.Entities;
using FiapSrvAuthManager.Domain.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
public class AuthServiceTest
{
    private readonly Mock<IUserRepository> _repoMock;
    private readonly Mock<IConfiguration> _configMock;
    private readonly Mock<ILogger<AuthService>> _loggerMock;
    private readonly AuthService _service;

    public AuthServiceTest()
    {
        _repoMock = new Mock<IUserRepository>();
        _configMock = new Mock<IConfiguration>();
        _loggerMock = new Mock<ILogger<AuthService>>();

        // Setup JWT config
        _configMock.Setup(c => c["Jwt:Key"]).Returns("12345678901234567890123456789012");
        _configMock.Setup(c => c["Jwt:Issuer"]).Returns("issuer");

        string publicKey = "LZ+6FWlb7t7ijNXeZKMNbQB0zJGrti9dUGxvGNobb88=";


        _service = new AuthService(
            _repoMock.Object,
            _configMock.Object,
            _loggerMock.Object,
            publicKey
            );
    }

    [Fact]
    public async Task AuthenticateAsync_ReturnsToken_WhenCredentialsAreValid()
    {
        var password = "Senha@123";
        var user = new Player
        {
            Id = Guid.NewGuid(),
            Email = "user@email.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Role = UserRole.Player
        };
        _repoMock.Setup(r => r.GetByEmailAsync(user.Email)).ReturnsAsync(user);

        var dto = new AuthenticateDto { Email = user.Email, Password = password };

        var result = await _service.AuthenticateAsync(dto);

        Assert.NotNull(result);
        Assert.False(string.IsNullOrEmpty(result.Token));
    }

    [Fact]
    public async Task AuthenticateAsync_ThrowsUnauthorizedException_WhenCredentialsAreInvalid()
    {
        var user = new Player
        {
            Id = Guid.NewGuid(),
            Email = "user@email.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Senha@123"),
            Role = UserRole.Player
        };
        _repoMock.Setup(r => r.GetByEmailAsync(user.Email)).ReturnsAsync(user);

        var dto = new AuthenticateDto { Email = user.Email, Password = "SenhaErrada" };

        await Assert.ThrowsAsync<UnauthorizedException>(() => _service.AuthenticateAsync(dto));
    }

    [Fact]
    public async Task RegisterPlayerAsync_ReturnsPlayerDto_WhenDataIsValid()
    {
        var dto = new RegisterPlayerDto
        {
            Name = "Player",
            Username = "player1",
            Email = "player@email.com",
            Password = "Senha@123",
            BornDate = DateTime.UtcNow.AddYears(-20),
            Cpf = "12345678901"
        };
        _repoMock.Setup(r => r.GetByEmailAsync(dto.Email)).ReturnsAsync((User)null);

        var result = await _service.RegisterPlayerAsync(dto);

        Assert.NotNull(result);
        Assert.Equal(dto.Email, result.Email);
    }

    [Fact]
    public async Task RegisterPlayerAsync_ThrowsConflictException_WhenEmailExists()
    {
        var dto = new RegisterPlayerDto
        {
            Name = "Player",
            Username = "player1",
            Email = "player@email.com",
            Password = "Senha@123",
            BornDate = DateTime.UtcNow.AddYears(-20),
            Cpf = "12345678901"
        };
        _repoMock.Setup(r => r.GetByEmailAsync(dto.Email)).ReturnsAsync(new Player());

        await Assert.ThrowsAsync<ConflictException>(() => _service.RegisterPlayerAsync(dto));
    }

    [Fact]
    public async Task RegisterPublisherAsync_ReturnsPublisherDto_WhenDataIsValid()
    {
        var dto = new RegisterPublisherDto
        {
            Name = "Publisher",
            PublisherName = "Editora",
            CompanyName = "Empresa",
            Username = "pub1",
            Email = "pub@email.com",
            Password = "Senha@123",
            BornDate = DateTime.UtcNow.AddYears(-20),
            Cnpj = "12345678000199"
        };
        _repoMock.Setup(r => r.GetByEmailAsync(dto.Email)).ReturnsAsync((User)null);

        var result = await _service.RegisterPublisherAsync(dto);

        Assert.NotNull(result);
        Assert.Equal(dto.Email, result.Email);
    }

    [Fact]
    public async Task RegisterPublisherAsync_ThrowsConflictException_WhenEmailExists()
    {
        var dto = new RegisterPublisherDto
        {
            Name = "Publisher",
            PublisherName = "Editora",
            CompanyName = "Empresa",
            Username = "pub1",
            Email = "pub@email.com",
            Password = "Senha@123",
            BornDate = DateTime.UtcNow.AddYears(-20),
            Cnpj = "12345678000199"
        };
        _repoMock.Setup(r => r.GetByEmailAsync(dto.Email)).ReturnsAsync(new Publisher());

        await Assert.ThrowsAsync<ConflictException>(() => _service.RegisterPublisherAsync(dto));
    }
}
