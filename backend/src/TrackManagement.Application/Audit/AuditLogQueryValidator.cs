using FluentValidation;

namespace TrackManagement.Application.Audit;

public class AuditLogQueryValidator : AbstractValidator<AuditLogQuery>
{
    public const int MaxPageSize = 100;

    public AuditLogQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1).WithMessage("Page must be 1 or more.");
        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, MaxPageSize).WithMessage($"PageSize must be between 1 and {MaxPageSize}.");
        RuleFor(x => x.Action).IsInEnum().When(x => x.Action is not null);
    }
}
