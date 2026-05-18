using WeatherAPI.Application.Common;
using WeatherAPI.Application.Dtos;
using WeatherAPI.Application.Interfaces;
using WeatherAPI.Domain.Entities;
using WeatherAPI.Domain.Enums;

namespace WeatherAPI.Application.Service;

public class AdminLocationService : IAdminLocationService
{
    private readonly IAuthService _authService;
    private readonly ILocationRepository _locationRepository;
    private readonly IUserFavoriteLocationRepository _userFavoriteLocationRepository;

    public AdminLocationService(
        IAuthService authService,
        ILocationRepository locationRepository,
        IUserFavoriteLocationRepository userFavoriteLocationRepository)
    {
        _authService = authService;
        _locationRepository = locationRepository;
        _userFavoriteLocationRepository = userFavoriteLocationRepository;
    }

    public async Task<GetLocationResponseDto> CreateLocationAsync(
        string sessionToken,
        CreateLocationRequestDto request,
        CancellationToken cancellationToken = default)
    {
        await EnsureAdminAsync(sessionToken, cancellationToken);

        var locationName = request.Name.Trim();

        if (string.IsNullOrWhiteSpace(locationName))
        {
            throw new BadRequestException("Location name is required.");
        }

        var existingLocation = await _locationRepository.GetLocationAsync(
            request.Latitude,
            request.Longitude,
            request.Altitude,
            cancellationToken);

        if (existingLocation is not null)
        {
            throw new ConflictException("Location with the same coordinates and altitude already exists.");
        }

        var location = Location.Create(request.Latitude, request.Longitude, request.Altitude);
        location.Rename(locationName);

        await _locationRepository.AddAsync(location, cancellationToken);
        await _locationRepository.SaveChangesAsync(cancellationToken);

        return new GetLocationResponseDto
        {
            Id = location.Id,
            Name = location.Name,
            Latitude = location.Latitude,
            Longitude = location.Longitude,
            Altitude = location.Altitude
        };
    }

    public async Task DeleteLocationAsync(
        string sessionToken,
        short locationId,
        CancellationToken cancellationToken = default)
    {
        await EnsureAdminAsync(sessionToken, cancellationToken);

        var wasDeleted = await _locationRepository.DeleteAsync(locationId, cancellationToken);

        if (!wasDeleted)
        {
            throw new NotFoundException($"Location with ID {locationId} was not found.");
        }

        await _userFavoriteLocationRepository.RemoveByLocationIdAsync(locationId, cancellationToken);
    }

    private async Task EnsureAdminAsync(string sessionToken, CancellationToken cancellationToken)
    {
        var user = await _authService.GetCurrentUserAsync(sessionToken, cancellationToken);

        if (user.Role != UserRole.Admin)
        {
            throw new ForbiddenException("Only admin users can manage global locations.");
        }
    }
}
