using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using WeatherAPI.Application.Common;
using WeatherAPI.Application.Configuration;
using WeatherAPI.Application.Dtos;
using WeatherAPI.Application.Interfaces;
using WeatherAPI.Domain.Entities;

namespace WeatherAPI.Application.Service;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly AuthOptions _authOptions;
    private readonly IUserSessionRepository _userSessionRepository;
    
    public AuthService(
        IUserRepository userRepository,
        IOptions<AuthOptions> authOptions,
        IUserSessionRepository userSessionRepository)
    {
        _userRepository = userRepository;
        _authOptions = authOptions.Value;
        _userSessionRepository = userSessionRepository;
    }
    
    public async Task<RegisterUserResponseDto> RegisterAsync(RegisterUserRequestDto request, CancellationToken cancellationToken)
    {
        // check maila
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var doesExist = await _userRepository.ExistsByEmailAsync(normalizedEmail, cancellationToken);
        if (doesExist)
            throw new BadRequestException("Email already exists");
        
        // check sifre
        ValidatePassword(request.Password);
        
        // hashiranje sifre
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);
        
        // spremanje u bazu
        var normalizedFirstName = request.FirstName.Trim();
        var normalizedLastName = request.LastName.Trim();
        var user = User.Create(normalizedFirstName, normalizedLastName, normalizedEmail, hashedPassword);
        var newUser = await _userRepository.AddAsync(user, cancellationToken);
        var response = new RegisterUserResponseDto
        {
            Id = newUser.Id,
            FirstName = newUser.FirstName,
            LastName = newUser.LastName,
            Email = newUser.Email,
            CreatedAt = newUser.CreatedAt,
        };
        return response;
        // TODO: kreiranje sessije 
    }

    public async Task<LoginUserResultDto> LoginAsync(LoginUserRequestDto request, CancellationToken cancellationToken)
    {
        // provjera credentialsa
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var user = await _userRepository.GetByEmailAsync(normalizedEmail, cancellationToken);

        if (user == null)
            throw new BadRequestException("Invalid email or password");

        var isPasswordCorrect = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);

        if (!isPasswordCorrect)
            throw new BadRequestException("Invalid email or password");

        // kreiranje sesije
        var rawToken = GenerateSessionToken();
        var hashedToken = HashSessionToken(rawToken);
        var expiresAt = DateTime.UtcNow.AddDays(_authOptions.SessionDurationDays);
        
        var session = UserSession.Create(user, hashedToken, expiresAt);
        await _userSessionRepository.AddAsync(session, cancellationToken);
        
        // kreiranje responsa koji vraca usera, sessiju i kad istice
        return new LoginUserResultDto
        {
            User = new LoginUserResponseDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Role = user.Role
            },
            SessionToken = rawToken,
            ExpiresAt = expiresAt
        };
    }

    public async Task<CurrentUserResponseDto> GetCurrentUserAsync(string sessionToken, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(sessionToken))
            throw new UnauthorizedAccessException("User is not authenticated");
        
        var hashedSessionToken = HashSessionToken(sessionToken);
        var session = await _userSessionRepository.GetByTokenAsync(hashedSessionToken, cancellationToken);

        if (session is null || !session.IsActive())
            throw new UnauthorizedAccessException("Invalid or expired session");
        
        return new CurrentUserResponseDto
        {
            Id = session.User.Id,
            FirstName = session.User.FirstName,
            LastName = session.User.LastName,
            Email = session.User.Email,
            Role = session.User.Role
        };
    }
    
    private static void ValidatePassword(string password)
    {
        var hasDigit = password.Any(char.IsDigit);
        var hasUppercase = password.Any(char.IsUpper);

        if (!hasDigit || !hasUppercase)
            throw new BadRequestException("Password must contain at least one number and one uppercase letter");
    }

    private static string GenerateSessionToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');
    }
    
    private static string HashSessionToken(string token)
    {
        var tokenBytes = Encoding.UTF8.GetBytes(token);
        var hashBytes = SHA256.HashData(tokenBytes);
        
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }
}
