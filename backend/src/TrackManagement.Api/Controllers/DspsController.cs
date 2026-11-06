using Microsoft.AspNetCore.Mvc;
using TrackManagement.Application.Common.Interfaces;
using TrackManagement.Application.Dsps;

namespace TrackManagement.Api.Controllers;

[ApiController]
[Route("api/dsps")]
[Produces("application/json")]
public class DspsController : ControllerBase
{
    private readonly IDspService _dspService;

    public DspsController(IDspService dspService)
    {
        _dspService = dspService;
    }

    /// <summary>List the DSPs a track can be distributed to.</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<DspDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<DspDto>>> GetAll(CancellationToken cancellationToken) =>
        Ok(await _dspService.GetAllAsync(cancellationToken));
}
