using System.ComponentModel;
using System.Windows.Forms;

namespace SoundSwitch.UI.Component;

public class NumericUpDownWithUnits : NumericUpDown
{
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string TextUnit { get; set; }

    protected override void UpdateEditText()
    {
        base.UpdateEditText();

        if (!string.IsNullOrEmpty(TextUnit))
            Text += TextUnit;
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
