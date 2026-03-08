using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using ProFinder.Application.Common.Exceptions;

namespace ProFinder.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            await HandleExceptionAsync(context, exception);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, title) = exception switch
        {
            NotFoundException => (StatusCodes.Status404NotFound, "Recurso não encontrado."),
            ValidationException => (StatusCodes.Status400BadRequest, "Falha de validação."),
            DuplicateResourceException => (StatusCodes.Status409Conflict, "Conflito de dados."),
            _ => (StatusCodes.Status500InternalServerError, "Erro interno do servidor.")
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(exception, "Unhandled exception while processing request.");
        }
        else
        {
            _logger.LogWarning(exception, "Handled application exception.");
        }

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        var problemDetails = new ProblemDetails
        {
            Title = title,
            Detail = exception.Message,
            Status = statusCode,
            Instance = context.Request.Path
        };

        var payload = JsonSerializer.Serialize(problemDetails);
        await context.Response.WriteAsync(payload);
    }
}
