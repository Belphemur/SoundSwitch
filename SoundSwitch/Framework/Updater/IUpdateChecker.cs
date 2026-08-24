using System;
using System.Threading;
using System.Threading.Tasks;

namespace SoundSwitch.Framework.Updater;

/// <summary>
/// Contract implemented by update checkers (stable/beta or nightly).
/// </summary>
public interface IUpdateChecker
{
    bool Beta { get; set; }

    event EventHandler<UpdateChecker.NewReleaseEvent> UpdateAvailable;

    Task CheckForUpdate(CancellationToken token);
}
