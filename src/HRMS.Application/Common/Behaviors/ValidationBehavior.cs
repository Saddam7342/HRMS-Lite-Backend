using FluentValidation;
using MediatR;
using ValidationException = HRMS.Shared.Exceptions.ValidationException;

namespace HRMS.Application.Common.Behaviors;

/// <summary>
/// MediatR pipeline behavior that runs FluentValidation validators
/// before any command/query handler executes.
/// Throws ValidationException (maps to HTTP 422) on failure.
/// Validators are auto-discovered and injected by DI.
/// </summary>
public sealed class ValidationBehavior<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!validators.Any())
            return await next();

        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .Where(r => r.Errors.Count != 0)
            .SelectMany(r => r.Errors)
            .GroupBy(e => e.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.ErrorMessage).ToArray());

        if (failures.Count != 0)
            throw new ValidationException(failures);

        return await next();
    }
}
