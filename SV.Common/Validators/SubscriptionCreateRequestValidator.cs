using FluentValidation;
using SV.Common.DTOs.Subscription;

namespace SV.Common.Validators;

public class SubscriptionCreateRequestValidator : AbstractValidator<SubscriptionCreateRequest>
{
    public SubscriptionCreateRequestValidator()
    {
        RuleFor(x => x.PlanGuid).NotEmpty().WithMessage("PlanGuid is required");
        RuleFor(x => x.StartDate).LessThan(x => x.EndDate).WithMessage("StartDate must be before EndDate");
    }
}
