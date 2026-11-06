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

    public ArtistService(IApplicationDbContext db, IValidator<CreateArtistRequest> createValidator)
    {
        _db = db;
        _createValidator = createValidator;
    }

    public async Task<IReadOnlyList<ArtistDto>> GetAllAsync(CancellationToken cancellationToken) =>
        await _db.Artists
            .AsNoTracking()
            .OrderBy(a => a.Name)
            .Select(a => new ArtistDto(a.Id, a.Name, a.Email, a.Country))
            .ToListAsync(cancellationToken);

    public async Task<ArtistDto> CreateAsync(CreateArtistRequest request, CancellationToken cancellationToken)
    {
        await _createValidator.ValidateAndThrowAsync(request, cancellationToken);

        var artist = Artist.Create(request.Name, request.Email, request.Country);

        // Friendly 409 for the common case; the unique index still protects against races.
        if (await _db.Artists.AnyAsync(a => a.Email == artist.Email, cancellationToken))
            throw new ConflictException($"An artist with email '{artist.Email}' already exists.");

        _db.Artists.Add(artist);
        await _db.SaveChangesAsync(cancellationToken);

        return new ArtistDto(artist.Id, artist.Name, artist.Email, artist.Country);
    }
}
