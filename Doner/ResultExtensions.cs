using LanguageExt.Common;

namespace Doner;

public static class ResultExtensions
{
    public static Result<TOut> Bind<TIn, TOut>(this Result<TIn> result, Func<TIn, Result<TOut>> binder)
    {
        return result.Match(binder, e => new Result<TOut>(e));
    }
}