using WeatherAPI.Application.Common;
using WeatherAPI.Application.Dtos;
using WeatherAPI.Application.Interfaces;
using WeatherAPI.Domain.Entities;

namespace WeatherAPI.Application.Service;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    
    public AuthService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }
    
    public async Task<RegisterUserResponseDto> RegisterAsync(RegisterUserRequestDto request, CancellationToken cancellationToken)
    {
        //check maila
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var doesExist = await _userRepository.ExistsByEmailAsync(normalizedEmail, cancellationToken);
        if (doesExist)
            throw new BadRequestException("Email already exists");
        //check sifre
        ValidatePassword(request.Password);
        
        //hashiranje sifre
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);
        
        //spremanje u bazu
        var normalizedFirstName = request.FirstName.Trim();
        var normalizedLastName = request.LastName.Trim();
        var user = User.Create(normalizedFirstName, normalizedLastName, normalizedEmail, hashedPassword);
        var newUser =  await _userRepository.AddAsync(user, cancellationToken);
        var response = new RegisterUserResponseDto
        {
            Id = newUser.Id,
            FirstName = newUser.FirstName,
            LastName = newUser.LastName,
            Email = newUser.Email,
            CreatedAt = newUser.CreatedAt,
        };
        return response;
        //TODO: kreiranje sessije 
    }

    private static void ValidatePassword(string password)
    {
        var hasDigit = password.Any(char.IsDigit);
        var hasUppercase = password.Any(char.IsUpper);

        if (!hasDigit || !hasUppercase)
            throw new BadRequestException("Password must contain at least one number and one uppercase letter");
    }

}