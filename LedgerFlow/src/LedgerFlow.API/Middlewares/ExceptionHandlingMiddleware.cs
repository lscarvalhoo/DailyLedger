using FluentValidation;
using LedgerFlow.Application.Exceptions;
using LedgerFlow.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace LedgerFlow.API.Middlewares;

public sealed class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (ValidationException exception)
        {
            Activity.Current?.SetStatus(ActivityStatusCode.Error, "Validation failed");
            var errors = exception.Errors
                .GroupBy(error => error.PropertyName)
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(error => error.ErrorMessage).Distinct().ToArray());

            var problem = new HttpValidationProblemDetails(errors)
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Validation failed."
            };

            await WriteProblemAsync(context, problem, StatusCodes.Status400BadRequest);
        }
        catch (InvalidCredentialsException exception)
        {
            Activity.Current?.SetStatus(ActivityStatusCode.Error, exception.Message);
            var problem = new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Authentication failed.",
                Detail = exception.Message
            };

            await WriteProblemAsync(context, problem, StatusCodes.Status401Unauthorized);
        }
        catch (DomainException exception)
        {
            Activity.Current?.SetStatus(ActivityStatusCode.Error, exception.Message);
            var problem = new ProblemDetails
            {
                Status = StatusCodes.Status422UnprocessableEntity,
                Title = "Business rule violation.",
                Detail = exception.Message
            };

            await WriteProblemAsync(context, problem, StatusCodes.Status422UnprocessableEntity);
        }
        catch (Exception exception)
        {
            Activity.Current?.SetStatus(ActivityStatusCode.Error, exception.Message);
            logger.LogError(exception, "An unhandled error occurred while processing the request.");

            var problem = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "An unexpected error occurred."
            };

            await WriteProblemAsync(context, problem, StatusCodes.Status500InternalServerError);
        }
    }

    private static async Task WriteProblemAsync<TProblem>(
        HttpContext context,
        TProblem problem,
        int statusCode)
        where TProblem : ProblemDetails
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        await context.Response.WriteAsJsonAsync(problem, context.RequestAborted);
    }
}