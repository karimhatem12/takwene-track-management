using System.Text.Json;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TrackManagement.Application.Common.Exceptions;
using TrackManagement.Application.Common.Interfaces;
using TrackManagement.Application.Common.Models;
using TrackManagement.Domain.Entities;

namespace TrackManagement.Application.Audit;

public class AuditLogService : IAuditLogService
{
    private readonly IApplicationDbContext _db;
    private readonly IValidator<AuditLogQuery> _queryValidator;

    public AuditLogService(IApplicationDbContext db, IValidator<AuditLogQuery> queryValidator)
    {
        _db = db;
        _queryValidator = queryValidator;
    }

    public async Task<IReadOnlyList<AuditLogDto>> GetTrackHistoryAsync(int trackId, CancellationToken cancellationToken)
    {
        if (!await _db.Tracks.AnyAsync(t => t.Id == trackId, cancellationToken))
            throw new NotFoundException("Track", trackId);

        var trackKey = trackId.ToString();
        var distributionKeys = await _db.TrackDistributions
            .Where(d => d.TrackId == trackId)
            .Select(d => d.Id.ToString())
            .ToListAsync(cancellationToken);

        var logs = await _db.AuditLogs
            .AsNoTracking()
            .Where(a =>
                (a.EntityName == nameof(Track) && a.EntityId == trackKey) ||
                (a.EntityName == nameof(TrackDistribution) && distributionKeys.Contains(a.EntityId)))
            .OrderByDescending(a => a.Timestamp)
            .ThenByDescending(a => a.Id)
            .ToListAsync(cancellationToken);

        return logs.Select(ToDto).ToList();
    }

    public async Task<PagedResult<AuditLogDto>> SearchAsync(AuditLogQuery query, CancellationToken cancellationToken)
    {
        await _queryValidator.ValidateAndThrowAsync(query, cancellationToken);

        var logs = _db.AuditLogs.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.EntityName))
            logs = logs.Where(a => a.EntityName == query.EntityName.Trim());
        if (!string.IsNullOrWhiteSpace(query.EntityId))
            logs = logs.Where(a => a.EntityId == query.EntityId.Trim());
        if (query.UserId is not null)
            logs = logs.Where(a => a.UserId == query.UserId);
        if (query.Action is not null)
            logs = logs.Where(a => a.Action == query.Action);

        var total = await logs.CountAsync(cancellationToken);
        var page = await logs
            .OrderByDescending(a => a.Timestamp)
            .ThenByDescending(a => a.Id)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<AuditLogDto>(page.Select(ToDto).ToList(), query.Page, query.PageSize, total);
    }

    private static AuditLogDto ToDto(AuditLog log) =>
        new(
            log.Id,
            log.Timestamp,
            log.UserId,
            log.UserEmail,
            log.Action,
            log.EntityName,
            log.EntityId,
            // Return the stored JSON as a real JSON object, not as an escaped string.
            log.Changes is null ? null : JsonDocument.Parse(log.Changes).RootElement.Clone());
}
