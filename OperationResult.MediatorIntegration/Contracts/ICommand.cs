using MediatR;

namespace OperationResult.MediatorIntegration.Contracts
{
    public interface ICommand<TResponse> : IRequest<OperationResult<TResponse>>
    {
    }

    public interface ICommand : IRequest<OperationResult>
    {
    }
}
