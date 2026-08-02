namespace eMarket.SharedKernel.Results;

public static class ResultExtensions
{
    public static TResult Match<TResult>(
        this Result result,
        Func<TResult> onSuccess,
        Func<Error, TResult> onFailure)
    {
        return result.IsSuccess
            ? onSuccess()
            : onFailure(result.Error);
    }
    public static TResult Match<T, TResult>(
       this Result<T> result,
       Func<T, TResult> onSuccess,
       Func<Error, TResult> onFailure)
    {
        return result.IsSuccess
            ? onSuccess(result.Value!)
            : onFailure(result.Error);
    }
    
    public static Result<TOut> Map<TIn, TOut>(
        this Result<TIn> result,
        Func<TIn, TOut> mapper)
    {
        if (result.IsFailure)
            return Result<TOut>.Failure(result.Error);

        return Result<TOut>.Success(mapper(result.Value!));
    }

    public static Result Ensure(
        this Result result,
        Func<bool> predicate,
        Error error)
    {
        if (result.IsFailure)
            return result;

        return predicate()
            ? Result.Success()
            : Result.Failure(error);
    }

    public static Result<T> Ensure<T>(
        this Result<T> result,
        Func<T, bool> predicate,
        Error error)
    {
        if (result.IsFailure)
            return Result<T>.Failure(result.Error);

        return predicate(result.Value!)
            ? result
            : Result<T>.Failure(error);
    }
}
