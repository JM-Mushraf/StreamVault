using FluentValidation;
using SV.Common.DTOs;

namespace SV.Common.Validators;

public class WatchHistoryCreateRequestValidator : AbstractValidator<WatchHistoryCreateRequest>
{
    public WatchHistoryCreateRequestValidator()
    {
        RuleFor(x => x.MovieId).GreaterThan(0);
        RuleFor(x => x.WatchMinutes).GreaterThanOrEqualTo(0).LessThanOrEqualTo(1000);
    }
}
