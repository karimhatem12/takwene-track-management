using Microsoft.EntityFrameworkCore;
using TrackManagement.Application.Common.Interfaces;

namespace TrackManagement.Application.Dsps;

public class DspService : IDspService
{
    private readonly IApplicationDbContext _db;

    public DspService(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<DspDto>> GetAllAsync(CancellationToken cancellationToken) =>
        await _db.Dsps
            .AsNoTracking()
            .OrderBy(d => d.Name)
            .Select(d => new DspDto(d.Id, d.Name))
            .ToListAsync(cancellationToken);
}
