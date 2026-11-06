using Microsoft.AspNetCore.Mvc;
using TrackManagement.Application.Audit;
using TrackManagement.Application.Common.Interfaces;
using TrackManagement.Application.Common.Models;

namespace TrackManagement.Api.Controllers;

/// <summary>Read-only access to the audit trail. There are deliberately no write endpoints.</summary>
[ApiController]
[Route("api/audit-logs")]
[Produces("application/json")]
public class AuditLogsController : ControllerBase
{
    private readonly IAuditLogService _auditLogService;

    public AuditLogsController(IAuditLogService auditLogService)
    {
        _auditLogService = auditLogService;
    }

    /// <summary>Search the audit trail, newest first.</summary>
    /// <remarks>Example: GET /api/audit-logs?entityName=Track&amp;entityId=3&amp;page=1&amp;pageSize=20</remarks>
    [HttpGet]
    [ProducesResponseType<PagedResult<AuditLogDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResult<AuditLogDto>>> Search(
        [FromQuery] AuditLogQuery query, CancellationToken cancellationToken) =>
        Ok(await _auditLogService.SearchAsync(query, cancellationToken));
}
