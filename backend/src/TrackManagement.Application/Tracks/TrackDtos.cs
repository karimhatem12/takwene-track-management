using TrackManagement.Domain.Enums;

namespace TrackManagement.Application.Tracks;

/// <summary>Row in the track list.</summary>
public record TrackDto(
    int Id,
    string Title,
    int ArtistId,
    string ArtistName,
    string Isrc,
    DateOnly ReleaseDate,
    string Genre,
    TrackStatus Status);

/// <summary>Full track info including where it has been distributed.</summary>
public record TrackDetailsDto(
    int Id,
    string Title,
    int ArtistId,
    string ArtistName,
    string Isrc,
    DateOnly ReleaseDate,
    string Genre,
    TrackStatus Status,
    IReadOnlyList<TrackDistributionDto> Distributions);

public record TrackDistributionDto(
    int Id,
    int DspId,
    string DspName,
    DateTime SubmittedAt,
    DistributionStatus Status);

/// <summary>Optional filters for GET /api/tracks.</summary>
public record TrackFilter(int? ArtistId, string? Genre, TrackStatus? Status);

public record CreateTrackRequest(
    int ArtistId,
    string Title,
    string Isrc,
    DateOnly? ReleaseDate,
    string Genre);

public record DistributeTrackRequest(IReadOnlyList<int> DspIds);

public record UpdateTrackStatusRequest(TrackStatus? Status);
