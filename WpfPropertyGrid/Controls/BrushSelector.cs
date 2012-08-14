using System.Globalization;
using System.Windows.Controls.WpfPropertyGrid.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace System.Windows.Controls.WpfPropertyGrid
{
	public class BrushSelector : IValueConverter
	{
		private bool AreBrushesEqual(Brush b1, Brush b2)
		{
			SolidColorBrush sb1 = b1 as SolidColorBrush;
			SolidColorBrush sb2 = b2 as SolidColorBrush;

			if (sb1 != null && sb2 != null && sb1.Color == sb2.Color)
				return true;

			//!!!  AR TODO: Implement some comparison for more complicated brushes
			return false;
		}

		private static string GetBrushName(Brush brush)
		{
			SolidColorBrush b = brush as SolidColorBrush;
			return b != null ? b.Color.ToString() : "Gradient Brush";
		}

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			Brush brush = value as Brush;

			if (brush == null) 
				return null;

			foreach (NamedBrush b in BrushList.Brushes)
				if (AreBrushesEqual(brush, b.Brush))
					return b;
			NamedBrush namedBrush = new NamedBrush(GetBrushName(brush), brush);
			BrushList.Add(namedBrush);
			return namedBrush;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return value;
		}
	}
}