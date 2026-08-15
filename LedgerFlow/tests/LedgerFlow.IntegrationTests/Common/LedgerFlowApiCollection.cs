namespace LedgerFlow.IntegrationTests.Common;

[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class LedgerFlowApiCollection : ICollectionFixture<LedgerFlowApiFactory>
{
    public const string Name = "LedgerFlow API";
}