using System;
using RailSharp.Internal.Result;

namespace SoundSwitch.Util
{
    public static class ResultTypeExtension
    {
        public static RailSharp.Result<TFailure, TSuccess> Catch<TFailure, TSuccess>(
            this RailSharp.Result<TFailure, TSuccess> result,
            Func<TFailure, RailSharp.Result<TFailure, TSuccess>>                  mapper)
        {
            return !(result is Failure<TFailure, TSuccess> failure) ? result :  mapper((TFailure) failure);
        }
    }
}