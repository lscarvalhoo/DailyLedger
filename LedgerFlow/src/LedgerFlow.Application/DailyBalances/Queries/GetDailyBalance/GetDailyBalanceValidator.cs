using FluentValidation;

namespace LedgerFlow.Application.DailyBalances.Queries.GetDailyBalance;

public sealed class GetDailyBalanceValidator : AbstractValidator<GetDailyBalanceQuery>
{
    public GetDailyBalanceValidator()
    {
        RuleFor(query => query.MerchantId)
            .NotEmpty()
            .WithMessage("MerchantId must be provided.");

        RuleFor(query => query.Date)
            .NotEmpty()
            .WithMessage("Date must be provided.");
    }
}