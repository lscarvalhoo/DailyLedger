using MediatR;

namespace LedgerFlow.Application.Abstractions;

public interface ICommand<out TResponse> : IRequest<TResponse>
{
}
