using Microsoft.AspNetCore.Mvc;
using TrackManagement.Application.Artists;
using TrackManagement.Application.Common.Interfaces;

namespace TrackManagement.Api.Controllers;

[ApiController]
[Route("api/artists")]
[Produces("application/json")]
public class ArtistsController : ControllerBase
{
    private readonly IArtistService _artistService;

    public ArtistsController(IArtistService artistService)
    {
        _artistService = artistService;
    }

    /// <summary>List all artists.</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<ArtistDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ArtistDto>>> GetAll(CancellationToken cancellationToken) =>
        Ok(await _artistService.GetAllAsync(cancellationToken));

    /// <summary>Create an artist.</summary>
    [HttpPost]
    [ProducesResponseType<ArtistDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ArtistDto>> Create(CreateArtistRequest request, CancellationToken cancellationToken)
    {
        var artist = await _artistService.CreateAsync(request, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, artist);
    }

    /// <summary>Edit an artist's name, email and country.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType<ArtistDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ArtistDto>> Update(
        int id, UpdateArtistRequest request, CancellationToken cancellationToken) =>
        Ok(await _artistService.UpdateAsync(id, request, cancellationToken));
}
