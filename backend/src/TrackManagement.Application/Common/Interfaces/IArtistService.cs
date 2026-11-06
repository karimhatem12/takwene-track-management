using TrackManagement.Application.Artists;

namespace TrackManagement.Application.Common.Interfaces;

public interface IArtistService
{
    Task<IReadOnlyList<ArtistDto>> GetAllAsync(CancellationToken cancellationToken);
    Task<ArtistDto> CreateAsync(CreateArtistRequest request, CancellationToken cancellationToken);
}
