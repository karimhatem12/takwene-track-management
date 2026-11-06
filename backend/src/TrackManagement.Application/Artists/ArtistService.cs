using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TrackManagement.Application.Common.Exceptions;
using TrackManagement.Application.Common.Interfaces;
using TrackManagement.Domain.Entities;

namespace TrackManagement.Application.Artists;

public class ArtistService : IArtistService
{
    private readonly IApplicationDbContext _db;
    private readonly IValidator<CreateArtistRequest> _createValidator;
    private readonly IValidator<UpdateArtistRequest> _updateValidator;

    public ArtistService(
        IApplicationDbContext db,
        IValidator<CreateArtistRequest> createValidator,
        IValidator<UpdateArtistRequest> updateValidator)
    {
        _db = db;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<IReadOnlyList<ArtistDto>> GetAllAsync(CancellationToken cancellationToken) =>
        await _db.Artists
            .AsNoTracking()
            .OrderBy(a => a.Name)
            .Select(a => new ArtistDto(a.Id, a.Name, a.Email, a.Country, a.Tracks.Count))
            .ToListAsync(cancellationToken);

    public async Task<ArtistDto> CreateAsync(CreateArtistRequest request, CancellationToken cancellationToken)
    {
        await _createValidator.ValidateAndThrowAsync(request, cancellationToken);

        var artist = Artist.Create(request.Name, request.Email, request.Country);
        await EnsureEmailIsFreeAsync(artist.Email, exceptArtistId: null, cancellationToken);

        _db.Artists.Add(artist);
        await _db.SaveChangesAsync(cancellationToken);

        return new ArtistDto(artist.Id, artist.Name, artist.Email, artist.Country, TrackCount: 0);
    }

    public async Task<ArtistDto> UpdateAsync(int id, UpdateArtistRequest request, CancellationToken cancellationToken)
    {
        await _updateValidator.ValidateAndThrowAsync(request, cancellationToken);

        var artist = await _db.Artists.FirstOrDefaultAsync(a => a.Id == id, cancellationToken)
            ?? throw new NotFoundException("Artist", id);

        await EnsureEmailIsFreeAsync(Artist.NormalizeEmail(request.Email), exceptArtistId: id, cancellationToken);

        artist.Update(request.Name, request.Email, request.Country);
        await _db.SaveChangesAsync(cancellationToken);

        var trackCount = await _db.Tracks.CountAsync(t => t.ArtistId == id, cancellationToken);
        return new ArtistDto(artist.Id, artist.Name, artist.Email, artist.Country, trackCount);
    }

    // Friendly 409 for the common case; the unique index still protects against races.
    private async Task EnsureEmailIsFreeAsync(string email, int? exceptArtistId, CancellationToken cancellationToken)
    {
        if (await _db.Artists.AnyAsync(a => a.Email == email && a.Id != exceptArtistId, cancellationToken))
            throw new ConflictException($"An artist with email '{email}' already exists.");
    }
}
