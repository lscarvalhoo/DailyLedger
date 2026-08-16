using LedgerFlow.API.Contracts.Responses;
using LedgerFlow.Application.DTOs;
using LedgerFlow.Domain.Enums;

namespace LedgerFlow.UnitTests.API.Contracts.Responses;

public sealed class ApiResponseTests
{
    [Fact]
    public void Ok_ShouldWrapData()
    {
        var payload = CreateTransactionResponse.From(Guid.NewGuid());

        var response = ApiResponse<CreateTransactionResponse>.Ok(payload);

        Assert.True(response.Success);
        Assert.Same(payload, response.Data);
        Assert.Null(response.Message);
    }

    [Fact]
    public void Failure_ShouldContainMessageWithoutData()
    {
        var response = ApiResponse<TransactionResponse>.Failure("Transaction not found.");

        Assert.False(response.Success);
        Assert.Null(response.Data);
        Assert.Equal("Transaction not found.", response.Message);
    }

    [Fact]
    public void TransactionResponse_From_ShouldMapAllProperties()
    {
        var dto = new TransactionDto(
            Guid.NewGuid(),
            Guid.NewGuid(),
            TransactionType.Credit,
            150,
            DateTime.UtcNow,
            "Sale #123",
            DateTime.UtcNow);

        var response = TransactionResponse.From(dto);

        Assert.Equal(dto.Id, response.Id);
        Assert.Equal(dto.MerchantId, response.MerchantId);
        Assert.Equal(dto.Type, response.Type);
        Assert.Equal(dto.Amount, response.Amount);
        Assert.Equal(dto.OccurredAt, response.OccurredAt);
        Assert.Equal(dto.Description, response.Description);
        Assert.Equal(dto.CreatedAt, response.CreatedAt);
    }

    [Fact]
    public void DailyBalanceResponse_From_ShouldMapAllProperties()
    {
        var dto = new DailyBalanceDto(
            Guid.NewGuid(),
            Guid.NewGuid(),
            new DateOnly(2026, 8, 15),
            200,
            50,
            150,
            DateTime.UtcNow);

        var response = DailyBalanceResponse.From(dto);

        Assert.Equal(dto.Id, response.Id);
        Assert.Equal(dto.MerchantId, response.MerchantId);
        Assert.Equal(dto.Date, response.Date);
        Assert.Equal(dto.TotalCredits, response.TotalCredits);
        Assert.Equal(dto.TotalDebits, response.TotalDebits);
        Assert.Equal(dto.Balance, response.Balance);
        Assert.Equal(dto.UpdatedAt, response.UpdatedAt);
    }
}