using TrackManagement.Application.Tracks;

namespace TrackManagement.Application.Common.Interfaces;

public interface ITrackService
{
    Task<IReadOnlyList<TrackDto>> GetAllAsync(TrackFilter filter, CancellationToken cancellationToken);
    Task<TrackDetailsDto> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<TrackDetailsDto> CreateAsync(CreateTrackRequest request, CancellationToken cancellationToken);
    Task<TrackDetailsDto> UpdateAsync(int id, UpdateTrackRequest request, CancellationToken cancellationToken);
    Task<TrackDetailsDto> DistributeAsync(int id, DistributeTrackRequest request, CancellationToken cancellationToken);
    Task<TrackDetailsDto> UpdateStatusAsync(int id, UpdateTrackStatusRequest request, CancellationToken cancellationToken);
}
