namespace TrackManagement.Domain.Enums;

/// <summary>
/// Lifecycle of a track. Moves forward only: Draft -> Submitted -> Distributed.
/// </summary>
public enum TrackStatus
{
    Draft = 0,
    Submitted = 1,
    Distributed = 2
}
