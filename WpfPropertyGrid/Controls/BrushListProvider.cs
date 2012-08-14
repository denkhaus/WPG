using System.Linq;
using System.Reflection;
using System.Windows.Media;

namespace System.Windows.Controls.WpfPropertyGrid.Controls
{

	public class BrushList
	{
		private static BrushList brushList;
		private static readonly Utils.MTObservableCollection<NamedBrush> brushes = new Utils.MTObservableCollection<NamedBrush>();

		private BrushList()
		{
			foreach (PropertyInfo pi in typeof(Colors).GetProperties())
			{
				if (pi.PropertyType != typeof(Color)) continue;

				MethodInfo mi = pi.GetGetMethod();
				const MethodAttributes inclusiveAttributes = MethodAttributes.Static | MethodAttributes.Public;
				if (mi == null || (mi.Attributes & inclusiveAttributes) != inclusiveAttributes) continue;

				NamedBrush namedBrush = new NamedBrush(pi.Name, (Color)pi.GetValue(null, null));
				brushes.Add(namedBrush);
			}
		}

		public static BrushList Instance
		{
			get { return brushList ?? (brushList = new BrushList()); }
		}

		public static Utils.MTObservableCollection<NamedBrush> Brushes
		{
			get { return brushes; }
		}

		public static void Add(NamedBrush namedBrush)
		{
			if (brushes.Count(nb => nb.Name == namedBrush.Name) <= 0)
				brushes.Insert(0, namedBrush);
		}
	}
}
