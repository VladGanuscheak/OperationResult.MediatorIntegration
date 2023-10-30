using MediatR;

namespace OperationResult.MediatorIntegration.Contracts
{
    public interface IQuery<TResponse> : IRequest<OperationResult<TResponse>>
    {
    }
}
