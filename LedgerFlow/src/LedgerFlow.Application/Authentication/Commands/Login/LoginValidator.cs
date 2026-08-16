using FluentValidation;

namespace LedgerFlow.Application.Authentication.Commands.Login;

public sealed class LoginValidator : AbstractValidator<LoginCommand>
{
    public LoginValidator()
    {
        RuleFor(command => command.Email)
            .NotEmpty()
            .EmailAddress()
            .WithMessage("A valid email must be provided.");

        RuleFor(command => command.Password)
            .NotEmpty()
            .WithMessage("Password must be provided.");
    }
}