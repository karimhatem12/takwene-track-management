using System.Text.RegularExpressions;
using FluentValidation;
using TrackManagement.Domain.Common;
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

/// <summary>Field rules shared by create and update, so the two can never drift apart.</summary>
internal static class TrackRules
{
    private static readonly DateOnly EarliestReleaseDate = new(1900, 1, 1);

    public static IRuleBuilderOptions<T, int> ArtistIdRule<T>(this IRuleBuilder<T, int> rule) =>
        rule.GreaterThan(0).WithMessage("ArtistId is required.");

    public static IRuleBuilderOptions<T, string> TitleRule<T>(this IRuleBuilder<T, string> rule) =>
        rule.NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200);

    public static IRuleBuilderOptions<T, string> IsrcRule<T>(this IRuleBuilder<T, string> rule) =>
        rule.NotEmpty().WithMessage("ISRC is required.")
            .Must(IsrcFormat.IsValid)
            .WithMessage("ISRC must look like 'CC-XXX-YY-NNNNN' (e.g. 'EG-TKW-24-00001').");

    public static IRuleBuilderOptions<T, DateOnly?> ReleaseDateRule<T>(this IRuleBuilder<T, DateOnly?> rule) =>
        rule.NotNull().WithMessage("ReleaseDate is required.")
            .GreaterThanOrEqualTo(EarliestReleaseDate)
            .WithMessage("ReleaseDate must be on or after 1900-01-01.");

    public static IRuleBuilderOptions<T, string> GenreRule<T>(this IRuleBuilder<T, string> rule) =>
        rule.NotEmpty().WithMessage("Genre is required.")
            .Must(Genres.IsKnown)
            .WithMessage($"Genre must be one of: {string.Join(", ", Genres.All)}.");
}

public class CreateTrackRequestValidator : AbstractValidator<CreateTrackRequest>
{
    public CreateTrackRequestValidator()
    {
        // One clear message per field even without the global setting in AddApplication (e.g. in tests).
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.ArtistId).ArtistIdRule();
        RuleFor(x => x.Title).TitleRule();
        RuleFor(x => x.Isrc).IsrcRule();
        RuleFor(x => x.ReleaseDate).ReleaseDateRule();
        RuleFor(x => x.Genre).GenreRule();
    }
}

public class UpdateTrackRequestValidator : AbstractValidator<UpdateTrackRequest>
{
    public UpdateTrackRequestValidator()
    {
        // One clear message per field even without the global setting in AddApplication (e.g. in tests).
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.ArtistId).ArtistIdRule();
        RuleFor(x => x.Title).TitleRule();
        RuleFor(x => x.Isrc).IsrcRule();
        RuleFor(x => x.ReleaseDate).ReleaseDateRule();
        RuleFor(x => x.Genre).GenreRule();
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
