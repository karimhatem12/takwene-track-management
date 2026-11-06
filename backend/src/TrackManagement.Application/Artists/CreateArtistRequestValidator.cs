using FluentValidation;

namespace TrackManagement.Application.Artists;

public class CreateArtistRequestValidator : AbstractValidator<CreateArtistRequest>
{
    public CreateArtistRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200);

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email must be a valid email address.")
            .MaximumLength(256);

        RuleFor(x => x.Country)
            .NotEmpty().WithMessage("Country is required.")
            .Matches("^[A-Za-z]{2}$")
            .WithMessage("Country must be a 2-letter ISO 3166-1 code, e.g. 'EG' or 'SA'.");
    }
}
