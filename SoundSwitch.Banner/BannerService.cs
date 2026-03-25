using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;

namespace SoundSwitch.Banner;

/// <summary>
/// Service to manage banner notifications.
/// </summary>
public class BannerService
{
    private static SynchronizationContext? _syncContext;
    private readonly Dictionary<Guid, BannerForm> _bannerForms = new();
    private BannerForm? _singleBanner;
    private BannerForm? _customPositionBanner;
    private const int SPACING = 10;

    private readonly IBannerConfiguration _configuration;
    private readonly IBannerAudioService? _audioService;
    private readonly BannerPositionFactory _positionFactory = new();
    private readonly Action<Point>? _onCustomPositionChanged;

    /// <summary>
    /// Initializes a new instance of the <see cref="BannerService"/> class.
    /// </summary>
    public BannerService(IBannerConfiguration configuration, IBannerAudioService? audioService = null, Action<Point>? onCustomPositionChanged = null)
    {
        _configuration = configuration;
        _audioService = audioService;
        _onCustomPositionChanged = onCustomPositionChanged;
    }

    /// <summary>
    /// Shows a banner notification.
    /// </summary>
    public BannerForm? Show(BannerRequest request, BannerPosition positionType, bool persistent = false)
    {
        if (_syncContext == null)
            throw new InvalidOperationException("BannerService must be initialized via Setup() in the UI thread.");

        var position = _positionFactory.Get(positionType);

        if (request.CustomPositionMode)
        {
            return ShowOrUpdateCustomPositionNotification(request, position);
        }

        BannerForm? result = null;
        _syncContext.Send(_ =>
        {
            if (_configuration.MaxConcurrentBanners > 1 || persistent)
            {
                var banner = new BannerForm(_configuration, _audioService, _onCustomPositionChanged);
                if (!persistent)
                {
                    banner.Disposed += (s, e) => { _bannerForms.Remove(banner.Id); };
                    _bannerForms.Add(banner.Id, banner);

                    foreach (var bannerForm in _bannerForms.Values.Where(f => f.Id != banner.Id))
                    {
                        bannerForm.UpdateLocationOpacity(position, banner.Height + SPACING, 0.10, 100 / _configuration.MaxConcurrentBanners);
                    }
                }
                banner.SetData(request, position, persistent);
                result = banner;
            }
            else
            {
                if (_singleBanner == null || _singleBanner.IsDisposed)
                {
                    _singleBanner = new BannerForm(_configuration, _audioService, _onCustomPositionChanged);
                    _singleBanner.Disposed += (s, e) => { _singleBanner = null; };
                }

                _singleBanner.SetData(request, position, persistent);
                result = _singleBanner;
            }
        }, null);
        return result;
    }

    private BannerForm? ShowOrUpdateCustomPositionNotification(BannerRequest request, IPosition position)
    {
        BannerForm? result = null;
        _syncContext!.Send(_ =>
        {
            if (_customPositionBanner == null || _customPositionBanner.IsDisposed)
            {
                _customPositionBanner = new BannerForm(_configuration, _audioService, _onCustomPositionChanged);
                _customPositionBanner.Disposed += (s, e) => { _customPositionBanner = null; };
            }

            _customPositionBanner.SetData(request, position);
            result = _customPositionBanner;
        }, null);
        return result;
    }



    /// <summary>
    /// Initializes the synchronization context. Must be called from the UI thread.
    /// </summary>
    public static void Setup()
    {
        _syncContext = SynchronizationContext.Current;
        if (_syncContext is not WindowsFormsSynchronizationContext)
            throw new InvalidOperationException("BannerService must be initialized in the context of the UI thread.");
    }
}
