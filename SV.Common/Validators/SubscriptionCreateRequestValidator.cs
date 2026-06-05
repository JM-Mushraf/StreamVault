using FluentValidation;
using SV.Common.DTOs;

namespace SV.Common.Validators;

public class SubscriptionCreateRequestValidator : AbstractValidator<SubscriptionCreateRequest>
{
    public SubscriptionCreateRequestValidator()
    {
        RuleFor(x => x.PlanId).GreaterThan(0);
        RuleFor(x => x.StartDate).LessThan(x => x.EndDate).WithMessage("StartDate must be before EndDate");
    }
}
