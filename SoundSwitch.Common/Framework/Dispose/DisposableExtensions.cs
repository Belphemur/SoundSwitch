using System;
using System.Threading;

namespace SoundSwitch.Common.Framework.Dispose
{
    public static class Disposable
    {
        public static IDisposable DisposeOnCancellation(this IDisposable disposable, CancellationToken token)
        {
            var cancellationTokenRegistration = token.Register(disposable.Dispose);
            return new DisposableScope(
                () =>
                {
                    cancellationTokenRegistration.Dispose();
                    disposable.Dispose();
                });
        }

    }
}