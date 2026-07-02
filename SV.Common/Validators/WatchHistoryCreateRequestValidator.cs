using FluentValidation;
using SV.Common.DTOs;

namespace SV.Common.Validators;

public class WatchHistoryCreateRequestValidator : AbstractValidator<WatchHistoryCreateRequest>
{
    public WatchHistoryCreateRequestValidator()
    {
        RuleFor(x => x.UserGuid).NotEmpty();
        RuleFor(x => x.MovieGuid).NotEmpty();
        RuleFor(x => x.WatchMinutes).GreaterThanOrEqualTo(0);
        RuleFor(x => x.DeviceType).NotEmpty();
    }
}
