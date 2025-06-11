#nullable enable

using System;
using RailSharp.Internal.Result;

namespace SoundSwitch.Util;

public static class ResultTypeExtension
{
    public static RailSharp.Result<TFailure, TSuccess> Catch<TFailure, TSuccess>(
        this RailSharp.Result<TFailure, TSuccess> result,
        Func<TFailure, RailSharp.Result<TFailure, TSuccess>> mapper)
    {
        return !(result is Failure<TFailure, TSuccess> failure) ? result : mapper((TFailure)failure);
    }

    /// <summary>
    /// Unwrap the failure case
    /// </summary>
    /// <param name="result"></param>
    /// <param name="defaultValue"></param>
    /// <typeparam name="TFailure"></typeparam>
    /// <typeparam name="TSuccess"></typeparam>
    /// <returns></returns>
    public static TFailure? UnwrapFailure<TFailure, TSuccess>(this RailSharp.Result<TFailure, TSuccess> result, TFailure? defaultValue = null) where TFailure : class
    {
        if (result is Failure<TFailure, TSuccess> failure)
        {
            return failure;
        }

        return defaultValue;
    }

    /// <summary>
    /// Unwrap the failure case
    /// </summary>
    /// <param name="result"></param>
    /// <param name="defaultValue"></param>
    /// <typeparam name="TFailure"></typeparam>
    /// <typeparam name="TSuccess"></typeparam>
    /// <returns></returns>
    public static TFailure? UnwrapFailure<TFailure, TSuccess>(this RailSharp.Result<TFailure, TSuccess> result, TFailure? defaultValue = null) where TFailure : struct
    {
        if (result is Failure<TFailure, TSuccess> failure)
        {
            return failure;
        }

        return defaultValue;
    }

    /// <summary>
    /// Unwrap the success value
    /// </summary>
    /// <param name="result"></param>
    /// <param name="defaultValue"></param>
    /// <typeparam name="TFailure"></typeparam>
    /// <typeparam name="TSuccess"></typeparam>
    /// <returns></returns>
    public static TSuccess? UnwrapSuccess<TFailure, TSuccess>(this RailSharp.Result<TFailure, TSuccess> result, TSuccess? defaultValue = default)
    {
        if (result is Success<TFailure, TSuccess> success)
        {
            return success;
        }

        return defaultValue;
    }
}