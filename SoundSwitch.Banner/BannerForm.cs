using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace SoundSwitch.Banner;

/// <summary>
/// This class implements the UI form used to show a Banner notification.
/// </summary>
public partial class BannerForm : Form
{
    private sealed class CustomPositionMessageFilter(BannerForm owner) : IMessageFilter
    {
        private const int WmKeyDown = 0x0100;
        private const int WmSysKeyDown = 0x0104;

        public bool PreFilterMessage(ref Message m)
        {
            if (m.Msg != WmKeyDown && m.Msg != WmSysKeyDown)
            {
                return false;
            }

            return owner.HandleCustomPositionKey((Keys)m.WParam.ToInt32());
        }
    }

    private Timer? _timerHide;
    private bool _hiding;
    private BannerRequest? _currentRequest;
    private readonly BannerPositionFactory _bannerPositionFactory = new();
    private CancellationTokenSource _cancellationTokenSource = new();
    private IPosition? _currentPosition;
    private int _currentOffset;
    private int _hidePercentage = 100;
    private readonly float _defaultFontSize;
    private readonly Size _defaultPictureSize;
    private readonly Padding _defaultPadding;
    private bool _isCompact;
    private Point _lastMousePosition;
    private CustomPositionMessageFilter? _customPositionMessageFilter;

    private readonly IBannerConfiguration _configuration;
    private readonly IBannerAudioService? _audioService;
    private readonly Action<Point>? _onCustomPositionChanged;

    public Guid Id { get; } = Guid.NewGuid();

    /// <summary>
    /// Gets the Screen object to display the banner on.
    /// </summary>
    internal Screen GetScreen() =>
        (_configuration.NotifyUsingPrimaryScreen ? Screen.PrimaryScreen : Screen.FromPoint(Cursor.Position))!;

    /// <summary>
    /// Constructor for the <see cref="BannerForm"/> class.
    /// </summary>
    public BannerForm(IBannerConfiguration configuration, IBannerAudioService? audioService = null, Action<Point>? onCustomPositionChanged = null)
    {
        _configuration = configuration;
        _audioService = audioService;
        _onCustomPositionChanged = onCustomPositionChanged;

        InitializeComponent();
        StartPosition = FormStartPosition.Manual;
        Bounds = GetScreen().Bounds;
        TopMost = true;
        TopLevel = true;
        FormBorderStyle = FormBorderStyle.None;
        ShowInTaskbar = false;

        // Store default sizes for compact mode calculations
        _defaultFontSize = Font.Size;
        _defaultPictureSize = pbxLogo.Size;
        _defaultPadding = Padding;

        // Register event handlers
        RegisterHandlers(this);
        RegisterHandlers(lblTitle);
        RegisterHandlers(lblTop);
        RegisterHandlers(pbxLogo);
        RegisterHandlers(tableLayoutPanel);

        // Enable double buffering to reduce flicker
        SetStyle(ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.UserPaint, true);

        // Also enable double buffering for container controls
        foreach (Control control in Controls)
            EnableDoubleBuffering(control);
    }

    private void RegisterHandlers(Control control)
    {
        control.Click += BannerForm_Click;
        control.MouseDown += BannerForm_MouseDown;
        control.MouseUp += BannerForm_MouseUp;
        control.MouseMove += BannerForm_MouseMove;
        control.KeyDown += BannerForm_KeyDown;
    }

    protected override bool ShowWithoutActivation => true;

    protected override CreateParams CreateParams
    {
        get
        {
            var cp = base.CreateParams;
            cp.ExStyle |= 0x80;        // WS_EX_TOOLWINDOW
            cp.ExStyle |= 0x08000000;  // WS_EX_NOACTIVATE
            cp.ExStyle |= 0x00000008;  // WS_EX_TOPMOST
            return cp;
        }
    }

    protected override void WndProc(ref Message m)
    {
        const int WM_NCHITTEST = 0x0084;
        const int HTCLIENT = 1;
        const int WM_LBUTTONUP = 0x0202;
        const int WM_ERASEBKGND = 0x0014;

        switch (m.Msg)
        {
            case WM_NCHITTEST:
                m.Result = (IntPtr)HTCLIENT;
                return;
            case WM_LBUTTONUP:
                Point location = PointToClient(new Point(m.LParam.ToInt32() & 0xFFFF, m.LParam.ToInt32() >> 16));
                BannerForm_Click(this, new MouseEventArgs(MouseButtons.Left, 1, location.X, location.Y, 0));
                m.Result = IntPtr.Zero;
                return;
            case WM_ERASEBKGND:
                m.Result = (IntPtr)1;
                return;
            default:
                base.WndProc(ref m);
                break;
        }
    }

    private void EnableDoubleBuffering(Control control)
    {
        typeof(Control).GetProperty("DoubleBuffered",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance)?
            .SetValue(control, true);

        foreach (Control childControl in control.Controls)
            EnableDoubleBuffering(childControl);
    }

    public void SetData(BannerRequest request, IPosition? position = null, bool persistent = false)
    {
        if (_currentRequest != null && _currentRequest.Priority > request.Priority) return;

        _currentRequest = request;
        
        var ttl = request.Ttl ?? _configuration.Ttl;

        if (!persistent && ttl != TimeSpan.MaxValue)
        {
            if (_timerHide == null)
            {
                _timerHide = new Timer { Interval = (int)ttl.TotalMilliseconds };
                _timerHide.Tick += TimerHide_Tick!;
            }
            else
            {
                _timerHide.Stop();
                _timerHide.Interval = (int)ttl.TotalMilliseconds;
            }
        }
        else
        {
            _timerHide?.Stop();
        }

        if (request.Image != null)
            pbxLogo.Image = request.Image;

        if (request.SoundPath != null && _audioService != null)
        {
            ResetAudio();
            _audioService.PlaySoundAsync(request.SoundPath, request.PlaybackDeviceId, _cancellationTokenSource.Token);
        }

        _hiding = false;
        Opacity = (double)_configuration.Opacity / 100;
        lblTitle.Text = request.Text;
        lblTop.Text = request.Title;

        if (request.CustomPositionMode)
        {
            _customPositionMessageFilter ??= new CustomPositionMessageFilter(this);
            Application.AddMessageFilter(_customPositionMessageFilter);
        }
        else if (_customPositionMessageFilter != null)
        {
            Application.RemoveMessageFilter(_customPositionMessageFilter);
            _customPositionMessageFilter = null;
        }

        if (request.CompactMode)
            ApplyCompactMode();

        Region = Region.FromHrgn(RoundedCorner.CreateRoundRectRgn(0, 0, Width, Height, 20, 20));

        if (position != null)
            _currentPosition = position;

        if (_currentPosition != null)
            Location = _currentPosition.GetScreenPosition(GetScreen(), Height, Width, _currentOffset, _configuration.CustomPosition);


        if (_timerHide != null && !persistent && ttl != TimeSpan.MaxValue)
            _timerHide.Enabled = true;

        Show();
        BringToFront();
        TopMost = false;
        TopMost = true;
    }

    public void UpdatePosition(IPosition position, int offset)
    {
        Location = position.GetScreenPosition(GetScreen(), Height, Width, offset, _configuration.CustomPosition);
        _currentOffset = offset;
    }

    public void UpdateLocationOpacity(IPosition position, int positionChange, double opacityChange, int hideChange)
    {
        _currentOffset += positionChange;
        Location = position.GetScreenPosition(GetScreen(), Height, Width, _currentOffset, _configuration.CustomPosition);
        Opacity -= opacityChange;
        _hidePercentage -= hideChange;

        if (Opacity <= 0.0 || _hidePercentage <= 0)
        {
            _hiding = true;
            Dispose();
        }
    }

    private void ResetAudio()
    {
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();
        _cancellationTokenSource = new CancellationTokenSource();
    }

    private void ApplyCompactMode()
    {
        if (_isCompact) return;

        const float scaleFactorImage = 0.1f;
        const float scaleFactorFont = 0.8f;

        Font = new Font(Font.FontFamily, _defaultFontSize * scaleFactorFont, Font.Style);
        lblTop.Font = new Font(lblTop.Font.FontFamily, lblTop.Font.Size * scaleFactorFont, lblTop.Font.Style);
        lblTitle.Font = new Font(lblTitle.Font.FontFamily, lblTitle.Font.Size * scaleFactorFont, lblTitle.Font.Style);

        if (pbxLogo.Image != null && _defaultPictureSize.Width > 0 && _defaultPictureSize.Height > 0)
        {
            var newWidth = (int)(_defaultPictureSize.Width * scaleFactorImage);
            var newHeight = (int)(_defaultPictureSize.Height * scaleFactorImage);

            if (newWidth > 0 && newHeight > 0)
            {
                pbxLogo.Size = new Size(newWidth, newHeight);
            }
        }

        Padding = new Padding(
            (int)(_defaultPadding.Left * scaleFactorImage),
            (int)(_defaultPadding.Top * scaleFactorImage),
            (int)(_defaultPadding.Right * scaleFactorImage),
            (int)(_defaultPadding.Bottom * scaleFactorImage)
        );

        PerformLayout();
        _isCompact = true;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (_customPositionMessageFilter != null)
            {
                Application.RemoveMessageFilter(_customPositionMessageFilter);
                _customPositionMessageFilter = null;
            }

            _timerHide?.Dispose();
            _cancellationTokenSource.Dispose();
            components?.Dispose();
        }

        base.Dispose(disposing);
    }

    private void TimerHide_Tick(object sender, EventArgs e)
    {
        TriggerHidingDisposal();
    }

    private void TriggerHidingDisposal()
    {
        if (_hiding) return;

        _hiding = true;
        if (_timerHide != null)
            _timerHide.Enabled = false;
        ResetAudio();
        FadeOut();
    }

    private async void FadeOut()
    {
        try
        {
            while (Opacity > 0.0)
            {
                await Task.Delay(50);

                if (!_hiding)
                    break;
                Opacity -= 0.05;
            }

            if (_hiding) Dispose();
        }
        catch (Win32Exception)
        {
            try { Dispose(); } catch { /* Ignored */ }
        }
    }

    private void BannerForm_Click(object? sender, EventArgs e)
    {
        if (_currentRequest?.CustomPositionMode == true) return;

        if (_currentRequest?.OnClick == null)
        {
            TriggerHidingDisposal();
            return;
        }

        _currentRequest.OnClick.Invoke(this, e);
    }

    private void BannerForm_MouseDown(object? sender, MouseEventArgs e)
    {
        if (_currentRequest?.CustomPositionMode != true) return;
        if (e.Button == MouseButtons.Left)
        {
            _lastMousePosition = new Point(e.X, e.Y);
            _timerHide?.Stop();
        }
    }

    private void BannerForm_MouseUp(object? sender, MouseEventArgs e)
    {
        if (_currentRequest?.CustomPositionMode != true) return;
        if (e.Button == MouseButtons.Left)
        {
            _onCustomPositionChanged?.Invoke(Location);
            _timerHide?.Start();
        }
    }

    private void BannerForm_MouseMove(object? sender, MouseEventArgs e)
    {
        if (_currentRequest?.CustomPositionMode != true) return;
        if (e.Button == MouseButtons.Left)
        {
            var screen = GetScreen().Bounds;

            Point newLocation = new(
                Left + e.X - _lastMousePosition.X,
                Top + e.Y - _lastMousePosition.Y);

            newLocation.X = Math.Max(0, Math.Min(newLocation.X, screen.Width - Width));
            newLocation.Y = Math.Max(0, Math.Min(newLocation.Y, screen.Height - Height));

            Location = newLocation;
        }
    }

    private void BannerForm_KeyDown(object? sender, KeyEventArgs e)
    {
        if (_currentRequest?.CustomPositionMode != true) return;
        if (HandleCustomPositionKey(e.KeyCode))
        {
            e.Handled = true;
        }
    }

    private bool HandleCustomPositionKey(Keys key)
    {
        if (_currentRequest == null || !_currentRequest.CustomPositionMode)
        {
            return false;
        }

        switch (key)
        {
            case Keys.Escape:
                Dispose();
                return true;
            case Keys.R:
                Location = Point.Empty;
                _onCustomPositionChanged?.Invoke(Location);
                return true;
            default:
                return false;
        }
    }
}
