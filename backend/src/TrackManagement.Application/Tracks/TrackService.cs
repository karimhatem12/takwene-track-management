using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TrackManagement.Application.Common.Exceptions;
using TrackManagement.Application.Common.Interfaces;
using TrackManagement.Application.Common.Validation;
using TrackManagement.Domain.Entities;

namespace TrackManagement.Application.Tracks;

public class TrackService : ITrackService
{
    private readonly IApplicationDbContext _db;
    private readonly TimeProvider _timeProvider;
    private readonly IValidator<CreateTrackRequest> _createValidator;
    private readonly IValidator<DistributeTrackRequest> _distributeValidator;
    private readonly IValidator<UpdateTrackStatusRequest> _statusValidator;

    public TrackService(
        IApplicationDbContext db,
        TimeProvider timeProvider,
        IValidator<CreateTrackRequest> createValidator,
        IValidator<DistributeTrackRequest> distributeValidator,
        IValidator<UpdateTrackStatusRequest> statusValidator)
    {
        _db = db;
        _timeProvider = timeProvider;
        _createValidator = createValidator;
        _distributeValidator = distributeValidator;
        _statusValidator = statusValidator;
    }

    public async Task<IReadOnlyList<TrackDto>> GetAllAsync(TrackFilter filter, CancellationToken cancellationToken)
    {
        var query = _db.Tracks.AsNoTracking();

        if (filter.ArtistId is not null)
            query = query.Where(t => t.ArtistId == filter.ArtistId);

        // SQL Server's default collation is case-insensitive, so "rai" matches "Rai".
        if (!string.IsNullOrWhiteSpace(filter.Genre))
        {
            var genre = filter.Genre.Trim();
            query = query.Where(t => t.Genre == genre);
        }

        if (filter.Status is not null)
            query = query.Where(t => t.Status == filter.Status);

        return await query
            .OrderByDescending(t => t.ReleaseDate)
            .ThenBy(t => t.Title)
            .Select(t => new TrackDto(
                t.Id, t.Title, t.ArtistId, t.Artist.Name, t.Isrc, t.ReleaseDate, t.Genre, t.Status))
            .ToListAsync(cancellationToken);
    }

    public async Task<TrackDetailsDto> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var track = await _db.Tracks
            .AsNoTracking()
            .Include(t => t.Artist)
            .Include(t => t.Distributions).ThenInclude(d => d.Dsp)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken)
            ?? throw new NotFoundException("Track", id);

        return ToDetails(track);
    }

    public async Task<TrackDetailsDto> CreateAsync(CreateTrackRequest request, CancellationToken cancellationToken)
    {
        await _createValidator.ValidateAndThrowAsync(request, cancellationToken);

        if (!await _db.Artists.AnyAsync(a => a.Id == request.ArtistId, cancellationToken))
            throw ValidationExtensions.FieldError(
                nameof(request.ArtistId), $"Artist with id '{request.ArtistId}' does not exist.");

        var track = Track.Create(
            request.ArtistId, request.Title, request.Isrc, request.ReleaseDate!.Value, request.Genre);

        // Friendly 409 for the common case; the unique index still protects against races.
        if (await _db.Tracks.AnyAsync(t => t.Isrc == track.Isrc, cancellationToken))
            throw new ConflictException($"A track with ISRC '{track.Isrc}' already exists.");

        _db.Tracks.Add(track);
        await _db.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(track.Id, cancellationToken);
    }

    public async Task<TrackDetailsDto> DistributeAsync(
        int id, DistributeTrackRequest request, CancellationToken cancellationToken)
    {
        await _distributeValidator.ValidateAndThrowAsync(request, cancellationToken);

        // Distributions must be loaded so the domain can reject DSPs the track was already sent to.
        var track = await _db.Tracks
            .Include(t => t.Distributions)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken)
            ?? throw new NotFoundException("Track", id);

        var requestedIds = request.DspIds.Distinct().ToList();
        var dsps = await _db.Dsps
            .Where(d => requestedIds.Contains(d.Id))
            .ToListAsync(cancellationToken);

        var unknownIds = requestedIds.Except(dsps.Select(d => d.Id)).ToList();
        if (unknownIds.Count > 0)
            throw ValidationExtensions.FieldError(
                nameof(request.DspIds), $"Unknown DSP id(s): {string.Join(", ", unknownIds)}.");

        track.Distribute(dsps, _timeProvider.GetUtcNow().UtcDateTime);
        await _db.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task<TrackDetailsDto> UpdateStatusAsync(
        int id, UpdateTrackStatusRequest request, CancellationToken cancellationToken)
    {
        await _statusValidator.ValidateAndThrowAsync(request, cancellationToken);

        // Distributions must be loaded: moving to Distributed requires a Live distribution.
        var track = await _db.Tracks
            .Include(t => t.Distributions)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken)
            ?? throw new NotFoundException("Track", id);

        track.ChangeStatus(request.Status!.Value);
        await _db.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(id, cancellationToken);
    }

    private static TrackDetailsDto ToDetails(Track track) =>
        new(
            track.Id,
            track.Title,
            track.ArtistId,
            track.Artist.Name,
            track.Isrc,
            track.ReleaseDate,
            track.Genre,
            track.Status,
            track.Distributions
                .OrderBy(d => d.Dsp.Name)
                .Select(d => new TrackDistributionDto(d.Id, d.DspId, d.Dsp.Name, d.SubmittedAt, d.Status))
                .ToList(),
            track.CreatedAt,
            track.CreatedBy,
            track.UpdatedAt,
            track.UpdatedBy);
}
