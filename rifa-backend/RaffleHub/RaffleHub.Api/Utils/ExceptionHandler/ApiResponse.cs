namespace RaffleHub.Api.Utils.ExceptionHandler;

public class ApiResponse<T>
{
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }

    public static ApiResponse<T> Ok(T? data, string message = "Operação realizada com sucesso") => new() 
    { 
        Message = message,
        Data = data,
    };

    public static ApiResponse<object> Fail(string message, object? data = null) => new() 
    { 
        Message = message,
        Data = data ?? new { },
    };
}