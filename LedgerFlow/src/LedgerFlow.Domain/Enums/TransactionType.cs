using System.ComponentModel;

namespace LedgerFlow.Domain.Enums;

public enum TransactionType
{
    [Description("Credit Transaction")]
    Credit,
    [Description("Debit Transaction")]
    Debit
}
