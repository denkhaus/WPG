using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Windows.Media;
using System.Windows.Threading;

namespace System.Windows.Controls.WpfPropertyGrid.Controls
{

	public class BrushList
	{
		private static BrushList brushList;
		private static readonly MtObservableCollection<NamedBrush> brushes = new MtObservableCollection<NamedBrush>();

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

		public static MtObservableCollection<NamedBrush> Brushes
		{
			get { return brushes; }
		}

		public static void Add(NamedBrush namedBrush)
		{
			if (brushes.Where(nb => nb.Name == namedBrush.Name).Count() <= 0)
				brushes.Insert(0, namedBrush);
		}
	}

	public class MtObservableCollection<T> : ObservableCollection<T>
	{
		public override event NotifyCollectionChangedEventHandler CollectionChanged;
		protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
		{
			NotifyCollectionChangedEventHandler eh = CollectionChanged;
			if (eh == null) return;
			Dispatcher dispatcher = (from NotifyCollectionChangedEventHandler nh in eh.GetInvocationList()
										let		dpo = nh.Target as DispatcherObject
										where	dpo != null
										select	dpo.Dispatcher).FirstOrDefault();

			if (dispatcher != null && dispatcher.CheckAccess() == false)
				dispatcher.Invoke(DispatcherPriority.DataBind, (Action) (() => OnCollectionChanged(e)));
			else
				foreach (NotifyCollectionChangedEventHandler nh in eh.GetInvocationList())
					nh.Invoke(this, e);
		}
	}
}
