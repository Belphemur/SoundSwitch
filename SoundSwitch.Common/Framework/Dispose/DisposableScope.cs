using System;
using System.Threading;

namespace SoundSwitch.Common.Framework.Dispose
{
    public class DisposableScope : IDisposable
    {
        private readonly Action _closeScopeAction;

        public DisposableScope(Action closeScopeAction)
        {
            _closeScopeAction = closeScopeAction;
        }

        public virtual void Dispose()
        {
            _closeScopeAction();
        }
    }

    public sealed class CancellableDisposableScope : DisposableScope
    {
        private readonly CancellationTokenRegistration _cancellationTokenRegistration;
        private bool _isDisposed;

        public CancellableDisposableScope(Action closeScopeAction, CancellationToken token) : base(closeScopeAction)
        {
            _cancellationTokenRegistration = token.Register(Dispose);
        }

        public override void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _cancellationTokenRegistration.Dispose();
            base.Dispose();
            _isDisposed = true;
        }
    }
}