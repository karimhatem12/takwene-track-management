using TrackManagement.Application.Audit;
using TrackManagement.Application.Common.Models;

namespace TrackManagement.Application.Common.Interfaces;

public interface IAuditLogService
{
    /// <summary>Changes to a track and to its DSP distributions, newest first.</summary>
    Task<IReadOnlyList<AuditLogDto>> GetTrackHistoryAsync(int trackId, CancellationToken cancellationToken);

    /// <summary>Search the whole audit trail, newest first, one page at a time.</summary>
    Task<PagedResult<AuditLogDto>> SearchAsync(AuditLogQuery query, CancellationToken cancellationToken);
}
