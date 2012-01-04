using System.Collections.ObjectModel;
using System.Windows.Media;

namespace System.Windows.Controls.WpfPropertyGrid
{
	public sealed class DashStyleList : ObservableCollection<DashStyle>
	{
		public DashStyleList()
		{
			Add(DashStyles.Dash);
			Add(DashStyles.DashDot);
			Add(DashStyles.DashDotDot);
			Add(DashStyles.Dot);
			Add(DashStyles.Solid);
		}
	}
}
