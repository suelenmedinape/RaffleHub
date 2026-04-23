using FluentResults;
using Microsoft.AspNetCore.Mvc;
using RaffleHub.Api.Utils.ExceptionHandler;

namespace RaffleHub.Api.Utils.Extensions;

public static class ResultExtensions
{
    public static IActionResult ToResponse<T>(this Result<T> result, string? successMessage = null)
    {
        if (result.IsSuccess)
        {
            return new OkObjectResult(ApiResponse<T>.Ok(result.Value, successMessage ?? "Sucesso ao processar a requisição"));
        }

        var firstError = result.Errors.FirstOrDefault()?.Message ?? "Alguns erros ocorreram";
        return new BadRequestObjectResult(ApiResponse<object>.Fail(firstError, null));
    }

    public static IActionResult ToCreatedResponse<T>(this Result<T> result, string actionName, object? routeValues = null, string? successMessage = null)
    {
        if (result.IsSuccess)
        {
            var response = ApiResponse<T>.Ok(result.Value, successMessage ?? "Recurso criado com sucesso");
            return new CreatedAtActionResult(actionName, null, routeValues, response);
        }

        var firstError = result.Errors.FirstOrDefault()?.Message ?? "Erro ao criar recurso";
        return new BadRequestObjectResult(ApiResponse<object>.Fail(firstError, null));
    }
    
    public static IActionResult ToResponse(this Result result, string? successMessage = null)
    {
        if (result.IsSuccess)
            return new OkObjectResult(ApiResponse<object>.Ok(null, successMessage ?? "Operação realizada com sucesso"));

        var firstError = result.Errors.FirstOrDefault()?.Message ?? "Ocorreu um erro na operação";
        return new BadRequestObjectResult(ApiResponse<object>.Fail(firstError, null));
    }
}