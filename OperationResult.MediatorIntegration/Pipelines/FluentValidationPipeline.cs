using FluentValidation;
using MediatR;
using OperationResult.Extensions;
using System.Collections.Generic;
using System.Linq;
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
                if (typeof(TResponse).IsIn(typeof(OperationResult), typeof(OperationResult<>)))
                {
                    return (dynamic)OperationResultHelper.BadRequest(failures.Select(x => x.ErrorMessage).ToArray());
                }
            }

            return await next();
        }
    }
}
