using Microsoft.AspNetCore.Mvc;
using TrackManagement.Domain.Common;

namespace TrackManagement.Api.Controllers;

[ApiController]
[Route("api/genres")]
[Produces("application/json")]
public class GenresController : ControllerBase
{
    /// <summary>The genres a track can have. Any other value is rejected with 400.</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<string>>(StatusCodes.Status200OK)]
    public ActionResult<IReadOnlyList<string>> GetAll() => Ok(Genres.All);
}
