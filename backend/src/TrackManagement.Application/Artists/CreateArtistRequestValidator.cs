using FluentValidation;

namespace TrackManagement.Application.Artists;

/// <summary>Field rules shared by create and update, so the two can never drift apart.</summary>
internal static class ArtistRules
{
    public static IRuleBuilderOptions<T, string> ArtistName<T>(this IRuleBuilder<T, string> rule) =>
        rule.NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200);

    public static IRuleBuilderOptions<T, string> ArtistEmail<T>(this IRuleBuilder<T, string> rule) =>
        rule.NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email must be a valid email address.")
            .MaximumLength(256);

    public static IRuleBuilderOptions<T, string> CountryCode<T>(this IRuleBuilder<T, string> rule) =>
        rule.NotEmpty().WithMessage("Country is required.")
            .Matches("^[A-Za-z]{2}$")
            .WithMessage("Country must be a 2-letter ISO 3166-1 code, e.g. 'EG' or 'SA'.");
}

public class CreateArtistRequestValidator : AbstractValidator<CreateArtistRequest>
{
    public CreateArtistRequestValidator()
    {
        // One clear message per field even without the global setting in AddApplication (e.g. in tests).
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Name).ArtistName();
        RuleFor(x => x.Email).ArtistEmail();
        RuleFor(x => x.Country).CountryCode();
    }
}

public class UpdateArtistRequestValidator : AbstractValidator<UpdateArtistRequest>
{
    public UpdateArtistRequestValidator()
    {
        // One clear message per field even without the global setting in AddApplication (e.g. in tests).
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Name).ArtistName();
        RuleFor(x => x.Email).ArtistEmail();
        RuleFor(x => x.Country).CountryCode();
    }
}
