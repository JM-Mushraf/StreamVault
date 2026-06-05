using FluentValidation;
using SV.Common.DTOs;

namespace SV.Common.Validators;

public class PaymentCreateRequestValidator : AbstractValidator<PaymentCreateRequest>
{
    public PaymentCreateRequestValidator()
    {
        RuleFor(x => x.SubscriptionId).GreaterThan(0);
        RuleFor(x => x.Amount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.PaymentMethod).NotEmpty();
    }
}
