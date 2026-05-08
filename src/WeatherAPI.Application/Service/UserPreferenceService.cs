using WeatherAPI.Application.Common;
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

    public async Task<GetUserPreferenceResponseDto> UpdateCurrentUserPreferencesAsync(
        string sessionToken,
        UpdateUserPreferenceRequestDto request,
        CancellationToken cancellationToken)
    {
        var user = await _authService.GetCurrentUserAsync(sessionToken, cancellationToken);
        
        //provjera unesenih podataka
        ValidateUnit(UserPreferenceUnits.TemperatureUnits, request.TemperatureUnit, nameof(request.TemperatureUnit));
        ValidateUnit(UserPreferenceUnits.WindSpeedUnits, request.WindSpeedUnit, nameof(request.WindSpeedUnit));
        ValidateUnit(UserPreferenceUnits.PressureUnits, request.PressureUnit, nameof(request.PressureUnit));
        ValidateUnit(UserPreferenceUnits.CloudinessUnits, request.CloudinessUnit, nameof(request.CloudinessUnit));
        ValidateUnit(UserPreferenceUnits.PrecipitationUnits, request.PrecipitationUnit, nameof(request.PrecipitationUnit));

        //
        var userPreference = await _userPreferenceRepository.GetByUserIdAsync(user.Id, cancellationToken);

        if (userPreference is null)
        {
            userPreference = UserPreference.Create(
                user.Id,
                request.TemperatureUnit,
                request.WindSpeedUnit,
                request.PressureUnit,
                request.CloudinessUnit,
                request.PrecipitationUnit
            );

            await _userPreferenceRepository.AddAsync(userPreference, cancellationToken);
        }
        else
        {
            userPreference.Update(
                request.TemperatureUnit,
                request.WindSpeedUnit,
                request.PressureUnit,
                request.CloudinessUnit,
                request.PrecipitationUnit);

            await _userPreferenceRepository.SaveChangesAsync(cancellationToken);
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

    private static void ValidateUnit(
        IReadOnlySet<string> allowedValues,
        string value,
        string fieldName)
    {
        if (!allowedValues.Contains(value))
            throw new BadRequestException($"{fieldName} is invalid.");
    }

}
