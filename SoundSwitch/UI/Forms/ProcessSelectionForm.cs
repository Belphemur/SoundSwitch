using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

using Serilog;

using SoundSwitch.Audio.Manager;
using SoundSwitch.Framework.Audio;
using SoundSwitch.Common.Framework.Icon;
using SoundSwitch.Framework.WinApi;
using SoundSwitch.Localization;
using SoundSwitch.UI.Component;

namespace SoundSwitch.UI.Forms;

public partial class ProcessSelectionForm : Form
{
    /// <summary>Presentation-only classification of a row, newest signal first (design doc §5).</summary>
    private enum PickerGroup
    {
        HasOwnDevice,
        UsingAudioNow,
        AllProcesses
    }

    public string SelectedProcessName { get; private set; } = string.Empty;
    public string SelectedProcessPath { get; private set; } = string.Empty;
    public string SelectedWindowTitle { get; private set; } = string.Empty;

    private static readonly ILogger _logger = Log.ForContext<ProcessSelectionForm>();
    private readonly PersistedDefaultEndpointStore _persistedStore = new();
    private List<ProcessInfo> _allProcesses;
    private List<ProcessInfo> _notRunningApps;
    private bool _showAllProcesses;
    private bool _allProcessesExpandedManually;

    public ProcessSelectionForm()
    {
        InitializeComponent();
        MinimumSize = new System.Drawing.Size(600, 400);
        LocalizeForm();
        LoadProcesses();
        ApplyTheme();
    }

    protected override void OnSystemColorsChanged(EventArgs e)
    {
        base.OnSystemColorsChanged(e);
        ApplyTheme();
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        // App-mode light/dark flips do not raise OnSystemColorsChanged; the repository
        // exposes them through WindowsAPIAdapter.SystemThemeChanged (see Settings form).
        WindowsAPIAdapter.SystemThemeChanged += OnSystemThemeChanged;
    }

    protected override void OnHandleDestroyed(EventArgs e)
    {
        WindowsAPIAdapter.SystemThemeChanged -= OnSystemThemeChanged;
        base.OnHandleDestroyed(e);
    }

    private void OnSystemThemeChanged(object sender, EventArgs e)
    {
        // The event is posted asynchronously, so re-check teardown state on both
        // sides of the marshal (see ThemeChangeDispatcher).
        ThemeChangeDispatcher.BeginThemeUpdate(this, ApplyTheme);
    }

    internal static Color DarkGridBackgroundColor => Color.FromArgb(32, 32, 32);
    internal static Color DarkGridForegroundColor => Color.FromArgb(240, 240, 240);
    internal static Color DarkGridLineColor => Color.FromArgb(80, 80, 80);
    internal static Color DarkGridHeaderColor => Color.FromArgb(45, 45, 45);
    internal static Color DarkGridDimmedColor => Color.FromArgb(128, 128, 128);

    /// <summary>
    /// Theme the DataGridView: it does not follow the application dark mode automatically,
    /// so its cell styles are set explicitly. Light mode uses the standard framework palette.
    /// </summary>
    internal void ApplyTheme()
    {
        ApplyTheme(dgvProcesses, WindowsThemeHelper.IsDarkModeEnabled());
        foreach (DataGridViewRow row in dgvProcesses.Rows)
        {
            StyleRow(row);
        }
    }

    internal static void ApplyTheme(DataGridView dgv, bool isDarkMode)
    {
        if (isDarkMode)
        {
            dgv.EnableHeadersVisualStyles = false;
            dgv.BackgroundColor = DarkGridBackgroundColor;
            dgv.GridColor = DarkGridLineColor;
            dgv.DefaultCellStyle.BackColor = DarkGridBackgroundColor;
            dgv.DefaultCellStyle.ForeColor = DarkGridForegroundColor;
            dgv.DefaultCellStyle.SelectionBackColor = SystemColors.Highlight;
            dgv.DefaultCellStyle.SelectionForeColor = SystemColors.HighlightText;
            dgv.ColumnHeadersDefaultCellStyle.BackColor = DarkGridHeaderColor;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = DarkGridForegroundColor;
            dgv.ColumnHeadersDefaultCellStyle.SelectionBackColor = DarkGridHeaderColor;
        }
        else
        {
            dgv.EnableHeadersVisualStyles = true;
            // White "list canvas": the data area and its void read as the list surface
            // (like the grouped design), not the silver AppWorkspace chrome.
            dgv.BackgroundColor = SystemColors.Window;
            dgv.GridColor = SystemColors.ControlLight;
            dgv.DefaultCellStyle.BackColor = SystemColors.Window;
            dgv.DefaultCellStyle.ForeColor = SystemColors.ControlText;
            dgv.DefaultCellStyle.SelectionBackColor = SystemColors.Highlight;
            dgv.DefaultCellStyle.SelectionForeColor = SystemColors.HighlightText;
            dgv.ColumnHeadersDefaultCellStyle.BackColor = SystemColors.Control;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = SystemColors.ControlText;
            dgv.ColumnHeadersDefaultCellStyle.SelectionBackColor = SystemColors.Highlight;
        }
    }

    /// <summary>
    /// Per-row styling on top of the shared <see cref="ApplyTheme(DataGridView,bool)"/> defaults:
    /// group-header rows and dimmed not-running rows. Colours come from the same theme accessors
    /// as the rest of the grid so light and dark mode both work.
    /// </summary>
    private void StyleRow(DataGridViewRow row)
    {
        switch (row.Tag)
        {
            case GroupHeaderInfo header:
            {
                var isDarkMode = WindowsThemeHelper.IsDarkModeEnabled();
                row.DefaultCellStyle.BackColor = isDarkMode ? DarkGridHeaderColor : SystemColors.Control;
                row.DefaultCellStyle.ForeColor = isDarkMode ? DarkGridForegroundColor : SystemColors.ControlText;
                row.DefaultCellStyle.SelectionBackColor = row.DefaultCellStyle.BackColor;
                row.DefaultCellStyle.SelectionForeColor = row.DefaultCellStyle.ForeColor;
                row.DefaultCellStyle.Font = new Font(dgvProcesses.Font, FontStyle.Bold);
                // Section band: a taller, full-width emphasised row that separates
                // the groups (the row-level style covers every column at once).
                row.Height = dgvProcesses.RowTemplate.Height + 6;
                break;
            }
            case ProcessInfo {IsNotRunning: true}:
            {
                var isDarkMode = WindowsThemeHelper.IsDarkModeEnabled();
                row.DefaultCellStyle.ForeColor = isDarkMode ? DarkGridDimmedColor : SystemColors.GrayText;
                row.DefaultCellStyle.Font = new Font(dgvProcesses.Font, FontStyle.Italic);
                break;
            }
        }
    }

    private void LocalizeForm()
    {
        Text = SettingsStrings.appSoundLock_selectProcess;
        btnSelect.Text = SettingsStrings.buttonSelect;
        btnCancel.Text = SettingsStrings.buttonClose;
        lblFilter.Text = SettingsStrings.appSoundLock_filter;

        dgvProcesses.Columns.Clear();
        var colIcon = new DataGridViewImageColumn
        {
            Name = "colIcon",
            HeaderText = string.Empty,
            Width = 32,
            ImageLayout = DataGridViewImageCellLayout.Zoom
        };
        // Cells without an image (the group-header bands) would otherwise paint
        // the image column's default placeholder glyph; an empty value must
        // render nothing so band rows show no icon.
        colIcon.DefaultCellStyle.NullValue = null;
        dgvProcesses.Columns.Add(colIcon);
        dgvProcesses.Columns.Add("colName", SettingsStrings.appSoundLock_rule_processName);
        dgvProcesses.Columns.Add("colTitle", SettingsStrings.appSoundLock_rule_windowTitle);
        dgvProcesses.Columns.Add("colOutput", SettingsStrings.playback);
        dgvProcesses.Columns.Add("colInput", SettingsStrings.recording);
        dgvProcesses.Columns.Add("colPath", SettingsStrings.appSoundLock_rule_processPath);

        dgvProcesses.AllowUserToResizeRows = false;
        dgvProcesses.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgvProcesses.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        // Crisp single-line outline around the list canvas (the default sunken 3D
        // border fights the white surface).
        dgvProcesses.BorderStyle = BorderStyle.FixedSingle;
        // The process-name column carries long names and the group bands (e.g.
        // "▸ Show all processes (247)") plus the not-running suffix; the path
        // column truncates anyway, so it gives up most of its width.
        dgvProcesses.Columns["colName"].FillWeight = 160;
        dgvProcesses.Columns["colPath"].FillWeight = 40;
        if (dgvProcesses.Columns.Contains("colIcon")) dgvProcesses.Columns["colIcon"].Width = 32;
    }

    private void LoadProcesses()
    {
        _allProcesses = new List<ProcessInfo>();
        _notRunningApps = new List<ProcessInfo>();
        var audioSwitcher = AudioSwitcher.Instance;

        var playbackMap = audioSwitcher.GetProcessDeviceMap(Audio.Manager.Interop.Enum.EDataFlow.eRender);
        var recordingMap = audioSwitcher.GetProcessDeviceMap(Audio.Manager.Interop.Enum.EDataFlow.eCapture);

        var defaultPlayback = audioSwitcher.GetDefaultAudioEndpoint(Audio.Manager.Interop.Enum.EDataFlow.eRender, Audio.Manager.Interop.Enum.ERole.eConsole);
        var defaultRecording = audioSwitcher.GetDefaultAudioEndpoint(Audio.Manager.Interop.Enum.EDataFlow.eCapture, Audio.Manager.Interop.Enum.ERole.eConsole);

        var defaultPlaybackName = defaultPlayback?.NameClean ?? "Default";
        var defaultRecordingName = defaultRecording?.NameClean ?? "Default";

        // Apps the user (or Windows) already gave their own device — read straight from the
        // persisted store, a plain registry pass, no COM (design doc §6.1).
        var persistedRecords = _persistedStore.ReadAll();
        var persistedRelativePaths = persistedRecords
            .Select(record => GetVolumeRelativePath(record.ExePath))
            .Where(relative => relative.Length > 0)
            .ToList();

        foreach (var p in Process.GetProcesses())
        {
            using (p)
            {
                try
                {
                    if (p.Id <= 4) continue;

                    var path = string.Empty;
                    try { path = p.MainModule?.FileName ?? string.Empty; }
                    catch (Exception ex)
                    {
                        _logger.Debug(ex, "Failed to get process path for {ProcessName} (PID: {PID})", p.ProcessName, p.Id);
                    }

                    var pid = (uint)p.Id;

                    // 1. Check persisted rules
                    var outputId = audioSwitcher.GetUsedDevice(Audio.Manager.Interop.Enum.EDataFlow.eRender, Audio.Manager.Interop.Enum.ERole.eConsole, pid);
                    var inputId = audioSwitcher.GetUsedDevice(Audio.Manager.Interop.Enum.EDataFlow.eCapture, Audio.Manager.Interop.Enum.ERole.eConsole, pid);

                    var hasOwnDevice =
                        HasPersistedPath(path, persistedRelativePaths) ||
                        !string.IsNullOrEmpty(outputId) ||
                        !string.IsNullOrEmpty(inputId);

                    // 2. Fallback to active sessions
                    if (string.IsNullOrEmpty(outputId)) playbackMap.TryGetValue(pid, out outputId);
                    if (string.IsNullOrEmpty(inputId)) recordingMap.TryGetValue(pid, out inputId);

                    // 3. Resolve names or fallback to system default
                    var outputName = string.IsNullOrEmpty(outputId) ? defaultPlaybackName : (audioSwitcher.GetAudioEndpoint(outputId)?.NameClean ?? outputId);
                    var inputName = string.IsNullOrEmpty(inputId) ? defaultRecordingName : (audioSwitcher.GetAudioEndpoint(inputId)?.NameClean ?? inputId);

                    System.Drawing.Image icon = Properties.Resources.program.ToBitmap();
                    if (!string.IsNullOrEmpty(path) && System.IO.File.Exists(path))
                    {
                        try { icon = IconExtractor.Extract(path, 0, true).ToBitmap(); }
                        catch (Exception ex)
                        {
                            _logger.Debug(ex, "Failed to extract icon for {Path}", path);
                        }
                    }

                    _allProcesses.Add(new ProcessInfo
                    {
                        Name = p.ProcessName,
                        Path = path,
                        Icon = icon,
                        WindowTitle = p.MainWindowTitle,
                        OutputDevice = outputName,
                        InputDevice = inputName,
                        Group = ClassifyGroup(hasOwnDevice, playbackMap.ContainsKey(pid) || recordingMap.ContainsKey(pid))
                    });
                }
                catch (Exception ex)
                {
                    _logger.Debug(ex, "Failed to load individual process info for PID {PID}", p.Id);
                }
            }
        }

        // Only show one row per process/path/window combination
        _allProcesses = _allProcesses
            .GroupBy(x => new {x.Name, x.Path, x.WindowTitle})
            .Select(g => g.First())
            .ToList();

        LoadNotRunningApps(persistedRecords, defaultPlaybackName, defaultRecordingName);

        UpdateGrid();
    }

    private static PickerGroup ClassifyGroup(bool hasOwnDevice, bool hasActiveSession)
    {
        if (hasOwnDevice)
            return PickerGroup.HasOwnDevice;
        if (hasActiveSession)
            return PickerGroup.UsingAudioNow;
        return PickerGroup.AllProcesses;
    }

    private bool HasPersistedPath(string processPath, List<string> persistedRelativePaths)
    {
        if (string.IsNullOrEmpty(processPath))
            return false;

        // Volume-prefix-insensitive suffix match (design doc §6.1): the store keeps NT device
        // paths, so compare on the remainder after the \Device\HarddiskVolume<N>\ prefix.
        return persistedRelativePaths.Any(relative => processPath.EndsWith(relative, StringComparison.OrdinalIgnoreCase));
    }

    private static string GetVolumeRelativePath(string ntPath)
    {
        const string volumePrefix = @"\Device\HarddiskVolume";
        if (string.IsNullOrEmpty(ntPath) || !ntPath.StartsWith(volumePrefix, StringComparison.OrdinalIgnoreCase))
            return string.Empty;

        var index = volumePrefix.Length;
        while (index < ntPath.Length && char.IsDigit(ntPath[index]))
        {
            index++;
        }

        if (index < ntPath.Length && ntPath[index] == '\\')
            return ntPath.Substring(index + 1);

        return string.Empty;
    }

    /// <summary>
    /// Store records that matched no running process become dimmed "(not running)" rows — still
    /// selectable, so a rule can be created for a closed app (design doc §5, §6.3). Records for
    /// the same application are merged into one row, one entry per record key.
    /// </summary>
    private void LoadNotRunningApps(IReadOnlyList<PersistedDefaultEndpointStore.Record> persistedRecords, string defaultPlaybackName, string defaultRecordingName)
    {
        var audioSwitcher = AudioSwitcher.Instance;
        foreach (var records in persistedRecords
                     .GroupBy(record => GetVolumeRelativePath(record.ExePath), StringComparer.OrdinalIgnoreCase)
                     .Where(group => group.Key.Length > 0))
        {
            if (_allProcesses.Any(info => info.Path.Length > 0 && info.Path.EndsWith(records.Key, StringComparison.OrdinalIgnoreCase)))
                continue; // covered by the running row(s)

            var outputDevice = defaultPlaybackName;
            var inputDevice = defaultRecordingName;
            foreach (var record in records)
            {
                var deviceId = GetPreferredDeviceId(record);
                if (deviceId == null)
                    continue;

                var deviceName = audioSwitcher.GetAudioEndpoint(deviceId)?.NameClean ?? deviceId;
                if (record.Flow == Audio.Manager.Interop.Enum.EDataFlow.eRender)
                    outputDevice = deviceName;
                else
                    inputDevice = deviceName;
            }

            _notRunningApps.Add(new ProcessInfo
            {
                Name = System.IO.Path.GetFileNameWithoutExtension(records.Key),
                Path = records.First().ExePath,
                Icon = Properties.Resources.program.ToBitmap(),
                WindowTitle = string.Empty,
                OutputDevice = outputDevice,
                InputDevice = inputDevice,
                Group = PickerGroup.HasOwnDevice,
                IsNotRunning = true
            });
        }
    }

    /// <summary>
    /// The endpoint shown in a column: prefer the console role (the one the rest of the picker
    /// displays through <c>GetUsedDevice</c>), falling back to the other roles Windows may have persisted.
    /// </summary>
    private static string GetPreferredDeviceId(PersistedDefaultEndpointStore.Record record)
    {
        if (record.DeviceIds.TryGetValue(Audio.Manager.Interop.Enum.ERole.eConsole, out var deviceId))
            return deviceId;
        if (record.DeviceIds.TryGetValue(Audio.Manager.Interop.Enum.ERole.eMultimedia, out deviceId))
            return deviceId;
        if (record.DeviceIds.TryGetValue(Audio.Manager.Interop.Enum.ERole.eCommunications, out deviceId))
            return deviceId;
        return null;
    }

    private void UpdateGrid()
    {
        dgvProcesses.Rows.Clear();
        var filter = txtFilter.Text.ToLower();

        AddGroupHeader(SettingsStrings.appSoundLock_picker_hasOwnDevice, isToggle: false);
        AddRows(FindRows(PickerGroup.HasOwnDevice, filter));
        AddRows(FindRows(_notRunningApps, filter));
        AddGroupHeader(SettingsStrings.appSoundLock_picker_usingAudioNow, isToggle: false);
        AddRows(FindRows(PickerGroup.UsingAudioNow, filter));

        // The toggle band carries a disclosure chevron and the number of rows the
        // third group adds, e.g. "▸ Show all processes (247)". The count is pure
        // presentation, composed in code like the rule list's "Hotkey (3)".
        var allProcesses = FindRows(PickerGroup.AllProcesses, filter).ToList();
        var chevron = _showAllProcesses ? "▾" : "▸";
        AddGroupHeader($"{chevron} {SettingsStrings.appSoundLock_picker_showAllProcesses} ({allProcesses.Count})", isToggle: true);
        if (_showAllProcesses)
        {
            AddRows(allProcesses);
        }
    }

    private IEnumerable<ProcessInfo> FindRows(PickerGroup group, string filter)
    {
        return FindRows(_allProcesses.Where(info => info.Group == group), filter);
    }

    private static IEnumerable<ProcessInfo> FindRows(IEnumerable<ProcessInfo> processes, string filter)
    {
        return processes.Where(x => x.Name.ToLower().Contains(filter)
                                    || x.Path.ToLower().Contains(filter)
                                    || x.WindowTitle.ToLower().Contains(filter));
    }

    private void AddRows(IEnumerable<ProcessInfo> rows)
    {
        // Sorting must not depend on process-enumeration order (design doc §5)
        foreach (var info in rows.OrderBy(x => x.Name))
        {
            var name = info.Name;
            if (info.IsNotRunning)
                name += " " + SettingsStrings.appSoundLock_picker_notRunningSuffix;

            var rowIndex = dgvProcesses.Rows.Add(info.Icon, name, info.WindowTitle, info.OutputDevice, info.InputDevice, info.Path);
            var row = dgvProcesses.Rows[rowIndex];
            row.Tag = info;
            StyleRow(row);
        }
    }

    private void AddGroupHeader(string text, bool isToggle)
    {
        var rowIndex = dgvProcesses.Rows.Add();
        var row = dgvProcesses.Rows[rowIndex];
        // The first column is the icon column: a text value there would raise a
        // DataGridView DataError (formatting a string as an image) when the row is
        // painted. The label therefore goes into the process-name column.
        row.Cells[1].Value = text;
        row.Cells[2].Value = string.Empty;
        row.Cells[3].Value = string.Empty;
        row.Cells[4].Value = string.Empty;
        row.Cells[5].Value = string.Empty;
        row.Tag = new GroupHeaderInfo {Text = text, IsToggle = isToggle};
        StyleRow(row);
    }

    private void TxtFilter_TextChanged(object sender, EventArgs e)
    {
        // The third group stays collapsed until the filter gives a reason to show
        // it: any filter expands it so its matches are visible; clearing returns
        // to the state the toggle row controls (manual expansions stick).
        if (txtFilter.Text.Length > 0)
        {
            _showAllProcesses = true;
        }
        else
        {
            _showAllProcesses = _allProcessesExpandedManually;
        }

        UpdateGrid();
    }

    private void DgvProcesses_CellClick(object sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0 || dgvProcesses.Rows[e.RowIndex].Tag is not GroupHeaderInfo {IsToggle: true})
            return;

        _showAllProcesses = !_showAllProcesses;
        _allProcessesExpandedManually = _showAllProcesses;
        UpdateGrid();
    }

    private void BtnSelect_Click(object sender, EventArgs e)
    {
        if (dgvProcesses.CurrentCell != null)
        {
            var info = dgvProcesses.Rows[dgvProcesses.CurrentCell.RowIndex].Tag as ProcessInfo;
            if (info == null)
                return; // group-header or toggle rows don't carry a selectable process

            SelectedProcessName = info.Name;
            SelectedProcessPath = info.Path;
            SelectedWindowTitle = info.WindowTitle;

            DialogResult = DialogResult.OK;
            Close();
        }
    }

    private void DgvProcesses_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex >= 0)
        {
            BtnSelect_Click(sender, e);
        }
    }

    private class ProcessInfo
    {
        public string Name { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public System.Drawing.Image Icon { get; set; }
        public string WindowTitle { get; set; } = string.Empty;
        public string OutputDevice { get; set; } = string.Empty;
        public string InputDevice { get; set; } = string.Empty;
        public PickerGroup Group { get; set; }
        public bool IsNotRunning { get; set; }
    }

    private class GroupHeaderInfo
    {
        public string Text { get; set; } = string.Empty;
        public bool IsToggle { get; set; }
    }
}
