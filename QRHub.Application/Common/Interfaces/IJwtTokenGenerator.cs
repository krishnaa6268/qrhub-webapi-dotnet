using QRHub.Domain.Entities;

namespace QRHub.Application.Common.Interfaces;

public interface IJwtTokenGenerator
{
    (string Token, DateTime ExpiresAtUtc) GenerateToken(User user);
}
