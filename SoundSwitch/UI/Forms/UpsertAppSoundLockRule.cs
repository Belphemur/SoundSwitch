using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

using SoundSwitch.Common.Framework.Audio.Device;
using SoundSwitch.Localization;
using SoundSwitch.Model;
using SoundSwitch.UI.Component;

namespace SoundSwitch.UI.Forms;

public partial class UpsertAppSoundLockRule : Form
{
    private readonly AppSoundRule _rule;
    private readonly bool _editing;
    private TextBox _txtProcessPath;
    private CheckBox _chkCaseSensitive;

    public AppSoundRule Rule => _rule;

    public UpsertAppSoundLockRule(AppSoundRule rule, IEnumerable<DeviceFullInfo> playbacks, IEnumerable<DeviceFullInfo> recordings, bool editing = false)
    {
        _rule = editing ? rule.Copy() : rule;
        _editing = editing;
        InitializeComponent();
        LocalizeForm();
        InitComboBoxes(playbacks, recordings);
        InitializeFromRule();
    }

    private void LocalizeForm()
    {
        Text = _editing ? SettingsStrings.appSoundLock_editRule : SettingsStrings.appSoundLock_addRule;
        lblMatchMode.Text = SettingsStrings.appSoundLock_rule_processPath;
        lblPattern.Text = SettingsStrings.appSoundLock_rule_windowTitle;
        lblRegexNotice.Text = SettingsStrings.appSoundLock_rule_regexNotice;
        lblPlayback.Text = SettingsStrings.playback;
        lblRecording.Text = SettingsStrings.recording;
        chkEnabled.Text = SettingsStrings.appSoundLock_rule_enabled;
        chkNotify.Text = SettingsStrings.appSoundLock_rule_notify;
        btnSave.Text = SettingsStrings.buttonSave;
        btnCancel.Text = SettingsStrings.buttonClose;

        // Repurpose cmbMatchMode by hiding it and adding a TextBox in its place
        cmbMatchMode.Visible = false;
        _txtProcessPath = new TextBox
        {
            Location = cmbMatchMode.Location,
            Size = cmbMatchMode.Size,
            Anchor = cmbMatchMode.Anchor
        };
        Controls.Add(_txtProcessPath);
        
        // Add CaseSensitive checkbox
        _chkCaseSensitive = new CheckBox
        {
            Text = SettingsStrings.appSoundLock_rule_caseSensitive,
            Location = new System.Drawing.Point(chkNotify.Right + 20, chkEnabled.Top),
            AutoSize = true
        };
        Controls.Add(_chkCaseSensitive);
        
        btnSelectProcess.Location = new System.Drawing.Point(btnSelectProcess.Left, _txtProcessPath.Top);
    }

    private void InitComboBoxes(IEnumerable<DeviceFullInfo> playbacks, IEnumerable<DeviceFullInfo> recordings)
    {

        var playbackItems = playbacks
            .OrderBy(info => info.State)
            .ThenBy(info => info.NameClean)
            .Select(info => new IconTextComboBox.DropDownItem
            {
                IconHandle = info.SmallIcon,
                Tag = info.Id,
                Text = info.NameClean
            })
            .ToArray();

        var recordingItems = recordings
            .OrderBy(info => info.State)
            .ThenBy(info => info.NameClean)
            .Select(info => new IconTextComboBox.DropDownItem
            {
                IconHandle = info.SmallIcon,
                Tag = info.Id,
                Text = info.NameClean
            })
            .ToArray();

        cmbPlayback.DataSource = playbackItems;
        cmbRecording.DataSource = recordingItems;
    }

    private void InitializeFromRule()
    {
        _txtProcessPath.Text = _rule.ProcessPath;
        txtPattern.Text = _rule.WindowName;
        _chkCaseSensitive.Checked = _rule.CaseSensitive;
        chkEnabled.Checked = _rule.Enabled;
        chkNotify.Checked = _rule.Notify;

        if (!string.IsNullOrEmpty(_rule.PlaybackDeviceId))
            cmbPlayback.SelectedValue = _rule.PlaybackDeviceId;
        else
            cmbPlayback.SelectedIndex = -1;

        if (!string.IsNullOrEmpty(_rule.RecordingDeviceId))
            cmbRecording.SelectedValue = _rule.RecordingDeviceId;
        else
            cmbRecording.SelectedIndex = -1;

        btnPlaybackReset.Visible = cmbPlayback.SelectedIndex != -1;
        btnRecordingReset.Visible = cmbRecording.SelectedIndex != -1;
    }

    private void CmbPlayback_SelectedIndexChanged(object sender, EventArgs e)
    {
        btnPlaybackReset.Visible = cmbPlayback.SelectedIndex != -1;
    }

    private void CmbRecording_SelectedIndexChanged(object sender, EventArgs e)
    {
        btnRecordingReset.Visible = cmbRecording.SelectedIndex != -1;
    }

    private void BtnPlaybackReset_Click(object sender, EventArgs e)
    {
        cmbPlayback.SelectedIndex = -1;
    }

    private void BtnRecordingReset_Click(object sender, EventArgs e)
    {
        cmbRecording.SelectedIndex = -1;
    }

    private void BtnSelectProcess_Click(object sender, EventArgs e)
    {
        using var form = new ProcessSelectionForm();
        if (form.ShowDialog(this) == DialogResult.OK)
        {
            // Always pre-fill all patterns
            if (!string.IsNullOrEmpty(form.SelectedProcessPath))
            {
                var fileName = System.IO.Path.GetFileName(form.SelectedProcessPath);
                _txtProcessPath.Text = $".*{Regex.Escape(fileName)}.*";
            }
            else if (!string.IsNullOrEmpty(form.SelectedProcessName))
            {
                var name = form.SelectedProcessName;
                if (!name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)) name += ".exe";
                _txtProcessPath.Text = $".*{Regex.Escape(name)}.*";
            }

            if (!string.IsNullOrEmpty(form.SelectedWindowTitle))
            {
                txtPattern.Text = $".*{Regex.Escape(form.SelectedWindowTitle)}.*";
            }
        }
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        if (!_editing)
        {
            BtnSelectProcess_Click(this, EventArgs.Empty);
        }
    }

    private void BtnSave_Click(object sender, EventArgs e)
    {
        _rule.ProcessPath = _txtProcessPath.Text;
        _rule.WindowName = txtPattern.Text;
        _rule.CaseSensitive = _chkCaseSensitive.Checked;
        _rule.Enabled = chkEnabled.Checked;
        _rule.Notify = chkNotify.Checked;
        _rule.PlaybackDeviceId = cmbPlayback.SelectedValue?.ToString();
        _rule.RecordingDeviceId = cmbRecording.SelectedValue?.ToString();

        if (string.IsNullOrWhiteSpace(_rule.ProcessPath) && string.IsNullOrWhiteSpace(_rule.WindowName))
        {
            MessageBox.Show("At least one pattern (Process or Window) is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        DialogResult = DialogResult.OK;
        Close();
    }
}
