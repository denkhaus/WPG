using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;

namespace WPGTest
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
		}

		private void OnLoaded(object sender, RoutedEventArgs e)
		{
			propertyGrid.SelectedObject = new BusinessObject();
		}
	}

	public class BusinessObject
	{
		public enum TestEnum { One, Two, Three };
		public SolidColorBrush Brush1 { get; set; }
		public SolidColorBrush Brush2 { get; set; }
		public SolidColorBrush Brush3 { get; set; }
		public string[] Things { get; set; }
		
		//[System.Windows.Controls.WpfPropertyGrid.PropertyEditor("StandardValuesEditor")]
		public TestEnum TestEnumThing { get; set; }

		public ObservableCollection<Brush>  MoreBrushes { get; set; }
		public BusinessObject()
		{
			Brush1 = Brushes.Red;
			Brush2 = Brushes.Green;
			Brush3 = Brushes.Blue;

			Things = new [] { "Foo", "Bar", "Baz" };

			MoreBrushes = new ObservableCollection<Brush>() { Brush1, Brush2, Brush3 };
		}

		
	}
}
