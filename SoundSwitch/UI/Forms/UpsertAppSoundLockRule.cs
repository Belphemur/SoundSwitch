using System;
using System.Collections.Generic;
using System.Drawing;
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
    private readonly List<DeviceFullInfo> _playbacks;
    private readonly List<DeviceFullInfo> _recordings;
    private readonly List<AppSoundRuleDevicePair> _pairs;
    private int _selectedPairIndex = -1;
    private bool _initializing;
    private bool _handlingModeChange;

    public AppSoundRule Rule => _rule;

    public UpsertAppSoundLockRule(AppSoundRule rule, IEnumerable<DeviceFullInfo> playbacks, IEnumerable<DeviceFullInfo> recordings, bool editing = false)
    {
        _initializing = true;
        _rule = editing ? rule.Copy() : rule;
        _editing = editing;
        _playbacks = playbacks.ToList();
        _recordings = recordings.ToList();
        _pairs = _rule.DevicePairs?.Select(pair => pair.Copy()).ToList() ?? new List<AppSoundRuleDevicePair>();

        InitializeComponent();
        LocalizeForm();
        InitComboBoxes();
        InitializeFromRule();

        hotKeyBox.ListenToHotkey = true;
        hotKeyBox.HotKeyChanged += (_, _) => btnHotKeyReset.Visible = hotKeyBox.HotKey != null;

        _initializing = false;
        ApplyModeLayout();
    }

    private void LocalizeForm()
    {
        Text = _editing ? SettingsStrings.appSoundLock_editRule : SettingsStrings.appSoundLock_addRule;
        lblProcessPath.Text = SettingsStrings.appSoundLock_rule_processPath;
        lblPattern.Text = SettingsStrings.appSoundLock_rule_windowTitle;
        lblRegexNotice.Text = SettingsStrings.appSoundLock_rule_regexNotice;
        lblMode.Text = SettingsStrings.appSoundLock_rule_mode;
        rdoAutomatic.Text = SettingsStrings.appSoundLock_rule_mode_automatic;
        rdoHotkey.Text = SettingsStrings.appSoundLock_rule_mode_hotkey;
        lblPlayback.Text = SettingsStrings.playback;
        lblRecording.Text = SettingsStrings.recording;
        lblHotKey.Text = SettingsStrings.appSoundLock_rule_shortcut;
        grpHotkey.Text = SettingsStrings.hotkey;
        chkEnabled.Text = SettingsStrings.appSoundLock_rule_enabled;
        chkCaseSensitive.Text = SettingsStrings.appSoundLock_rule_caseSensitive;
        chkNotify.Text = SettingsStrings.appSoundLock_rule_notify;
        btnNewPair.Text = SettingsStrings.appSoundLock_rule_newPair;
        btnRemovePair.Text = SettingsStrings.appSoundLock_rule_removePair;
        btnMoveUp.Text = SettingsStrings.appSoundLock_rule_moveUp;
        btnMoveDown.Text = SettingsStrings.appSoundLock_rule_moveDown;
        btnSave.Text = SettingsStrings.buttonSave;
        btnCancel.Text = SettingsStrings.buttonClose;
    }

    private void InitComboBoxes()
    {
        cmbPlayback.DataSource = BuildDeviceItems(_playbacks);
        cmbRecording.DataSource = BuildDeviceItems(_recordings);
        cmbPairPlayback.DataSource = BuildPairItems(_playbacks);
        cmbPairRecording.DataSource = BuildPairItems(_recordings);
    }

    private static IconTextComboBox.DropDownItem[] BuildDeviceItems(IEnumerable<DeviceFullInfo> devices)
    {
        return devices
            .OrderBy(info => info.State)
            .ThenBy(info => info.NameClean)
            .Select(info => new IconTextComboBox.DropDownItem
            {
                IconHandle = info.SmallIcon,
                Tag = info.Id,
                Text = info.NameClean
            })
            .ToArray();
    }

    private static IconTextComboBox.DropDownItem[] BuildPairItems(IEnumerable<DeviceFullInfo> devices)
    {
        var keepCurrent = new IconTextComboBox.DropDownItem
        {
            // An empty string (not null) keeps the item selectable with ValueMember:
            // a null value makes the data-bound ComboBox fall back to no selection.
            Tag = string.Empty,
            Text = SettingsStrings.appSoundLock_rule_keepCurrent
        };

        return new[] { keepCurrent }.Concat(BuildDeviceItems(devices)).ToArray();
    }

    private void InitializeFromRule()
    {
        txtProcessPath.Text = _rule.ProcessPath;
        txtPattern.Text = _rule.WindowName;
        chkCaseSensitive.Checked = _rule.CaseSensitive;
        chkEnabled.Checked = _rule.Enabled;
        chkNotify.Checked = _rule.Notify;
        hotKeyBox.HotKey = _rule.HotKey;

        var firstPair = _pairs.FirstOrDefault();
        SelectDevice(cmbPlayback, firstPair?.Playback, _playbacks);
        SelectDevice(cmbRecording, firstPair?.Recording, _recordings);

        btnPlaybackReset.Visible = cmbPlayback.SelectedIndex != -1;
        btnRecordingReset.Visible = cmbRecording.SelectedIndex != -1;
        btnHotKeyReset.Visible = _rule.HotKey != null;

        rdoAutomatic.Checked = _rule.Mode != AppRuleMode.Hotkey;
        rdoHotkey.Checked = _rule.Mode == AppRuleMode.Hotkey;

        PopulatePairList();
        if (_pairs.Count > 0)
        {
            SelectPair(0);
        }
        else
        {
            ResetPairBuilder();
        }
    }

    private void ModeChanged(object sender, EventArgs e)
    {
        if (_initializing || _handlingModeChange) return;

        _handlingModeChange = true;
        try
        {
            if (rdoHotkey.Checked)
            {
                // Automatic -> Hotkey: seed the pair list from the single-device combos.
                var seededPair = BuildPairFromCombo(cmbPlayback, cmbRecording, _playbacks, _recordings);
                if (_pairs.Count == 0)
                {
                    if (seededPair != null) _pairs.Add(seededPair);
                }
                else if (_pairs.Count == 1 && seededPair != null)
                {
                    _pairs[0] = seededPair;
                }

                PopulatePairList();
                if (_pairs.Count > 0)
                {
                    SelectPair(0);
                }
                else
                {
                    ResetPairBuilder();
                }
            }
            else
            {
                // Hotkey -> Automatic: Automatic only uses the first pair.
                if (_pairs.Count > 1)
                {
                    var confirm = MessageBox.Show(
                        SettingsStrings.appSoundLock_rule_confirm_trimPairs,
                        SettingsStrings.appSoundLock_rule_mode,
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);
                    if (confirm != DialogResult.Yes)
                    {
                        rdoHotkey.Checked = true;
                        return;
                    }

                    var first = _pairs[0];
                    _pairs.Clear();
                    _pairs.Add(first);
                    PopulatePairList();
                }

                var firstPair = _pairs.FirstOrDefault();
                if (firstPair != null)
                {
                    SelectDevice(cmbPlayback, firstPair.Playback, _playbacks);
                    SelectDevice(cmbRecording, firstPair.Recording, _recordings);
                }
            }
        }
        finally
        {
            _handlingModeChange = false;
        }

        ApplyModeLayout();
    }

    private void ApplyModeLayout()
    {
        var hotkeyMode = rdoHotkey.Checked;
        lblDescription.Text = hotkeyMode
            ? SettingsStrings.appSoundLock_rule_description_hotkey
            : SettingsStrings.appSoundLock_rule_description;

        lblPlayback.Visible = !hotkeyMode;
        cmbPlayback.Visible = !hotkeyMode;
        btnPlaybackReset.Visible = !hotkeyMode && cmbPlayback.SelectedIndex != -1;
        lblRecording.Visible = !hotkeyMode;
        cmbRecording.Visible = !hotkeyMode;
        btnRecordingReset.Visible = !hotkeyMode && cmbRecording.SelectedIndex != -1;
        grpHotkey.Visible = hotkeyMode;

        var checkboxY = hotkeyMode ? 398 : 240;
        chkEnabled.Location = new Point(110, checkboxY);
        chkCaseSensitive.Location = new Point(210, checkboxY);
        chkNotify.Location = new Point(340, checkboxY);

        var buttonY = hotkeyMode ? 430 : 272;
        btnSave.Location = new Point(378, buttonY);
        btnCancel.Location = new Point(459, buttonY);

        ClientSize = new Size(560, hotkeyMode ? 470 : 310);
    }

    private void PopulatePairList()
    {
        lvPairs.BeginUpdate();
        try
        {
            lvPairs.Items.Clear();
            lvPairs.Columns.Clear();
            lvPairs.Columns.Add("#", 30);
            lvPairs.Columns.Add(SettingsStrings.playback, 235);
            lvPairs.Columns.Add(SettingsStrings.recording, 235);

            for (var index = 0; index < _pairs.Count; index++)
            {
                var pair = _pairs[index];
                var item = new ListViewItem((index + 1).ToString()) { Tag = pair };

                var playbackItem = item.SubItems.Add(DeviceLabel(pair.Playback));
                playbackItem.Tag = FindDevice(pair.Playback, _playbacks)?.SmallIcon;

                var recordingItem = item.SubItems.Add(DeviceLabel(pair.Recording));
                recordingItem.Tag = FindDevice(pair.Recording, _recordings)?.SmallIcon;

                lvPairs.Items.Add(item);
            }
        }
        finally
        {
            lvPairs.EndUpdate();
        }

        UpdatePairCount();
        UpdatePairButtons();
    }

    private static string DeviceLabel(DeviceInfo? device)
    {
        return device?.NameClean ?? SettingsStrings.appSoundLock_rule_keepCurrent;
    }

    private static DeviceFullInfo? FindDevice(DeviceInfo? stored, IEnumerable<DeviceFullInfo> devices)
    {
        if (stored == null) return null;
        return devices.FirstOrDefault(d => d.Id == stored.Id) ?? devices.FirstOrDefault(d => d.Equals(stored));
    }

    private void UpdatePairCount()
    {
        lblPairCount.Visible = _pairs.Count > 0;
        lblPairCount.Text = string.Format(
            _pairs.Count == 1 ? SettingsStrings.appSoundLock_rule_pairCount_one : SettingsStrings.appSoundLock_rule_pairCount,
            _pairs.Count);
    }

    private void UpdatePairButtons()
    {
        var hasSelection = _selectedPairIndex >= 0 && _selectedPairIndex < _pairs.Count;
        btnAddPair.Text = hasSelection
            ? SettingsStrings.appSoundLock_rule_updatePair
            : SettingsStrings.appSoundLock_rule_addPair;
        btnNewPair.Enabled = hasSelection;
        btnRemovePair.Enabled = hasSelection;
        btnMoveUp.Enabled = hasSelection && _selectedPairIndex > 0;
        btnMoveDown.Enabled = hasSelection && _selectedPairIndex < _pairs.Count - 1;
    }

    private void SelectPair(int index)
    {
        if (index < 0 || index >= lvPairs.Items.Count) return;
        lvPairs.SelectedItems.Clear();
        lvPairs.Items[index].Selected = true;
        lvPairs.Items[index].Focused = true;
        lvPairs.EnsureVisible(index);
    }

    private void LvPairs_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (lvPairs.SelectedIndices.Count == 0)
        {
            _selectedPairIndex = -1;
            UpdatePairButtons();
            return;
        }

        _selectedPairIndex = lvPairs.SelectedIndices[0];
        var pair = _pairs[_selectedPairIndex];
        SelectPairDevice(cmbPairPlayback, pair.Playback, _playbacks);
        SelectPairDevice(cmbPairRecording, pair.Recording, _recordings);
        UpdatePairButtons();
    }

    private void ResetPairBuilder()
    {
        cmbPairPlayback.SelectedIndex = 0;
        cmbPairRecording.SelectedIndex = 0;
    }

    private void BtnAddPair_Click(object sender, EventArgs e)
    {
        var pair = BuildPairFromCombo(cmbPairPlayback, cmbPairRecording, _playbacks, _recordings);
        if (pair == null)
        {
            MessageBox.Show(SettingsStrings.appSoundLock_rule_error_pairIncomplete, SettingsStrings.appSoundLock_rule_error_title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (_selectedPairIndex >= 0 && _selectedPairIndex < _pairs.Count)
        {
            var index = _selectedPairIndex;
            _pairs[index] = pair;
            PopulatePairList();
            SelectPair(index);
            return;
        }

        _pairs.Add(pair);
        PopulatePairList();
        SelectPair(_pairs.Count - 1);
    }

    private void BtnNewPair_Click(object sender, EventArgs e)
    {
        _selectedPairIndex = -1;
        lvPairs.SelectedItems.Clear();
        ResetPairBuilder();
        UpdatePairButtons();
    }

    private void BtnRemovePair_Click(object sender, EventArgs e)
    {
        if (_selectedPairIndex < 0 || _selectedPairIndex >= _pairs.Count) return;

        var index = _selectedPairIndex;
        _pairs.RemoveAt(index);
        _selectedPairIndex = -1;
        PopulatePairList();

        if (_pairs.Count == 0)
        {
            ResetPairBuilder();
            return;
        }

        SelectPair(Math.Min(index, _pairs.Count - 1));
    }

    private void BtnMoveUp_Click(object sender, EventArgs e)
    {
        var index = _selectedPairIndex;
        if (index <= 0 || index >= _pairs.Count) return;

        (_pairs[index - 1], _pairs[index]) = (_pairs[index], _pairs[index - 1]);
        PopulatePairList();
        SelectPair(index - 1);
    }

    private void BtnMoveDown_Click(object sender, EventArgs e)
    {
        var index = _selectedPairIndex;
        if (index < 0 || index >= _pairs.Count - 1) return;

        (_pairs[index + 1], _pairs[index]) = (_pairs[index], _pairs[index + 1]);
        PopulatePairList();
        SelectPair(index + 1);
    }

    private void BtnHotKeyReset_Click(object sender, EventArgs e)
    {
        hotKeyBox.HotKey = null;
        btnHotKeyReset.Visible = false;
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

    private void SelectProcess()
    {
        using var form = new ProcessSelectionForm();
        if (form.ShowDialog(this) == DialogResult.OK)
        {
            // Always pre-fill all patterns
            if (!string.IsNullOrEmpty(form.SelectedProcessPath))
            {
                var fileName = System.IO.Path.GetFileName(form.SelectedProcessPath);
                txtProcessPath.Text = $".*{Regex.Escape(fileName)}.*";
            }
            else if (!string.IsNullOrEmpty(form.SelectedProcessName))
            {
                var name = form.SelectedProcessName;
                if (!name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)) name += ".exe";
                txtProcessPath.Text = $".*{Regex.Escape(name)}.*";
            }

            if (!string.IsNullOrEmpty(form.SelectedWindowTitle))
            {
                txtPattern.Text = $".*{Regex.Escape(form.SelectedWindowTitle)}.*";
            }
        }
    }

    private void BtnSelectProcess_Click(object sender, EventArgs e) => SelectProcess();

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        if (!_editing)
        {
            BeginInvoke(new Action(SelectProcess));
        }
    }

    /// <summary>
    /// Preselect the combo box for a stored device: exact id match first, then
    /// <see cref="DeviceInfo.Equals(DeviceInfo)"/> (NameClean) match — this survives device id changes.
    /// When nothing matches, the combo is left unselected; the user re-picks the device once.
    /// </summary>
    private static void SelectDevice(ComboBox comboBox, DeviceInfo? stored, IEnumerable<DeviceFullInfo> devices)
    {
        if (stored == null)
        {
            comboBox.SelectedIndex = -1;
            return;
        }

        var match = FindDevice(stored, devices);
        if (match == null)
        {
            comboBox.SelectedIndex = -1;
            return;
        }

        comboBox.SelectedValue = match.Id;
    }

    /// <summary>
    /// Select a device in a pair-builder combo. Unlike the Automatic combos, the first
    /// item is "Keep current", so a null device selects index 0 instead of clearing the combo.
    /// </summary>
    private static void SelectPairDevice(ComboBox comboBox, DeviceInfo? stored, IEnumerable<DeviceFullInfo> devices)
    {
        var match = FindDevice(stored, devices);
        comboBox.SelectedValue = match?.Id ?? string.Empty;
        if (comboBox.SelectedIndex == -1) comboBox.SelectedIndex = 0;
    }

    /// <summary>
    /// Build the <see cref="DeviceInfo"/> to persist from the device selected in the combo box.
    /// </summary>
    private static DeviceInfo? GetSelectedDevice(ComboBox comboBox, IEnumerable<DeviceFullInfo> devices)
    {
        if (comboBox.SelectedValue is not string id || string.IsNullOrEmpty(id)) return null;

        var info = devices.FirstOrDefault(d => d.Id == id);
        if (info == null) return null;

#pragma warning disable CS0618 // Type or member is obsolete
        return new DeviceInfo(info.Name, info.Id, info.Type, info.IsUsb, DateTime.UtcNow);
#pragma warning restore CS0618 // Type or member is obsolete
    }

    private static AppSoundRuleDevicePair? BuildPairFromCombo(ComboBox playback, ComboBox recording, IEnumerable<DeviceFullInfo> playbacks, IEnumerable<DeviceFullInfo> recordings)
    {
        var pair = new AppSoundRuleDevicePair
        {
            Playback = GetSelectedDevice(playback, playbacks),
            Recording = GetSelectedDevice(recording, recordings)
        };

        return pair.HasAnyDevice ? pair : null;
    }

    private static bool SamePair(AppSoundRuleDevicePair left, AppSoundRuleDevicePair right)
    {
        return left.Playback?.Id == right.Playback?.Id && left.Recording?.Id == right.Recording?.Id;
    }

    private void BtnSave_Click(object sender, EventArgs e)
    {
        _rule.ProcessPath = txtProcessPath.Text;
        _rule.WindowName = txtPattern.Text;
        _rule.CaseSensitive = chkCaseSensitive.Checked;
        _rule.Enabled = chkEnabled.Checked;
        _rule.Notify = chkNotify.Checked;

        if (string.IsNullOrWhiteSpace(_rule.ProcessPath) && string.IsNullOrWhiteSpace(_rule.WindowName))
        {
            MessageBox.Show(SettingsStrings.appSoundLock_rule_error_patternRequired, SettingsStrings.appSoundLock_rule_error_title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (rdoAutomatic.Checked)
        {
            var pair = BuildPairFromCombo(cmbPlayback, cmbRecording, _playbacks, _recordings);
            if (pair == null)
            {
                MessageBox.Show(SettingsStrings.appSoundLock_rule_error_pairRequired, SettingsStrings.appSoundLock_rule_error_title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _rule.Mode = AppRuleMode.Automatic;
            _rule.HotKey = null;
            _rule.DevicePairs = new List<AppSoundRuleDevicePair> { pair };
        }
        else
        {
            // Apply a builder edit that wasn't confirmed with the Add/Update button.
            var builderPair = BuildPairFromCombo(cmbPairPlayback, cmbPairRecording, _playbacks, _recordings);
            if (_selectedPairIndex >= 0 && _selectedPairIndex < _pairs.Count && builderPair != null)
            {
                _pairs[_selectedPairIndex] = builderPair;
            }
            else if (builderPair != null && !_pairs.Any(pair => SamePair(pair, builderPair)))
            {
                _pairs.Add(builderPair);
            }

            if (hotKeyBox.HotKey == null)
            {
                MessageBox.Show(SettingsStrings.appSoundLock_rule_error_hotkeyRequired, SettingsStrings.appSoundLock_rule_error_title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!AppModel.Instance.AppSoundLockManager.IsHotkeyAvailable(hotKeyBox.HotKey, _rule.Id))
            {
                MessageBox.Show(SettingsStrings.appSoundLock_rule_error_hotkeyInUse, SettingsStrings.appSoundLock_rule_error_title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_pairs.Count == 0)
            {
                MessageBox.Show(SettingsStrings.appSoundLock_rule_error_pairRequired, SettingsStrings.appSoundLock_rule_error_title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_pairs.Any(pair => !pair.HasAnyDevice))
            {
                MessageBox.Show(SettingsStrings.appSoundLock_rule_error_pairIncomplete, SettingsStrings.appSoundLock_rule_error_title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _rule.Mode = AppRuleMode.Hotkey;
            _rule.HotKey = hotKeyBox.HotKey;
            _rule.DevicePairs = _pairs.Select(pair => pair.Copy()).ToList();

            if (!AppModel.Instance.AppSoundLockManager.IsHotkeyTargetAvailable(_rule))
            {
                MessageBox.Show(SettingsStrings.appSoundLock_rule_error_duplicateTarget, SettingsStrings.appSoundLock_rule_error_title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        DialogResult = DialogResult.OK;
        Close();
    }
}
