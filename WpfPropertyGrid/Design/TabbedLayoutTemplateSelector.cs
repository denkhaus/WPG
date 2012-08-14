namespace System.Windows.Controls.WpfPropertyGrid.Design
{
	public class TabbedLayoutTemplateSelector : DataTemplateSelector
	{
		private readonly ResourceLocator resourceLocator = new ResourceLocator();

		public override DataTemplate SelectTemplate(object item, DependencyObject container)
		{
			CategoryItem category = item as CategoryItem;
			if (category != null)
			{
				DataTemplate template = FindEditorTemplate(category);
				if (template != null) return template;
			}

			return base.SelectTemplate(item, container);
		}

		protected virtual DataTemplate FindEditorTemplate(CategoryItem category)
		{
			if (category == null) return null;

			Editor editor = category.Editor;

			if (editor == null) return null;

			DataTemplate template = editor.InlineTemplate as DataTemplate;
			if (template != null) return template;

			return resourceLocator.GetResource(editor.InlineTemplate) as DataTemplate;
		}
	}
}
