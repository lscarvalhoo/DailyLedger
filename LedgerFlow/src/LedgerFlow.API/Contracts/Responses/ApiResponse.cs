namespace LedgerFlow.API.Contracts.Responses;

public sealed record ApiResponse<T>(
    bool Success,
    T? Data,
    string? Message)
{
    public static ApiResponse<T> Ok(T data, string? message = null)
    {
        return new ApiResponse<T>(true, data, message);
    }

    public static ApiResponse<T> Failure(string message)
    {
        return new ApiResponse<T>(false, default, message);
    }
}