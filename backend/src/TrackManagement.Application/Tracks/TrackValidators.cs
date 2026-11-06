using System.Text.RegularExpressions;
using FluentValidation;
using TrackManagement.Domain.Entities;

namespace TrackManagement.Application.Tracks;

public static partial class IsrcFormat
{
    /// <summary>
    /// ISRC = 2-letter country + 3 alphanumeric registrant + 2-digit year + 5-digit designation,
    /// e.g. "EG-TKW-24-00001". Hyphens/spaces are optional; checked after normalization.
    /// </summary>
    [GeneratedRegex("^[A-Z]{2}[A-Z0-9]{3}[0-9]{7}$")]
    private static partial Regex Pattern();

    public static bool IsValid(string? isrc) =>
        !string.IsNullOrWhiteSpace(isrc) && Pattern().IsMatch(Track.NormalizeIsrc(isrc));
}

public class CreateTrackRequestValidator : AbstractValidator<CreateTrackRequest>
{
    private static readonly DateOnly EarliestReleaseDate = new(1900, 1, 1);

    public CreateTrackRequestValidator()
    {
        RuleFor(x => x.ArtistId)
            .GreaterThan(0).WithMessage("ArtistId is required.");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200);

        RuleFor(x => x.Isrc)
            .NotEmpty().WithMessage("ISRC is required.")
            .Must(IsrcFormat.IsValid)
            .WithMessage("ISRC must look like 'CC-XXX-YY-NNNNN' (e.g. 'EG-TKW-24-00001').");

        RuleFor(x => x.ReleaseDate)
            .NotNull().WithMessage("ReleaseDate is required.")
            .GreaterThanOrEqualTo(EarliestReleaseDate)
            .WithMessage("ReleaseDate must be on or after 1900-01-01.");

        RuleFor(x => x.Genre)
            .NotEmpty().WithMessage("Genre is required.")
            .MaximumLength(50);
    }
}

public class DistributeTrackRequestValidator : AbstractValidator<DistributeTrackRequest>
{
    public DistributeTrackRequestValidator()
    {
        RuleFor(x => x.DspIds)
            .NotEmpty().WithMessage("At least one DSP id is required.");

        RuleForEach(x => x.DspIds)
            .GreaterThan(0).WithMessage("DSP ids must be positive numbers.");
    }
}

public class UpdateTrackStatusRequestValidator : AbstractValidator<UpdateTrackStatusRequest>
{
    public UpdateTrackStatusRequestValidator()
    {
        RuleFor(x => x.Status)
            .NotNull().WithMessage("Status is required (Draft, Submitted or Distributed).")
            .IsInEnum().WithMessage("Status must be Draft, Submitted or Distributed.");
    }
}
