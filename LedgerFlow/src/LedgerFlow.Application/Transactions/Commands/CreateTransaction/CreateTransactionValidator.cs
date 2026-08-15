using FluentValidation;

namespace LedgerFlow.Application.Transactions.Commands.CreateTransaction;

public sealed class CreateTransactionValidator : AbstractValidator<CreateTransactionCommand>
{
    public CreateTransactionValidator()
    {
        RuleFor(command => command.MerchantId)
            .NotEmpty()
            .WithMessage("MerchantId must be provided.");

        RuleFor(command => command.Amount)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Amount cannot be negative.");

        RuleFor(command => command.Description)
            .NotEmpty()
            .WithMessage("Description must be provided.");

        RuleFor(command => command.OccurredAt)
            .NotEqual(default(DateTime))
            .WithMessage("OccurredAt must be provided.");

        RuleFor(command => command.Type)
            .IsInEnum()
            .WithMessage("Type must be a valid transaction type.");
    }
}
