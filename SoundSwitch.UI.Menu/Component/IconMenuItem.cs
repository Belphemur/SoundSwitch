using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using JetBrains.Annotations;
using SoundSwitch.UI.Menu.Util;

namespace SoundSwitch.UI.Menu.Component
{
    public partial class IconMenuItem<T> : UserControl
    {
        public DataContainer CurrentDataContainer { get; }

        public class DataContainer : INotifyPropertyChanged, IDisposable
        {
            private bool _selected;
            private Image? _image;
            private string _label;
            private Icon _icon;

            public bool Selected
            {
                get => _selected;
                set
                {
                    if (value == _selected)
                    {
                        return;
                    }

                    _selected = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(Color));
                }
            }

            public Image Image
            {
                get { return _image ??= _icon.ToBitmap(); }
            }

            public string Label
            {
                get => _label;
                set
                {
                    if (value == _label)
                    {
                        return;
                    }

                    _label = value;
                    OnPropertyChanged();
                }
            }

            private Icon Icon
            {
                get => _icon;
                set
                {
                    if (value == _icon)
                    {
                        return;
                    }

                    _icon = value;
                    OnPropertyChanged();
                    var oldImage = _image;
                    _image = null;
                    OnPropertyChanged(nameof(Image));
                    oldImage?.Dispose();
                }
            }

            public T Payload { get; }

            public string Id { get; }
            public Color Color => Selected ? Color.RoyalBlue.WithOpacity(0x80) : Color.Black.WithOpacity(0x70);

            public DataContainer(Icon icon, string label, bool selected, string id, T payload)
            {
                Selected = selected;
                Icon = icon;
                Label = label;
                Id = id;
                Payload = payload;
            }

            /// <summary>
            /// Override the metadata part
            /// </summary>
            /// <param name="dataContainer"/>
            public void OverrideMetadata(DataContainer dataContainer)
            {
                if (dataContainer.Id != Id)
                {
                    throw new ArgumentException(@"Need to have the same ID", nameof(dataContainer));
                }

                Selected = dataContainer.Selected;
                Icon = dataContainer.Icon;
                Label = dataContainer.Label;
            }

            public event PropertyChangedEventHandler? PropertyChanged;

            [NotifyPropertyChangedInvocator]
            private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }

            public void Dispose()
            {
                _image?.Dispose();
                _image = null;
                GC.SuppressFinalize(this);
            }
        }

        public new event EventHandler Click
        {
            add
            {
                base.Click += value;
                foreach (Control control in Controls)
                {
                    control.Click += value;
                }
            }
            remove
            {
                base.Click -= value;
                foreach (Control control in Controls)
                {
                    control.Click -= value;
                }
            }
        }


        public IconMenuItem(DataContainer dataContainer)
        {
            CurrentDataContainer = dataContainer;
            InitializeComponent();

            base.CreateParams.ExStyle |= 0x20;
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            iconBox.BackColor = Color.Transparent;
            deviceName.BackColor = Color.Transparent;
            Name = CurrentDataContainer.Id;

            iconBox.DataBindings.Add(nameof(PictureBox.Image), CurrentDataContainer, nameof(CurrentDataContainer.Image), false, DataSourceUpdateMode.OnPropertyChanged);
            deviceName.DataBindings.Add(nameof(Label.Text), CurrentDataContainer, nameof(CurrentDataContainer.Label), false, DataSourceUpdateMode.OnPropertyChanged);
            DataBindings.Add(nameof(BackColor), CurrentDataContainer, nameof(CurrentDataContainer.Color), false, DataSourceUpdateMode.OnPropertyChanged);
            deviceName.TextChanged += AutoResizeLabel;
        }

        private void AutoResizeLabel(object? sender, EventArgs e)
        {
            if (sender is not Label lbl)
                return;
            if (lbl.Image != null)
                return;

            using Graphics cg = lbl.CreateGraphics();
            var labelSize = new SizeF(lbl.Width, lbl.Height);
            var textSize = cg.MeasureString(lbl.Text, lbl.Font, labelSize);
            while (textSize.Width > labelSize.Width - (labelSize.Width * 0.1))
            {
                lbl.Font = new Font(lbl.Font.Name, lbl.Font.Size - 0.2f, lbl.Font.Style);
                textSize = cg.MeasureString(lbl.Text, lbl.Font, labelSize);
                if (lbl.Font.Size < 8) break;
            }
        }
    }
}