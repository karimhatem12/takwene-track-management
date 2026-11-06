using TrackManagement.Application.Dsps;

namespace TrackManagement.Application.Common.Interfaces;

public interface IDspService
{
    Task<IReadOnlyList<DspDto>> GetAllAsync(CancellationToken cancellationToken);
}
