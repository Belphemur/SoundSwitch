using Serilog;
using SoundSwitch.Audio.Manager;
using SoundSwitch.Common.Framework.Icon;
using SoundSwitch.Framework.WinApi;
using SoundSwitch.Localization;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using System;

namespace SoundSwitch.UI.Forms;

public partial class ProcessSelectionForm : Form
{
    public string SelectedProcessName { get; private set; } = string.Empty;
    public string SelectedProcessPath { get; private set; } = string.Empty;
    public string SelectedWindowTitle { get; private set; } = string.Empty;

    private static readonly ILogger _logger = Log.ForContext<ProcessSelectionForm>();
    private List<ProcessInfo> _allProcesses;

    public ProcessSelectionForm()
    {
        InitializeComponent();
        MinimumSize = new System.Drawing.Size(600, 400);
        LocalizeForm();
        LoadProcesses();
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
        dgvProcesses.Columns.Add(colIcon);
        dgvProcesses.Columns.Add("colName", SettingsStrings.appSoundLock_rule_processName);
        dgvProcesses.Columns.Add("colTitle", SettingsStrings.appSoundLock_rule_windowTitle);
        dgvProcesses.Columns.Add("colOutput", SettingsStrings.playback);
        dgvProcesses.Columns.Add("colInput", SettingsStrings.recording);
        dgvProcesses.Columns.Add("colPath", SettingsStrings.appSoundLock_rule_processPath);
        
        dgvProcesses.AllowUserToResizeRows = false;
        dgvProcesses.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgvProcesses.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        if (dgvProcesses.Columns.Contains("colIcon")) dgvProcesses.Columns["colIcon"].Width = 32;
    }

    private void LoadProcesses()
    {
        _allProcesses = [];
        var audioSwitcher = AudioSwitcher.Instance;

        var playbackMap = audioSwitcher.GetProcessDeviceMap(Audio.Manager.Interop.Enum.EDataFlow.eRender);
        var recordingMap = audioSwitcher.GetProcessDeviceMap(Audio.Manager.Interop.Enum.EDataFlow.eCapture);

        var defaultPlayback = audioSwitcher.GetDefaultAudioEndpoint(Audio.Manager.Interop.Enum.EDataFlow.eRender, Audio.Manager.Interop.Enum.ERole.eConsole);
        var defaultRecording = audioSwitcher.GetDefaultAudioEndpoint(Audio.Manager.Interop.Enum.EDataFlow.eCapture, Audio.Manager.Interop.Enum.ERole.eConsole);

        var defaultPlaybackName = defaultPlayback?.NameClean ?? "Default";
        var defaultRecordingName = defaultRecording?.NameClean ?? "Default";

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
                        InputDevice = inputName
                    });
                }
                catch (Exception ex)
                {
                    _logger.Debug(ex, "Failed to load individual process info for PID {PID}", p.Id);
                }
            }
        }

        // Only show one row per process/path/window combination
        _allProcesses = [.. _allProcesses
            .GroupBy(x => new { x.Name, x.Path, x.WindowTitle })
            .Select(g => g.First())];

        UpdateGrid(_allProcesses);
    }

    private void UpdateGrid(IEnumerable<ProcessInfo> processes)
    {
        dgvProcesses.Rows.Clear();
        foreach (var info in processes.OrderBy(x => x.Name))
        {
            var rowIndex = dgvProcesses.Rows.Add(info.Icon, info.Name, info.WindowTitle, info.OutputDevice, info.InputDevice, info.Path);
            dgvProcesses.Rows[rowIndex].Tag = info;
        }
    }

    private void TxtFilter_TextChanged(object sender, EventArgs e)
    {
        var filter = txtFilter.Text.ToLower();
        var filtered = _allProcesses
            .Where(x => x.Name.ToLower().Contains(filter) || x.Path.ToLower().Contains(filter) || x.WindowTitle.ToLower().Contains(filter));
        
        UpdateGrid(filtered);
    }

    private void BtnSelect_Click(object sender, EventArgs e)
    {
        if (dgvProcesses.CurrentCell != null)
        {
            var row = dgvProcesses.Rows[dgvProcesses.CurrentCell.RowIndex];
            var info = (ProcessInfo)row.Tag;
            
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
    }
}
