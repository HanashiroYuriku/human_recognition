namespace human_recognition.Application.Common.Models;

/* 
Optional wrapper response
* Use this wrapper if frontend must accept a same format
*/
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public Dictionary<string, string[]>? Errors { get; set; }

    public static ApiResponse<T> SuccessResult(T data, string message = "Success")
        => new()
        {
            Success = true,
            Message = message,
            Data = data
        };

    public static ApiResponse<T> FailureResult(string message, Dictionary<string, string[]>? errors = null)
        => new()
        {
            Success = false,
            Message = message,
            Errors = errors
        };
}