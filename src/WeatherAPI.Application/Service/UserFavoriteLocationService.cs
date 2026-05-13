using WeatherAPI.Application.Common;
using WeatherAPI.Application.Dtos;
using WeatherAPI.Application.Interfaces;
using WeatherAPI.Domain.Entities;

namespace WeatherAPI.Application.Service;

public class UserFavoriteLocationService : IUserFavoriteLocationService
{
    private readonly IUserFavoriteLocationRepository _userFavoriteLocationRepository;
    private readonly IAuthService _authService;
    private readonly ILocationRepository _locationRepository;

    public UserFavoriteLocationService(IUserFavoriteLocationRepository userFavoriteLocationRepository,
        IAuthService authService,
        ILocationRepository locationRepository)
    {
        _userFavoriteLocationRepository = userFavoriteLocationRepository;
        _authService = authService;
        _locationRepository = locationRepository;
    }
    
    public async Task<List<GetLocationResponseDto>> GetCurrentUserFavoriteLocationsAsync(
        string sessionToken,
        CancellationToken cancellationToken = default)
    {
        var user = await _authService.GetCurrentUserAsync(sessionToken, cancellationToken);
        
        var favoriteLocations = await _userFavoriteLocationRepository.GetByUserIdAsync(user.Id, cancellationToken);
        var favoriteLocationIds = favoriteLocations.Select(favorite => favorite.LocationId);

        return await _locationRepository.GetLocationsWithCurrentWeatherAsync(
            favoriteLocationIds,
            cancellationToken);
    }

    public async Task<GetLocationResponseDto> AddCurrentUserFavoriteLocationAsync(
        string sessionToken,
        short locationId,
        CancellationToken cancellationToken = default)
    {
        var user = await _authService.GetCurrentUserAsync(sessionToken, cancellationToken);
        var location = await _locationRepository.GetByIdAsync(locationId, cancellationToken);

        if (location is null)
            throw new NotFoundException("Location was not found.");

        var alreadyFavorite = await _userFavoriteLocationRepository.ExistsAsync(
            user.Id,
            locationId,
            cancellationToken);

        if (alreadyFavorite)
            throw new ConflictException("Location is already in favorites.");

        var favoriteLocation = UserFavoriteLocation.Create(user.Id, locationId);
        await _userFavoriteLocationRepository.AddAsync(favoriteLocation, cancellationToken);

        return (await _locationRepository.GetLocationsWithCurrentWeatherAsync(
                [locationId],
                cancellationToken))
            .FirstOrDefault()
            ?? MapToResponse(location);
    }

    public async Task RemoveCurrentUserFavoriteLocationAsync(
        string sessionToken,
        short locationId,
        CancellationToken cancellationToken = default)
    {
        var user = await _authService.GetCurrentUserAsync(sessionToken, cancellationToken);
        var favoriteLocation = await _userFavoriteLocationRepository.GetAsync(
            user.Id,
            locationId,
            cancellationToken);

        if (favoriteLocation is null)
            throw new NotFoundException("Favorite location was not found.");

        await _userFavoriteLocationRepository.RemoveAsync(favoriteLocation, cancellationToken);
    }

    private static GetLocationResponseDto MapToResponse(Location location)
    {
        return new GetLocationResponseDto
        {
            Id = location.Id,
            Name = location.Name,
            Latitude = location.Latitude,
            Longitude = location.Longitude,
            Altitude = location.Altitude,
        };
    }
}
