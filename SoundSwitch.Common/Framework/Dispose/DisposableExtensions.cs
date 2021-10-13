using System;
using System.Threading;

namespace SoundSwitch.Common.Framework.Dispose
{
    public static class Disposable
    {
        /// <summary>
        /// Create a scope that will dispose the <see cref="disposable"/> on dispose or <see cref="token"/> cancellation.
        /// </summary>
        public static IDisposable DisposeOnCancellation(this IDisposable disposable, CancellationToken token)
        {
            return new CancellableDisposableScope(disposable.Dispose, token);
        }
    }
}