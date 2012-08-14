using System.Windows.Media;

namespace System.Windows.Controls.WpfPropertyGrid
{
	public sealed class DashStyleList : Utils.MTObservableCollection<DashStyle>
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
