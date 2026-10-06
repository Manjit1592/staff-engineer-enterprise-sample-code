namespace Enterprise.SharedKernel.Responses;

public class Result
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;

    public static Result Ok(string message = "Success")
        => new() { Success = true, Message = message };

    public static Result Fail(string message)
        => new() { Success = false, Message = message };
}

public class Result<T> : Result
{
    public T? Data { get; set; }

    public static Result<T> Ok(T data, string message = "Success")
        => new() { Success = true, Data = data, Message = message };

    public new static Result<T> Fail(string message)
        => new() { Success = false, Message = message };
}
