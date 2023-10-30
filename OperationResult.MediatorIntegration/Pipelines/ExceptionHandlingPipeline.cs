﻿using MediatR;
using OperationResult.Extensions;
using System;
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
                if (typeof(TResponse).IsIn(typeof(OperationResult), typeof(OperationResult<>)))
                {
                    return (dynamic)OperationResultExtensions.BadRequest(exception.Message);
                }

                throw;
            }
        }
    }
}