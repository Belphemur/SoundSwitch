using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

using SoundSwitch.Common.Framework.Audio.Device;
using SoundSwitch.Localization;
using SoundSwitch.Model;
using SoundSwitch.Services;
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
    private bool _formLoaded;
    private bool _loadingPairBuilder;
    private bool _pairPlaybackDirty;
    private bool _pairRecordingDirty;

    /// <summary>
    /// The concrete exe identity the process picker last returned (a full path, or the NT device
    /// path of a not-running store row). Assignment rules persist it as their identity; dynamic
    /// rules only need its basename wrapped in a regex pattern.
    /// </summary>
    private string? _pickedProcessPath;

    public AppSoundRule Rule => _rule;

    // Designer fields are private; these expose the pieces SoundSwitch.Tests drives to assert the
    // layout and accessibility contract (see UpsertAppSoundLockRuleLayoutTests).
    internal RadioButton AutomaticModeRadio => rdoAutomatic;
    internal RadioButton HotkeyModeRadio => rdoHotkey;
    // "CancelButton" is taken by Form.CancelButton, which the designer assigns.
    internal Button CancelButtonControl => btnCancel;
    internal GroupBox HotkeyGroup => grpHotkey;
    internal Button SelectProcessButton => btnSelectProcess;
    internal Button PlaybackResetButton => btnPlaybackReset;
    internal Button RecordingResetButton => btnRecordingReset;
    internal Button HotKeyResetButton => btnHotKeyReset;
    internal ListView PairList => lvPairs;
    internal Button NewPairButton => btnNewPair;
    internal Button RemovePairButton => btnRemovePair;
    internal CheckBox EnabledCheckbox => chkEnabled;
    internal Label AssignmentHelperLabel => lblAssignmentHelper;

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
        InitAccessibleNames();
        InitComboBoxes();
        InitializeFromRule();

        hotKeyBox.ListenToHotkey = true;
        hotKeyBox.HotKeyChanged += (_, _) => btnHotKeyReset.Visible = hotKeyBox.HotKey != null;
        lvPairs.Resize += (_, _) => RedistributePairColumns();

        _initializing = false;
        UpdateAssignmentNote();
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

        lblAssignmentHelper.Text = SettingsStrings.appSoundLock_rule_assignmentNote;
    }

    private void InitAccessibleNames()
    {
        // Image-only controls have no visible label a screen reader could read out.
        btnSelectProcess.AccessibleName = SettingsStrings.appSoundLock_rule_accessible_selectProcess;
        btnPlaybackReset.AccessibleName = SettingsStrings.appSoundLock_rule_accessible_resetPlayback;
        btnRecordingReset.AccessibleName = SettingsStrings.appSoundLock_rule_accessible_resetRecording;
        btnHotKeyReset.AccessibleName = SettingsStrings.appSoundLock_rule_accessible_resetHotkey;
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

        // No enforcement control exists any more (ADR �0): every eligible rule also writes an
        // identity assignment. Only those rules carry a concrete identity, so only those show
        // it in the process field instead of their matching pattern.
        if (_rule.Enforcement == AppRuleEnforcement.Assignment)
        {
            _pickedProcessPath = _rule.AssignmentExePath ?? _rule.ProcessPath;
            // An edited assignment routes by its identity (ADR �6.1): surface that identity in
            // the process field instead of a stale display value, so an edit that only changes
            // the device cannot quietly change which executable the rule targets.
            if (!string.IsNullOrEmpty(_pickedProcessPath))
                txtProcessPath.Text = _pickedProcessPath;
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

        UpdateAssignmentNote();
        ApplyModeLayout();
    }

    internal void ApplyModeLayout()
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

        // Assignment enforcement is offered on Automatic rules only (ADR �6.4).
        lblEnforcement.Visible = false;
        pnlEnforcement.Visible = false;
        lblAssignmentHelper.Visible = lblAssignmentHelper.Visible && !hotkeyMode;

        // The design-time coordinates are set by the constructor before the form
        // is shown, and the framework scales them once with AutoScaleMode.Dpi.
        // Mode changes after the form is loaded must apply that same DPI factor
        // themselves, otherwise the moved controls land at 96-DPI, not scaled,
        // positions on a >100% display.
        var factor = _formLoaded ? FontScaleFactor() : 1f;

        // The enforcement row sits between the device combos and the checkboxes; scale it the
        // same way so a mode switch after the form is shown keeps the layout aligned.
        lblEnforcement.Location = new Point((int)Math.Round(12 * factor), (int)Math.Round(233 * factor));
        pnlEnforcement.Location = new Point((int)Math.Round(110 * factor), (int)Math.Round(228 * factor));
        pnlEnforcement.Size = new Size((int)Math.Round(438 * factor), (int)Math.Round(20 * factor));
        lblAssignmentHelper.Location = new Point((int)Math.Round(110 * factor), (int)Math.Round(252 * factor));
        lblAssignmentHelper.Size = new Size((int)Math.Round(438 * factor), (int)Math.Round(46 * factor));

        // The bottom region toggles between the device combos (Automatic) and the hotkey
        // group. ComputeModeLayout derives the client size from the design size, pins the
        // bottom rows relative to the client edges so the anchors (Bottom|Left / Bottom|Right)
        // stay coherent on further resizes, and — unless the user resized the window — drops
        // back to the design baseline when leaving Hotkey mode (its extra height is stage
        // space, not a user preference).
        var untouchedHeight = _appliedClientHeight > 0 && ClientSize.Height == _appliedClientHeight;
        var layout = ComputeModeLayout(hotkeyMode, factor, ClientSize.Width, ClientSize.Height,
            !untouchedHeight, btnCancel.Width, btnSave.Width);

        // Enforce the mode's minimum while the user resizes, not only when the mode changes:
        // in Hotkey mode the window must not shrink below the design client (the pair list
        // would collapse and overlap the pair buttons). Set it BEFORE the client size or the
        // framework clamps the client-size change against the raised minimum, keeping the
        // hotkey height when switching back to Automatic. Only after the handle exists:
        // before that there is no chrome, so the frame delta is meaningless and the
        // designer's MinimumSize stands (it scales with the designer pass).
        if (IsHandleCreated)
        {
            var frameWidth = Math.Max(0, Size.Width - ClientSize.Width);
            var frameHeight = Math.Max(0, Size.Height - ClientSize.Height);
            MinimumSize = new Size(ScaleClientWidth() + frameWidth, ScaleClientBaseHeight() + frameHeight);
        }

        ClientSize = layout.ClientSize;
        _appliedClientHeight = layout.ClientSize.Height;

        if (hotkeyMode)
        {
            grpHotkey.Location = layout.GrpHotkeyLocation;
            grpHotkey.Size = layout.GrpHotkeySize;
        }

        chkEnabled.Location = new Point(layout.CheckboxLeft, layout.CheckboxY);
        chkCaseSensitive.Location = new Point(layout.CheckboxMid, layout.CheckboxY);
        chkNotify.Location = new Point(layout.CheckboxRight, layout.CheckboxY);
        btnCancel.Location = layout.CancelButtonLocation;
        btnSave.Location = layout.SaveButtonLocation;

        int ScaleClientWidth() => (int)Math.Round(560 * factor, MidpointRounding.AwayFromZero);
        int ScaleClientBaseHeight() => (int)Math.Round((hotkeyMode ? 470 : 372) * factor, MidpointRounding.AwayFromZero);
    }

    /// <summary>The client height <see cref="ApplyModeLayout"/> last set; a current height equal to
    /// it means the user has not resized the window since (see <see cref="ComputeModeLayout"/>).</summary>
    private int _appliedClientHeight;

    /// <summary>
    /// Pure geometry for one bottom-region mode, in runtime pixels: the designer coordinates of
    /// <see cref="UpsertAppSoundLockRule"/> at their 96-DPI baseline scaled by <paramref name="factor"/>,
    /// clamped so a user-enlarged window keeps its size. The bottom rows are expressed as offsets from
    /// the client's bottom/right edges because their controls are bottom/right anchored.
    /// </summary>
    /// <param name="heightWasUserChosen">False when the current height is just the height this
    /// mode last set; the returned <see cref="AppRuleDialogLayout.ClientSize"/> then uses the
    /// mode's <paramref name="factor"/>-scaled design height instead of keeping it.</param>
    /// <remarks>
    /// Testable without a live form; see UpsertAppSoundLockRuleLayoutTests.
    /// </remarks>
    internal static AppRuleDialogLayout ComputeModeLayout(bool hotkeyMode, double factor,
        int currentClientWidth, int currentClientHeight, bool heightWasUserChosen,
        int cancelButtonWidth, int saveButtonWidth)
    {
        int Scale(int design) => (int)Math.Round(design * factor, MidpointRounding.AwayFromZero);

        var width = Math.Max(currentClientWidth, Scale(560));
        var height = Math.Max(heightWasUserChosen ? currentClientHeight : 0, Scale(hotkeyMode ? 470 : 372));

        var cancelButtonX = width - Scale(26) - cancelButtonWidth;
        var buttonsY = height - Scale(hotkeyMode ? 40 : 38);

        return new AppRuleDialogLayout(
            ClientSize: new Size(width, height),
            GrpHotkeyLocation: new Point(Scale(12), Scale(168)),
            GrpHotkeySize: new Size(width - Scale(24), height - Scale(252)),
            CheckboxY: height - Scale(hotkeyMode ? 72 : 70),
            CheckboxLeft: Scale(110),
            CheckboxMid: Scale(210),
            CheckboxRight: Scale(340),
            SaveButtonLocation: new Point(cancelButtonX - Scale(6) - saveButtonWidth, buttonsY),
            CancelButtonLocation: new Point(cancelButtonX, buttonsY));
    }

    /// <summary>The runtime-pixel result of <see cref="ComputeModeLayout"/>.</summary>
    internal sealed record AppRuleDialogLayout(
        Size ClientSize,
        Point GrpHotkeyLocation,
        Size GrpHotkeySize,
        int CheckboxY,
        int CheckboxLeft,
        int CheckboxMid,
        int CheckboxRight,
        Point SaveButtonLocation,
        Point CancelButtonLocation);

    /// <summary>
    /// Ratio between the runtime DPI scale and the form's 96-DPI design baseline. Mode switches
    /// performed after the form has been autoscaled apply it so the moved controls land at
    /// scaled, not 96-DPI, positions. It must come from <see cref="Control.DeviceDpi"/> rather
    /// than <see cref="ContainerControl.AutoScaleDimensions"/>: WinForms rewrites the auto-scale
    /// dimensions to the current ones after each autoscale pass, which would make a
    /// current-vs-design ratio read as 1 at any DPI after the form is shown.
    /// </summary>
    private float FontScaleFactor()
    {
        const float designDpi = 96f;
        return DeviceDpi <= 0 ? 1f : DeviceDpi / designDpi;
    }

    private void PopulatePairList()
    {
        lvPairs.BeginUpdate();
        try
        {
            lvPairs.Items.Clear();
            lvPairs.Columns.Clear();
            // The first column keeps a fixed index width; the two device columns follow the
            // current list width via RedistributePairColumns instead of hard 96-DPI pixels.
            // Before the form is shown the design coordinates are unscaled (the framework
            // scales them on the first autoscale pass), so the factor is only applied after load.
            lvPairs.Columns.Add("#", (int)Math.Round(30 * (_formLoaded ? FontScaleFactor() : 1f)));
            lvPairs.Columns.Add(SettingsStrings.playback, 0);
            lvPairs.Columns.Add(SettingsStrings.recording, 0);

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

        RedistributePairColumns();
        UpdatePairCount();
        UpdatePairButtons();
    }

    /// <summary>
    /// Split the space left of the fixed index column evenly between the playback and
    /// recording columns, so the pair list tracks the dialog's resizable width instead of
    /// staying at 96-DPI pixel widths.
    /// </summary>
    private void RedistributePairColumns()
    {
        if (lvPairs.Columns.Count < 3) return;

        var available = lvPairs.ClientSize.Width - lvPairs.Columns[0].Width - 4;
        if (available <= 80) return;
        var half = available / 2;
        lvPairs.Columns[1].Width = half;
        lvPairs.Columns[2].Width = available - half;
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
        LoadPairIntoBuilder(pair);
        UpdatePairButtons();
    }

    /// <summary>
    /// Load a stored pair into the builder. Programmatic loads reset the per-side
    /// dirty flags so an untouched side that is temporarily unavailable is not
    /// silently dropped on Save (see <see cref="BuildEditedPair"/>).
    /// </summary>
    private void LoadPairIntoBuilder(AppSoundRuleDevicePair pair)
    {
        _loadingPairBuilder = true;
        try
        {
            SelectPairDevice(cmbPairPlayback, pair.Playback, _playbacks);
            SelectPairDevice(cmbPairRecording, pair.Recording, _recordings);
        }
        finally
        {
            _loadingPairBuilder = false;
        }

        _pairPlaybackDirty = false;
        _pairRecordingDirty = false;
    }

    private void ResetPairBuilder()
    {
        _loadingPairBuilder = true;
        try
        {
            cmbPairPlayback.SelectedIndex = 0;
            cmbPairRecording.SelectedIndex = 0;
        }
        finally
        {
            _loadingPairBuilder = false;
        }

        _pairPlaybackDirty = false;
        _pairRecordingDirty = false;
    }

    private void CmbPairPlayback_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (!_loadingPairBuilder)
        {
            _pairPlaybackDirty = true;
        }
    }

    private void CmbPairRecording_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (!_loadingPairBuilder)
        {
            _pairRecordingDirty = true;
        }
    }

    private void BtnAddPair_Click(object sender, EventArgs e)
    {
        var pair = BuildEditedPair();
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

    /// <summary>True while a process picker is open. The Add flow auto-opens it from
    /// <see cref="OnShown"/>; a click on the ... button while it is up (or another re-entry)
    /// must not stack a second modal picker.</summary>
    private bool _processPickerOpen;

    private void SelectProcess()
    {
        if (_processPickerOpen)
        {
            return;
        }

        _processPickerOpen = true;
        try
        {
            OpenProcessPickerCore();
        }
        finally
        {
            _processPickerOpen = false;
        }
    }

    private void OpenProcessPickerCore()
    {
        using var form = new ProcessSelectionForm();
        if (form.ShowDialog(this) == DialogResult.OK)
        {
            _pickedProcessPath = form.SelectedProcessPath;

            // Always pre-fill the matching pattern; the concrete identity the picker knows is
            // preferred at save time when the rule qualifies for an identity assignment.
            if (!string.IsNullOrEmpty(form.SelectedProcessPath))
            {
                txtProcessPath.Text = EscapeToPattern(Path.GetFileName(form.SelectedProcessPath));
            }
            else if (!string.IsNullOrEmpty(form.SelectedProcessName))
            {
                var name = form.SelectedProcessName;
                if (!name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)) name += ".exe";
                txtProcessPath.Text = EscapeToPattern(name);
            }

            if (!string.IsNullOrEmpty(form.SelectedWindowTitle))
            {
                txtPattern.Text = $".*{Regex.Escape(form.SelectedWindowTitle)}.*";
            }

            UpdateAssignmentNote();
        }
    }

    private void BtnSelectProcess_Click(object sender, EventArgs e) => SelectProcess();

    private static string EscapeToPattern(string value) => $".*{Regex.Escape(value)}.*";

    /// <summary>
    /// The assignment option only makes sense where the constraints the persisted store routes
    /// can be honoured: a concrete exe identity (picked or typed), no window-title pattern and
    /// Automatic mode (ADR �6.4). A just-picked concrete identity counts even while the
    /// dynamic pattern pre-filled the process field � switching to App assignment writes it in.
    /// </summary>
    /// <summary>
    /// Show the "Windows will remember this app" note only while the rule actually qualifies
    /// for an identity assignment (ADR �0): Automatic mode, no window-title condition and a
    /// concrete identity (typed, or picked from the process picker). There is no control to
    /// set: eligibility is derived.
    /// </summary>
    private void UpdateAssignmentNote()
    {
        lblAssignmentHelper.Visible =
            !_initializing &&
            rdoAutomatic.Checked &&
            string.IsNullOrWhiteSpace(txtPattern.Text) &&
            TryGetConcreteIdentity(out _);
    }

    private void TxtProcessPath_TextChanged(object sender, EventArgs e) => UpdateAssignmentNote();

    private void TxtPattern_TextChanged(object sender, EventArgs e) => UpdateAssignmentNote();

    /// <summary>
    /// The concrete exe path the rule targets, when the editor knows one: what the user typed
    /// if it is a path rather than a pattern, otherwise the process picked from the picker
    /// (whose pattern pre-fill names the same exe).
    /// </summary>
    private bool TryGetConcreteIdentity(out string identity)
    {
        identity = string.Empty;

        var typed = (txtProcessPath.Text ?? string.Empty).Trim();
        if (typed.Length > 0 && !AppRuleAssignmentValidator.HasPatternSyntax(typed))
        {
            identity = typed;
            return true;
        }

        var picked = (_pickedProcessPath ?? string.Empty).Trim();
        if (picked.Length > 0 &&
            !AppRuleAssignmentValidator.HasPatternSyntax(picked) &&
            (typed.Length == 0 || typed.Contains(Path.GetFileName(picked), StringComparison.OrdinalIgnoreCase)))
        {
            identity = picked;
            return true;
        }

        return false;
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        _formLoaded = true;
        // Re-derive the bottom-region geometry now that the framework has applied the initial
        // DPI scaling to the designer coordinates: _appliedClientHeight was recorded in the
        // constructor at 100% scale, and the first mode change must treat the scaled height as
        // framework-chosen, not user-chosen. This also refreshes MinimumSize from the real
        // chrome frame (docs/design/ui/2026-11-06-dpi-awareness-scalable-dialogs.md).
        ApplyModeLayout();
        if (!_editing)
        {
            BeginInvoke(new Action(SelectProcess));
        }
    }

    /// <summary>
    /// Re-derive the bottom-region geometry after a DPI change rescales the form: the height
    /// baseline and MinimumSize must both follow the new scale, or the next mode switch treats
    /// the rescaled height as a user choice and keeps it.
    /// </summary>
    protected override void OnDpiChanged(DpiChangedEventArgs e)
    {
        base.OnDpiChanged(e);
        if (_formLoaded)
        {
            ApplyModeLayout();
        }
    }

    /// <summary>
    /// Preselect the combo box for a stored device: exact id match first, then
    /// <see cref="DeviceInfo.Equals(DeviceInfo)"/> (NameClean) match � this survives device id changes.
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

    /// <summary>
    /// Build the pair from the builder combos, merging any side the user did not
    /// touch from the stored selected pair. This keeps an unavailable side that
    /// the builder displayed as "Keep current" in place, while a side the user
    /// explicitly changed (including to "Keep current") is honoured.
    /// </summary>
    private AppSoundRuleDevicePair? BuildEditedPair()
    {
        var builderPair = BuildPairFromCombo(cmbPairPlayback, cmbPairRecording, _playbacks, _recordings);
        if (builderPair == null || _selectedPairIndex < 0 || _selectedPairIndex >= _pairs.Count)
        {
            return builderPair;
        }

        var storedPair = _pairs[_selectedPairIndex];
        if (!_pairPlaybackDirty)
        {
            builderPair.Playback = storedPair.Playback;
        }
        if (!_pairRecordingDirty)
        {
            builderPair.Recording = storedPair.Recording;
        }

        return builderPair.HasAnyDevice ? builderPair : null;
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
            // No enforcement choice (ADR �0): a rule that can be an identity assignment is one
            // and keeps the dynamic path as its fallback. A known concrete identity replaces the
            // picker's basename pattern (that is what makes the rule eligible); a typed pattern
            // or a window-title condition stays dynamic-only.
            if (string.IsNullOrWhiteSpace(txtPattern.Text) && TryGetConcreteIdentity(out var identity))
            {
                _rule.ProcessPath = identity.Trim();
            }

            _rule.Enforcement = AppRuleAssignmentValidator.EnforcementFor(_rule);
            // The identity is the process field itself for assignment rules; the matching for
            // them compares it exactly (ADR �6.1).
            _rule.AssignmentExePath = _rule.Enforcement == AppRuleEnforcement.Assignment ? _rule.ProcessPath : null;

        }
        else
        {
            // Hotkey rules enforce nothing at identity level; a cycle cannot live in the store.
            _rule.Enforcement = AppRuleEnforcement.Dynamic;
            _rule.AssignmentExePath = null;

            // Apply a builder edit that wasn't confirmed with the Add/Update button.
            var builderPair = BuildEditedPair();
            if (_selectedPairIndex >= 0 && _selectedPairIndex < _pairs.Count && builderPair != null)
            {
                _pairs[_selectedPairIndex] = builderPair;
            }
            else if (builderPair != null && !_pairs.Any(pair => SamePair(pair, builderPair)))
            {
                _pairs.Add(builderPair);
            }

            if (hotKeyBox.HotKey == null || !hotKeyBox.IsValid)
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
