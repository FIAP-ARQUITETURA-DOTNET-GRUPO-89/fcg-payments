using OperationResult;

namespace FcgPayments.Api.Extensions;

public static class ResultExtensions
{
    public static IResult ToOkResult<T>(this Result<T> result) 
        => result.IsSuccess
            ? Results.Ok(result.Value)
            : throw result.Exception!;

    public static IResult ToCreatedResult<T>(this Result<T> result, Func<T, string> locationFactory) 
        => result.IsSuccess
            ? Results.Created(locationFactory(result.Value!), result.Value)
            : throw result.Exception!;
}
