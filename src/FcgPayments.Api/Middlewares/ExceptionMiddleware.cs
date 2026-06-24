using System.Net;
using FluentValidation;
using FcgPayments.SharedKernel.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace FcgPayments.Api.Middlewares;

public class ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger<ExceptionMiddleware> _logger = logger;

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException ex)
        {
            var errors = ex.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray()
                );

            await WriteProblemDetails(context, StatusCodes.Status400BadRequest, errors);
        }
        catch (BusinessException ex)
        {
            _logger.LogWarning(ex, "Erro de negócio na requisição {Method} {Path}", context.Request.Method, context.Request.Path);

            var statusCode = ex switch
            {
                AlreadyExistsException => StatusCodes.Status409Conflict,
                NotFoundException => StatusCodes.Status404NotFound,
                _ => StatusCodes.Status400BadRequest
            };

            await WriteProblemDetails(context, statusCode, ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Falha na validação de acesso em {Method} {Path} | Motivo: {Message}", context.Request.Method, context.Request.Path, ex.Message);

            await WriteProblemDetails(context, StatusCodes.Status401Unauthorized, ex.Message);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Forbidden)
        {
            _logger.LogWarning(ex, "Acesso proibido em {Method} {Path} | Motivo: {Message}", context.Request.Method, context.Request.Path, ex.Message);

            await WriteProblemDetails(context, StatusCodes.Status403Forbidden, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado na requisição {Method} {Path}", context.Request.Method, context.Request.Path);

            await WriteProblemDetails(context, StatusCodes.Status500InternalServerError, "Ocorreu um erro inesperado. Tente novamente mais tarde!");
        }
    }

    private static async Task WriteProblemDetails(
        HttpContext context,
        int statusCode,
        object? details)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        var problem = new ProblemDetails
        {
            Title = GetTitle(statusCode),
            Status = statusCode
        };

        if (details is string str)
        {
            problem.Detail = str;
        }
        else if (details is not null)
        {
            problem.Extensions["errors"] = details;
        }

        await context.Response.WriteAsJsonAsync(problem);
    }

    private static string GetTitle(int statusCode) => statusCode switch
    {
        StatusCodes.Status400BadRequest => "Bad Request",
        StatusCodes.Status401Unauthorized => "Unauthorized",
        StatusCodes.Status403Forbidden => "Forbidden",
        StatusCodes.Status404NotFound => "Not Found",
        StatusCodes.Status409Conflict => "Conflict",
        StatusCodes.Status500InternalServerError => "Internal Server Error",
        _ => "Error"
    };
}
