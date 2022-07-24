using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Job.Scheduler.Job;
using Job.Scheduler.Job.Action;
using Job.Scheduler.Job.Exception;
using Job.Scheduler.Utils;
using Serilog;

namespace SoundSwitch.Model.Job;

public class DeviceAddedJob : IDelayedJob
{
    private readonly IAppModel _appModel;
    private readonly string _deviceId;

    public DeviceAddedJob(IAppModel appModel, string deviceId)
    {
        _appModel = appModel;
        _deviceId = deviceId;
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        if (!_appModel.AutoAddNewDevice)
        {
            return;
        }

        while (_appModel.ActiveAudioDeviceLister.Refreshing && !cancellationToken.IsCancellationRequested)
        {
            await TaskUtils.WaitForDelayOrCancellation(TimeSpan.FromSeconds(1), cancellationToken);
        }

        if (cancellationToken.IsCancellationRequested)
        {
            return;
        }

        SelectDevice();
    }

    private void SelectDevice()
    {
        var device = _appModel.ActiveAudioDeviceLister.PlaybackDevices.FirstOrDefault(info => info.Id == _deviceId) 
                     ?? _appModel.ActiveAudioDeviceLister.RecordingDevices.FirstOrDefault(info => info.Id == _deviceId);
        if (device == null)
        {
            Log.Warning("Couldn't find device with ID {deviceId}.", _deviceId);

            return;
        }

        Log.Information("New device detected {device}, auto added to the selection.", device);

        _appModel.SelectDevice(device);
    }

    public Task OnFailure(JobException exception)
    {
        return Task.CompletedTask;
    }

    public IRetryAction FailRule { get; } = new NoRetry();
    public TimeSpan? MaxRuntime { get; } = TimeSpan.FromMinutes(5);
    public TimeSpan Delay { get; } = TimeSpan.FromMilliseconds(250);
}