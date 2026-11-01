using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace StudentContingent.API.Filters;

/// <summary>
/// Переводит исключения репозиториев и доменной модели в HTTP-ответы:
/// KeyNotFoundException → 404, InvalidOperationException → 400, DbUpdateException → 409.
/// </summary>
public class DomainExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        var (status, title) = context.Exception switch
        {
            KeyNotFoundException => (StatusCodes.Status404NotFound, "Объект не найден"),
            InvalidOperationException => (StatusCodes.Status400BadRequest, "Операция недопустима"),
            DbUpdateException => (StatusCodes.Status409Conflict, "Конфликт данных"),
            _ => (0, string.Empty)
        };

        if (status == 0)
        {
            return;
        }

        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = context.Exception.InnerException?.Message ?? context.Exception.Message
        };
        context.Result = new ObjectResult(problem) { StatusCode = status };
        context.ExceptionHandled = true;
    }
}
