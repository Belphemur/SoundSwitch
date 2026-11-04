using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace SoundSwitch.UI.Component;

public class NumericUpDownWithUnits : NumericUpDown
{
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string TextUnit { get; set; }

    /// <summary>
    /// Keeps <see cref="NumericUpDown.Value"/> in sync with a model value, without a WinForms data binding.
    /// </summary>
    /// <remarks>
    /// <see cref="NumericUpDown"/> throws an <see cref="ArgumentOutOfRangeException"/> instead of clamping when
    /// <see cref="NumericUpDown.Value"/> is assigned outside <see cref="NumericUpDown.Minimum"/> and
    /// <see cref="NumericUpDown.Maximum"/>. A binding registered while its control has no handle (for example
    /// a control on a tab page that has never been displayed) is still part of the shared
    /// <see cref="PropertyManager"/>, and every change on another bound control makes that manager push to it.
    /// The push writes <c>null</c>, which a decimal property turns into <c>0</c>, so a control with a non-zero
    /// <see cref="NumericUpDown.Minimum"/> throws from inside an unrelated control's event handler. Wiring the
    /// value by hand keeps the control out of that push.
    /// </remarks>
    public void BindValue(Func<int> read, Action<int> write)
    {
        Value = Math.Clamp(read(), Minimum, Maximum);
        ValueChanged += (_, _) => write((int)Value);
    }

    protected override void UpdateEditText()
    {
        base.UpdateEditText();

        if (string.IsNullOrEmpty(TextUnit))
            return;

        // The unit is not user input: flag the suffix as an internal text change, otherwise UpDownBase marks
        // the text as user-edited and the next Value read parses the text back into the value.
        ChangingText = true;
        Text += TextUnit;
        ChangingText = false;
    }

    protected override void ValidateEditText()
    {
        try
        {
            // Attempt to parse the text back to a decimal, removing the unit
            if (!string.IsNullOrEmpty(TextUnit) && Text.EndsWith(TextUnit))
            {
                string numericPart = Text.Substring(0, Text.Length - TextUnit.Length).Trim();
                Value = decimal.Parse(numericPart);
            }
            else
            {
                base.ValidateEditText(); // Fallback to default validation
            }
        }
        catch
        {
            base.ValidateEditText(); // If parsing fails, let the base handle it
        }
    }
}
