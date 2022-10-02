using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Devices.Enumeration;

namespace SoundSwitch.Bluetooth;

public class BluetoothManager : IDisposable
{
    private readonly BluetoothLEAdvertisementWatcher _bleWatcher;

    public BluetoothManager()
    {
        _bleWatcher = new BluetoothLEAdvertisementWatcher
        {
            ScanningMode = BluetoothLEScanningMode.Active
        };
        foreach (var device in DeviceInformation.FindAllAsync(BluetoothDevice.GetDeviceSelectorFromPairingState(true)).AsTask().Result)
        {
            Console.Out.WriteLine($"\t\t\tName: {device.Name}\n\t\t\tID: {device.Id}\n\t\t\tAddr: {device.Pairing.CanPair}");
        }
        //_bleWatcher.Received += BleWatcherOnReceived;
    }

    private void BleWatcherOnReceived(BluetoothLEAdvertisementWatcher sender, BluetoothLEAdvertisementReceivedEventArgs args)
    {
        try
        {
            var device = BluetoothLEDevice.FromBluetoothAddressAsync(args.BluetoothAddress).AsTask().Result;
            Console.Out.WriteLine($"\t\t\tName: {device.Name}\n\t\t\tID: {device.DeviceId}\n\t\t\tAddr: {device.BluetoothAddress}");


        }
        catch (Exception e)
        {
            //ignored
        }
    }

    public void Start()
    {
        _bleWatcher.Start();
    }

    public void Dispose()
    {
        _bleWatcher.Stop();
    }
}