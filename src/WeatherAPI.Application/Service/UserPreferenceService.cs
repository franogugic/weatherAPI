using WeatherAPI.Application.Dtos;
using WeatherAPI.Application.Interfaces;
using WeatherAPI.Domain.Constants;
using WeatherAPI.Domain.Entities;

namespace WeatherAPI.Application.Service;

public class UserPreferenceService : IUserPreferenceService
{
    private readonly IAuthService _authService;
    private readonly IUserPreferenceRepository _userPreferenceRepository;

    public UserPreferenceService(IAuthService authService, IUserPreferenceRepository userPreferenceRepository)
    {
        _authService = authService;
        _userPreferenceRepository = userPreferenceRepository;
    }
    
    public async Task<GetUserPreferenceResponseDto> GetCurrentUserPreferencesAsync(
        string sessionToken,
        CancellationToken cancellationToken = default)
    {
        var user = await _authService.GetCurrentUserAsync(sessionToken ,cancellationToken);
        
        var userPreference = await _userPreferenceRepository.GetByUserIdAsync(user.Id, cancellationToken);
        if (userPreference is null)
        {
            userPreference = UserPreference.Create(
                user.Id,
                DefaultUserPreferenceUnits.TemperatureUnit,
                DefaultUserPreferenceUnits.WindSpeedUnit,
                DefaultUserPreferenceUnits.PressureUnit,
                DefaultUserPreferenceUnits.CloudinessUnit,
                DefaultUserPreferenceUnits.PrecipitationUnit
            );

            await _userPreferenceRepository.AddAsync(userPreference, cancellationToken);
        }

        return MapToResponse(userPreference);
    }

    private static GetUserPreferenceResponseDto MapToResponse(UserPreference userPreference)
    {
        return new GetUserPreferenceResponseDto
        {
            CloudinessUnit = userPreference.CloudinessUnit,
            PrecipitationUnit = userPreference.PrecipitationUnit,
            PressureUnit = userPreference.PressureUnit,
            TemperatureUnit = userPreference.TemperatureUnit,
            WindSpeedUnit = userPreference.WindSpeedUnit
        };
    }

}
