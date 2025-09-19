using FiapSrvAuthManager.Application.DTOs;

namespace FiapSrvAuthManager.Application.Interface;

public interface IAuthService
{
    Task<TokenDto> AuthenticateAsync(AuthenticateDto dto);

    Task<PlayerDto> RegisterPlayerAsync(RegisterPlayerDto dto);

    Task<PublisherDto> RegisterPublisherAsync(RegisterPublisherDto dto);
}
