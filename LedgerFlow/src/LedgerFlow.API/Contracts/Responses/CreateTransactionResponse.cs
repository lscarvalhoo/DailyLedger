namespace LedgerFlow.API.Contracts.Responses;

public sealed record CreateTransactionResponse(Guid Id)
{
    public static CreateTransactionResponse From(Guid id)
    {
        return new CreateTransactionResponse(id);
    }
}