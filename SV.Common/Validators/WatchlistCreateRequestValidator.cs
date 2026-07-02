using FluentValidation;
using SV.Common.DTOs;

namespace SV.Common.Validators;

public class WatchlistCreateRequestValidator : AbstractValidator<WatchlistCreateRequest>
{
    public WatchlistCreateRequestValidator()
    {
        RuleFor(x => x.MovieGuid).NotEmpty();
    }
}
