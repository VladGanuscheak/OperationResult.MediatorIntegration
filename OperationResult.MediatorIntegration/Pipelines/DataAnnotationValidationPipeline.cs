using MediatR;
using OperationResult.Extensions;
using OperationResult.Results;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OperationResult.MediatorIntegration.Pipelines
{
    public class DataAnnotationValidationPipeline<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
        where TResponse : OperationResult
    {
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            var validationContext = new ValidationContext(request);
            var validationResults = new List<ValidationResult>();

            bool isValid = Validator.TryValidateObject(request, validationContext, validationResults, validateAllProperties: true);

            if (!isValid) 
            {
                if (typeof(TResponse).IsIn(typeof(OperationResult)))
                {
                    return (dynamic)OperationResultHelper.BadRequest(validationResults.Select(result => result.ErrorMessage).ToArray());
                }

                if (typeof(TResponse).IsIn(typeof(OperationResult<>)))
                {
                    var responseType = typeof(TResponse);

                    var typeArguments = responseType.GetGenericArguments();
                    var closedGenericType = typeof(FailureOperationResult<>).MakeGenericType(typeArguments);

                    dynamic result = Activator.CreateInstance(closedGenericType);
                    return result.WithMessages(validationResults.Select(result => result.ErrorMessage).ToList());
                }
            }

            return await next();
        }
    }
}
