using FluentValidation;
using MediatR;
using OperationResult.Extensions;
using OperationResult.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace OperationResult.MediatorIntegration.Pipelines
{
    public class FluentValidationPipeline<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
        where TResponse : OperationResult
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;

        public FluentValidationPipeline(IEnumerable<IValidator<TRequest>> validators)
        {
            _validators = validators;
        }

        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
        {
            if (!_validators.Any()) return await next();
            var context = new ValidationContext<TRequest>(request);

            var validationResults = await Task.WhenAll(_validators.Select(v => v.ValidateAsync(context, cancellationToken)));
            var failures = validationResults.SelectMany(r => r.Errors)
                .Where(f => f != null)
                .ToList();

            if (failures.Any())
            {
                if (typeof(TResponse).IsIn(typeof(OperationResult)))
                {
                    return (dynamic)OperationResultHelper.BadRequest(failures.Select(x => x.ErrorMessage).ToArray());
                }

                if (typeof(TResponse).IsIn(typeof(OperationResult<>)))
                {
                    var responseType = typeof(TResponse);

                    var typeArguments = responseType.GetGenericArguments();
                    var closedGenericType = typeof(FailureOperationResult<>).MakeGenericType(typeArguments);

                    dynamic result = Activator.CreateInstance(closedGenericType);
                    return result.WithMessages(failures.Select(x => x.ErrorMessage).ToList());
                }
            }

            return await next();
        }
    }
}
