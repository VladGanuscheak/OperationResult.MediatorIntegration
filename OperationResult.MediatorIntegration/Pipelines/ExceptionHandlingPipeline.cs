using MediatR;
using OperationResult.Extensions;
using OperationResult.Results;
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace OperationResult.MediatorIntegration.Pipelines
{
    public class ExceptionHandlingPipeline<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> 
        where TRequest : IRequest<TResponse>
        where TResponse : OperationResult
    {
        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
        {
            try
            {
                var response = await next();
                return response;
            }
            catch (Exception exception)
            {
                if (typeof(TResponse).IsIn(typeof(OperationResult)))
                {
                    return (dynamic)OperationResultExtensions.BadRequest(exception.Message);
                }

                if (typeof(TResponse).IsIn(typeof(OperationResult<>)))
                {
                    var responseType = typeof(TResponse);

                    var typeArguments = responseType.GetGenericArguments();
                    var closedGenericType = typeof(FailureOperationResult<>).MakeGenericType(typeArguments);

                    dynamic result = Activator.CreateInstance(closedGenericType);
                    return result.WithMessage(exception.Message);
                }

                throw;
            }
        }
    }
}
