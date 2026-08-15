using MediatR;

namespace LedgerFlow.Application.Abstractions;

public interface IQuery<out TResponse> : IRequest<TResponse>
{
}
