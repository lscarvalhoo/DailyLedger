using FluentValidation;

namespace LedgerFlow.Application.Transactions.Queries.GetTransaction;

public sealed class GetTransactionValidator : AbstractValidator<GetTransactionQuery>
{
    public GetTransactionValidator()
    {
        RuleFor(query => query.Id)
            .NotEmpty()
            .WithMessage("Transaction id must be provided.");
    }
}