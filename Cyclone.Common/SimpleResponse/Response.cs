namespace Cyclone.Common.SimpleResponse;

public class Response<T>
{
    public bool Success { get; private init; }
    public bool Failure { get; private init; }
    public T? Data { get; private init; }
    public string? Message { get; private init; }
    public List<string> Errors { get; private init; } = [];

    public static Response<T> Ok(T data, string? message = null) =>
        new() { Success = true, Failure = false, Data = data, Message = message };

    public static Response<T> Fail(string? message = null, params string[]? errors) =>
        new() { Success = false, Failure = true, Message = message, Errors = (errors ?? []).ToList() };


    public static implicit operator Response<T>(T data) => Ok(data);

    public static implicit operator Response<T>(string errorMessage) => Fail(errorMessage);

    public static implicit operator Response<T>(Exception ex) => Fail(ex.Message);

    public static implicit operator Response<T>((T data, string message) tuple) =>
        Ok(tuple.data, tuple.message);

    public static implicit operator Response<T>((bool success, string message) tuple) =>
        tuple.success ? Ok(default!, tuple.message) : Fail(tuple.message);
    
}