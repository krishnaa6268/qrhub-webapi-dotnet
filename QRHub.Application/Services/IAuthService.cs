using QRHub.Application.DTOs.Auth;

namespace QRHub.Application.Services;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task<UserDto> GetUserProfileAsync(Guid userId, CancellationToken cancellationToken = default);
}
