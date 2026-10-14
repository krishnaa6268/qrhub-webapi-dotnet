using Microsoft.EntityFrameworkCore;
using QRHub.Application.Common.Exceptions;
using QRHub.Application.Common.Interfaces;
using QRHub.Application.DTOs.Auth;
using QRHub.Domain.Entities;

namespace QRHub.Application.Services;

public class AuthService : IAuthService
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public AuthService(
        IApplicationDbContext context,
        IPasswordHasher _passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _context = context;
        this._passwordHasher = _passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var existingUser = await _context.Users
            .AnyAsync(u => u.Email == normalizedEmail, cancellationToken);

        if (existingUser)
        {
            throw new ConflictException("A user with this email address already exists.");
        }

        var passwordHash = _passwordHasher.HashPassword(request.Password);

        var user = new User
        {
            Name = request.Name.Trim(),
            Email = normalizedEmail,
            PasswordHash = passwordHash,
            CreatedAtUtc = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);

        var (token, expiresAtUtc) = _jwtTokenGenerator.GenerateToken(user);

        return new AuthResponse
        {
            Token = token,
            TokenType = "Bearer",
            ExpiresAtUtc = expiresAtUtc,
            User = new UserDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                CreatedAtUtc = user.CreatedAtUtc
            }
        };
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == normalizedEmail, cancellationToken);

        if (user == null)
        {
            throw new UnauthorizedException("Invalid email or password.");
        }

        var isPasswordValid = _passwordHasher.VerifyPassword(request.Password, user.PasswordHash);
        if (!isPasswordValid)
        {
            throw new UnauthorizedException("Invalid email or password.");
        }

        var (token, expiresAtUtc) = _jwtTokenGenerator.GenerateToken(user);

        return new AuthResponse
        {
            Token = token,
            TokenType = "Bearer",
            ExpiresAtUtc = expiresAtUtc,
            User = new UserDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                CreatedAtUtc = user.CreatedAtUtc
            }
        };
    }

    public async Task<UserDto> GetUserProfileAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null)
        {
            throw new NotFoundException("User not found.");
        }

        return new UserDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            CreatedAtUtc = user.CreatedAtUtc
        };
    }
}
