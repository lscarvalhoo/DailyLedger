using FluentValidation;
using LedgerFlow.Application.Telemetry;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LedgerFlow.Application.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> validators,
    ILogger<ValidationBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        using var activity = LedgerFlowTelemetry.ActivitySource.StartActivity(
            $"Validate {requestName}",
            System.Diagnostics.ActivityKind.Internal);

        if (!validators.Any())
        {
            activity?.SetTag("validation.validator_count", 0);
            logger.LogDebug("No validators registered for {RequestName}", requestName);
            return await next(cancellationToken);
        }

        var validatorList = validators.ToList();
        activity?.SetTag("validation.validator_count", validatorList.Count);
        logger.LogInformation(
            "Validating {RequestName} with {ValidatorCount} validator(s)",
            requestName,
            validatorList.Count);

        var context = new ValidationContext<TRequest>(request);

        var failures = (await Task.WhenAll(validatorList.Select(v => v.ValidateAsync(context, cancellationToken))))
            .SelectMany(result => result.Errors)
            .Where(failure => failure is not null)
            .ToList();

        if (failures.Count > 0)
        {
            activity?.SetTag("validation.failure_count", failures.Count);
            activity?.SetStatus(System.Diagnostics.ActivityStatusCode.Error, "Validation failed");
            logger.LogWarning(
                "Validation failed for {RequestName}: {ValidationErrors}",
                requestName,
                failures.Select(failure => new { failure.PropertyName, failure.ErrorMessage }));

            throw new ValidationException(failures);
        }

        activity?.SetStatus(System.Diagnostics.ActivityStatusCode.Ok);
        logger.LogInformation("Validation succeeded for {RequestName}", requestName);

        return await next(cancellationToken);
    }
}
