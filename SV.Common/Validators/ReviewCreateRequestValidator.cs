using FluentValidation;
using SV.Common.DTOs;

namespace SV.Common.Validators;

public class ReviewCreateRequestValidator : AbstractValidator<ReviewCreateRequest>
{
    public ReviewCreateRequestValidator()
    {
        RuleFor(x => x.MovieId).GreaterThan(0);
        RuleFor(x => x.Rating).InclusiveBetween(0, 10);
    }
}
