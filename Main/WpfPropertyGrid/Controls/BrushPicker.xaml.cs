using System.Windows.Media;

namespace System.Windows.Controls.WpfPropertyGrid.Controls
{
	public partial class BrushPicker
	{
		public BrushPicker()
		{
			InitializeComponent();
		}

		public Brush SelectedBrush
		{
			get { return (Brush)GetValue(SelectedBrushProperty); }
			set { SetValue(SelectedBrushProperty, value); }
		}

		public static readonly DependencyProperty SelectedBrushProperty = DependencyProperty.Register("SelectedBrush", typeof(Brush), typeof(BrushPicker), new UIPropertyMetadata(null));
	}
}
