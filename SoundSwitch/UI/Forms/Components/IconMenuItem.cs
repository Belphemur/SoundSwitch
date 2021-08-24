using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using JetBrains.Annotations;
using SoundSwitch.Util;

namespace SoundSwitch.UI.Forms.Components
{
    public partial class IconMenuItem : UserControl
    {
        public DataContainer CurrentDataContainer { get; }

        public class DataContainer : INotifyPropertyChanged
        {
            private bool _selected;
            private Image _image;
            private string _label;
            private Icon _icon;

            public bool Selected
            {
                get => _selected;
                set
                {
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
                    _label = value;
                    OnPropertyChanged();
                }
            }

            public Icon Icon
            {
                get => _icon;
                set
                {
                    _icon = value;
                    OnPropertyChanged();
                    var oldImage = _image;
                    _image = null;
                    OnPropertyChanged(nameof(Image));
                    oldImage?.Dispose();
                }
            }

            public string Id { get; }
            public Color Color => Selected ? Color.RoyalBlue.WithOpacity(0x80) : Color.Black.WithOpacity(0x70);

            public DataContainer(Icon icon, string label, bool selected, string id)
            {
                Selected = selected;
                Icon = icon;
                Label = label;
                Id = id;
            }

            /// <summary>
            /// Override the metadata part
            /// </summary>
            /// <param name="dataContainer"/>
            public void OverrideMetadata(DataContainer dataContainer)
            {
                if (dataContainer.Id != Id)
                {
                    throw new ArgumentException("Need to have the same ID", nameof(dataContainer));
                }

                if (Selected != dataContainer.Selected)
                    Selected = dataContainer.Selected;
                if (Icon != dataContainer.Icon)
                    Icon = dataContainer.Icon;
                if (Label != dataContainer.Label)
                    Label = dataContainer.Label;
            }

            public event PropertyChangedEventHandler PropertyChanged;

            [NotifyPropertyChangedInvocator]
            protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
        }
    }
}