using Microsoft.AspNetCore.Mvc;
using TrackManagement.Application.Audit;
using TrackManagement.Application.Common.Interfaces;
using TrackManagement.Application.Tracks;

namespace TrackManagement.Api.Controllers;

[ApiController]
[Route("api/tracks")]
[Produces("application/json")]
public class TracksController : ControllerBase
{
    private readonly ITrackService _trackService;
    private readonly IAuditLogService _auditLogService;

    public TracksController(ITrackService trackService, IAuditLogService auditLogService)
    {
        _trackService = trackService;
        _auditLogService = auditLogService;
    }

    /// <summary>List tracks, optionally filtered by artist, genre and/or status.</summary>
    /// <remarks>Example: GET /api/tracks?artistId=1&amp;genre=Rai&amp;status=Distributed</remarks>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<TrackDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<TrackDto>>> GetAll(
        [FromQuery] TrackFilter filter, CancellationToken cancellationToken) =>
        Ok(await _trackService.GetAllAsync(filter, cancellationToken));

    /// <summary>Get a track with its DSP distribution statuses.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType<TrackDetailsDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TrackDetailsDto>> GetById(int id, CancellationToken cancellationToken) =>
        Ok(await _trackService.GetByIdAsync(id, cancellationToken));

    /// <summary>Who changed this track and its DSP distributions, and what changed. Newest first.</summary>
    [HttpGet("{id:int}/history")]
    [ProducesResponseType<IReadOnlyList<AuditLogDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<AuditLogDto>>> GetHistory(int id, CancellationToken cancellationToken) =>
        Ok(await _auditLogService.GetTrackHistoryAsync(id, cancellationToken));

    /// <summary>Create a track for an artist. New tracks start as Draft.</summary>
    [HttpPost]
    [ProducesResponseType<TrackDetailsDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TrackDetailsDto>> Create(CreateTrackRequest request, CancellationToken cancellationToken)
    {
        var track = await _trackService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = track.Id }, track);
    }

    /// <summary>Submit a track to one or more DSPs. Each DSP can receive a track only once.</summary>
    [HttpPost("{id:int}/distribute")]
    [ProducesResponseType<TrackDetailsDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TrackDetailsDto>> Distribute(
        int id, DistributeTrackRequest request, CancellationToken cancellationToken) =>
        Ok(await _trackService.DistributeAsync(id, request, cancellationToken));

    /// <summary>Move a track's status one step forward (Draft → Submitted → Distributed).</summary>
    [HttpPatch("{id:int}/status")]
    [ProducesResponseType<TrackDetailsDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TrackDetailsDto>> UpdateStatus(
        int id, UpdateTrackStatusRequest request, CancellationToken cancellationToken) =>
        Ok(await _trackService.UpdateStatusAsync(id, request, cancellationToken));
}
