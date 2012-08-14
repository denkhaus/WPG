/*
 * Copyright © 2010, Denys Vuika
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *  http://www.apache.org/licenses/LICENSE-2.0
 *  
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Windows.Controls.WpfPropertyGrid.Controls;
using System.Windows.Controls.WpfPropertyGrid.Design;
using System.Windows.Input;
using System.Windows.Media;

namespace System.Windows.Controls.WpfPropertyGrid
{
	/// <summary>
	/// PropertyGrid control.
	/// </summary>
	public partial class PropertyGrid : Control, INotifyPropertyChanged
	{
		#region Static fields

		private static readonly Type thisType = typeof (PropertyGrid);

		#endregion

		/// <summary>
		/// Identifies the <see cref="PropertyEditingStarted"/> routed event.
		/// </summary>
		public static readonly RoutedEvent PropertyEditingStartedEvent = EventManager.RegisterRoutedEvent("PropertyEditingStarted", RoutingStrategy.Bubble, typeof (PropertyEditingEventHandler), thisType);

		/// <summary>
		/// Identifies the <see cref="PropertyEditingFinished"/> routed event.
		/// </summary>
		public static readonly RoutedEvent PropertyEditingFinishedEvent = EventManager.RegisterRoutedEvent("PropertyEditingFinished", RoutingStrategy.Bubble, typeof (PropertyEditingEventHandler), thisType);

		/// <summary>
		/// Identifies the <see cref="PropertyValueChanged"/> routed event.
		/// </summary>
		public static readonly RoutedEvent PropertyValueChangedEvent = EventManager.RegisterRoutedEvent("PropertyValueChanged", RoutingStrategy.Bubble, typeof(PropertyValueChangedEventHandler), thisType);

		/// <summary>
		/// Identifies the <see cref="ItemsBackground"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty ItemsBackgroundProperty = DependencyProperty.Register("ItemsBackground", typeof (Brush), thisType, new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.None));

		/// <summary>
		/// Identifies the <see cref="ItemsForeground"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty ItemsForegroundProperty = DependencyProperty.Register("ItemsForeground", typeof (Brush), thisType, new FrameworkPropertyMetadata(Brushes.Black, FrameworkPropertyMetadataOptions.None));

		/// <summary>
		/// Identifies the <see cref="Layout"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty LayoutProperty =
			DependencyProperty.Register("Layout", typeof (Control), thisType,
											new FrameworkPropertyMetadata(	default(AlphabeticalLayout),
																			FrameworkPropertyMetadataOptions.AffectsArrange |
																			FrameworkPropertyMetadataOptions.AffectsMeasure |
																			FrameworkPropertyMetadataOptions.AffectsRender,
																			OnLayoutChanged));

		/// <summary>
		/// Identifies the <see cref="ShowReadOnlyProperties"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty ShowReadOnlyPropertiesProperty = DependencyProperty.Register("ShowReadOnlyProperties", typeof (bool), thisType, new PropertyMetadata(false, OnShowReadOnlyPropertiesChanged));

		/// <summary>
		/// Identifies the <see cref="ShowAttachedProperties"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty ShowAttachedPropertiesProperty = DependencyProperty.Register("ShowAttachedProperties", typeof (bool), thisType, new PropertyMetadata(false, OnShowAttachedPropertiesChanged));

		/// <summary>
		/// Identifies the <see cref="PropertyFilter"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty PropertyFilterProperty = DependencyProperty.Register("PropertyFilter", typeof (string), thisType, new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnPropertyFilterChanged));

		/// <summary>
		/// Identifies the <see cref="PropertyFilterVisibility"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty PropertyFilterVisibilityProperty = DependencyProperty.Register("PropertyFilterVisibility", typeof (Visibility), thisType, new FrameworkPropertyMetadata(Visibility.Visible));

		/// <summary>
		/// Identifies the <see cref="PropertyDisplayMode"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty PropertyDisplayModeProperty = DependencyProperty.Register("PropertyDisplayMode", typeof (PropertyDisplayMode), thisType, new FrameworkPropertyMetadata(PropertyDisplayMode.All, OnPropertyDisplayModePropertyChanged));

		private GridEntryCollection<CategoryItem>	categories;
		private IComparer<CategoryItem>				categoryComparer;
		private GridEntryCollection<PropertyItem>	properties;
		private IComparer<PropertyItem>				propertyComparer;

		/// <summary>
		/// Gets or sets the brush for items background. This is a dependency property.
		/// </summary>
		/// <value>The items background brush.</value>
		public Brush ItemsBackground
		{
			get { return (Brush) GetValue(ItemsBackgroundProperty); }
			set { SetValue(ItemsBackgroundProperty, value); }
		}

		/// <summary>
		/// Gets or sets the items foreground brush. This is a dependency property.
		/// </summary>
		/// <value>The items foreground brush.</value>
		public Brush ItemsForeground
		{
			get { return (Brush) GetValue(ItemsForegroundProperty); }
			set { SetValue(ItemsForegroundProperty, value); }
		}

		/// <summary>
		/// Gets or sets the layout to be used to display properties.
		/// </summary>
		/// <value>The layout to be used to display properties.</value>
		public Control Layout
		{
			get { return (Control) GetValue(LayoutProperty); }
			set { SetValue(LayoutProperty, value); }
		}

		/// <summary>
		/// Gets or sets the selected object.
		/// </summary>
		/// <value>The selected object.</value>
		public object SelectedObject
		{
			get { return (currentObjects != null && currentObjects.Length != 0) ? currentObjects[0] : null; }
			set { SelectedObjects = (value == null) ? new object[0] : new[] {value}; }
		}

		/// <summary>
		/// Gets or sets the selected objects.
		/// </summary>
		/// <value>The selected objects.</value>
		public object[] SelectedObjects
		{
			get { return (currentObjects == null) ? new object[0] : (object[]) currentObjects.Clone(); }
			set
			{
				// Ensure there are no nulls in the array
				VerifySelectedObjects(value);

				bool sameSelection = false;

				// Check whether new selection is the same as was previously defined
				if (currentObjects != null && value != null && currentObjects.Length == value.Length)
				{
					sameSelection = true;

					for (int i = 0; i < value.Length && sameSelection; i++)
					{
						if (currentObjects[i] != value[i])
							sameSelection = false;
					}
				}

				if (!sameSelection)
				{
					// Assign new objects and reload
					if (value == null)
					{
						currentObjects = new object[0];
						DoReload();
					}
					else
					{
						// process single selection
						if (value.Length == 1 && currentObjects != null && currentObjects.Length == 1)
						{
							object oldValue = (currentObjects != null && currentObjects.Length > 0) ? currentObjects[0] : null;
							object newValue = (value.Length > 0) ? value[0] : null;

							currentObjects = (object[]) value.Clone();

							if (oldValue != null && newValue != null && oldValue.GetType().Equals(newValue.GetType()))
								SwapSelectedObject();
							else
								DoReload();
						}
							// process multiple selection
						else
						{
							currentObjects = (object[]) value.Clone();
							DoReload();
						}
					}

					OnPropertyChanged("SelectedObjects");
					OnPropertyChanged("SelectedObject");
					OnSelectedObjectsChanged();
				}
			}
		}

		/// <summary>
		/// Gets or sets the properties of the selected object(s).
		/// </summary>
		/// <value>The properties of the selected object(s).</value>
		public GridEntryCollection<PropertyItem> Properties
		{
			get { return properties; }
			private set
			{
				if (properties == value) return;

				if (properties != null)
				{
					foreach (PropertyItem item in properties)
					{
						UnhookPropertyChanged(item);
						item.Dispose();
					}
				}

				if (value != null)
				{
					properties = value;

					if (PropertyComparer != null)
						properties.Sort(PropertyComparer);

					foreach (PropertyItem item in properties)
						HookPropertyChanged(item);
				}

				OnPropertyChanged("Properties");
				OnPropertyChanged("HasProperties");
				OnPropertyChanged("BrowsableProperties");
			}
		}

		/// <summary>
		/// Enumerates the properties that should be visible for user
		/// </summary>
		public IEnumerable<PropertyItem> BrowsableProperties
		{
			get
			{
				if (properties != null)
					foreach (PropertyItem property in properties.Where(property => property.IsBrowsable)) yield return property;
			}
		}

		/// <summary>
		/// Gets or sets the default property comparer.
		/// </summary>
		/// <value>The default property comparer.</value>
		public IComparer<PropertyItem> PropertyComparer
		{
			get { return propertyComparer ?? (propertyComparer = new PropertyItemComparer()); }
			set
			{
				if (Equals(propertyComparer, value)) return;
				propertyComparer = value;

				if (properties != null)
					properties.Sort(propertyComparer);

				OnPropertyChanged("PropertyComparer");
			}
		}

		// !!!dmh	- added funcs for encrypting/decrypting property values on the UI
		//			- these must be set for the string value of a property to correct be shown / retrieved from UI
		public Func<string, string> PropertyEncryptor { get; set; }
		public Func<string, string> PropertyDecryptor { get; set; }

		/// <summary>
		/// Gets or sets the default category comparer.
		/// </summary>
		/// <value>The default category comparer.</value>
		public IComparer<CategoryItem> CategoryComparer
		{
			get { return categoryComparer ?? (categoryComparer = new CategoryItemComparer()); }
			set
			{
				if (Equals(categoryComparer, value)) return;
				categoryComparer = value;

				if (categories != null)
					categories.Sort(categoryComparer);

				OnPropertyChanged("Categories");
			}
		}

		/// <summary>
		/// Gets or sets the categories of the selected object(s).
		/// </summary>
		/// <value>The categories of the selected object(s).</value>
		public GridEntryCollection<CategoryItem> Categories
		{
			get { return categories; }
			private set
			{
				if (categories == value) return;
				categories = value;

				if (CategoryComparer != null)
					categories.Sort(CategoryComparer);

				OnPropertyChanged("Categories");
				OnPropertyChanged("HasCategories");
				OnPropertyChanged("BrowsableCategories");
			}
		}

		/// <summary>
		/// Enumerates the categories that should be visible for user.
		/// </summary>
		public IEnumerable<CategoryItem> BrowsableCategories
		{
			get
			{
				if (categories != null)
					foreach (CategoryItem category in categories.Where(category => category.IsBrowsable))
						yield return category;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether read-only properties should be displayed. This is a dependency property.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if read-only properties should be displayed; otherwise, <c>false</c>. Default is <c>false</c>.
		/// </value>
		public bool ShowReadOnlyProperties
		{
			get { return (bool) GetValue(ShowReadOnlyPropertiesProperty); }
			set { SetValue(ShowReadOnlyPropertiesProperty, value); }
		}

		/// <summary>
		/// Gets or sets a value indicating whether attached properties should be displayed.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if attached properties should be displayed; otherwise, <c>false</c>. Default is <c>false</c>.
		/// </value>
		public bool ShowAttachedProperties
		{
			get { return (bool) GetValue(ShowAttachedPropertiesProperty); }
			set { SetValue(ShowAttachedPropertiesProperty, value); }
		}

		/// <summary>
		/// Gets or sets the property filter. This is a dependency property.
		/// </summary>
		/// <value>The property filter.</value>
		public string PropertyFilter
		{
			get { return (string) GetValue(PropertyFilterProperty); }
			set { SetValue(PropertyFilterProperty, value); }
		}


		/// <summary>
		/// Gets or sets the property filter visibility state.
		/// </summary>
		/// <value>The property filter visibility state.</value>
		public Visibility PropertyFilterVisibility
		{
			get { return (Visibility) GetValue(PropertyFilterVisibilityProperty); }
			set { SetValue(PropertyFilterVisibilityProperty, value); }
		}

		/// <summary>
		/// Gets or sets the property display mode. This is a dependency property.
		/// </summary>
		/// <value>The property display mode.</value>
		public PropertyDisplayMode PropertyDisplayMode
		{
			get { return (PropertyDisplayMode) GetValue(PropertyDisplayModeProperty); }
			set { SetValue(PropertyDisplayModeProperty, value); }
		}

		#region INotifyPropertyChanged Members

		/// <summary>
		/// Occurs when a property value changes.
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

		#endregion

		/// <summary>
		/// Called when property value changes.
		/// </summary>
		/// <param name="propertyName">Name of the property.</param>
		protected virtual void OnPropertyChanged(string propertyName)
		{
			PropertyChangedEventHandler handler = PropertyChanged;
			if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
		}

		/// <summary>
		/// Occurs when property editing is started.
		/// </summary>
		/// <remarks>
		/// This event is intended to be used in customization scenarios. It is not used by PropertyGrid control directly.
		/// </remarks>
		public event RoutedEventHandler PropertyEditingStarted
		{
			add { AddHandler(PropertyEditingStartedEvent, value); }
			remove { RemoveHandler(PropertyEditingStartedEvent, value); }
		}

		/// <summary>
		/// Occurs when property editing is finished.
		/// </summary>
		/// <remarks>
		/// This event is intended to be used in customization scenarios. It is not used by PropertyGrid control directly.
		/// </remarks>
		public event RoutedEventHandler PropertyEditingFinished
		{
			add { AddHandler(PropertyEditingFinishedEvent, value); }
			remove { RemoveHandler(PropertyEditingFinishedEvent, value); }
		}

		/// <summary>
		/// Occurs when property item value is changed.
		/// </summary>
		public event PropertyValueChangedEventHandler PropertyValueChanged
		{
			add { AddHandler(PropertyValueChangedEvent, value); }
			remove { RemoveHandler(PropertyValueChangedEvent, value); }
		}

		private void RaisePropertyValueChangedEvent(PropertyItem property, object oldValue)
		{
			PropertyValueChangedEventArgs args = new PropertyValueChangedEventArgs(PropertyValueChangedEvent, property, oldValue);
			RaiseEvent(args);
		}

		/// <summary>
		/// Occurs when selected objects are changed.
		/// </summary>
		public event EventHandler SelectedObjectsChanged;

		/// <summary>
		/// Called when selected objects were changed.
		/// </summary>
		protected virtual void OnSelectedObjectsChanged()
		{
			EventHandler handler = SelectedObjectsChanged;
			if (handler != null) handler(this, EventArgs.Empty);
		}

		private static void OnLayoutChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
		{
			Control layoutContainer = e.NewValue as Control;
			if (layoutContainer != null)
				layoutContainer.DataContext = sender;
		}

		private static void OnShowReadOnlyPropertiesChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
		{
			PropertyGrid pg = (PropertyGrid) sender;

			// Check whether any object was selected
			if (pg.SelectedObject == null) return;

			// Check whether categories or properties were created
			if (pg.HasCategories | pg.HasProperties) pg.DoReload();
		}

		private static void OnShowAttachedPropertiesChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
		{
			PropertyGrid pg = (PropertyGrid) sender;
			if (pg.SelectedObject == null) return;
			if (pg.HasCategories | pg.HasProperties) pg.DoReload();
		}

		private static void OnPropertyFilterChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
		{
			PropertyGrid propertyGrid = (PropertyGrid) sender;

			if (propertyGrid.SelectedObject == null || !propertyGrid.HasCategories) return;

			foreach (CategoryItem category in propertyGrid.Categories)
				category.ApplyFilter(new PropertyFilter(propertyGrid.PropertyFilter));
		}

		private static void OnPropertyDisplayModePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
		{
			PropertyGrid propertyGrid = (PropertyGrid) sender;
			if (propertyGrid.SelectedObject == null) return;
			propertyGrid.DoReload();
		}

		public IEnumerable<string> GetAllValidationErrors()
		{
		    if (Properties == null)		return new HashSet<string>();
			// validate only visible (UI relevant) properties
			return Properties.Where(p => p.IsBrowsable)
				.Select(property => property.PropertyValue[property.Name])
				.Where(temp => !string.IsNullOrWhiteSpace(temp));
		}

		/// <summary>
		/// Gets the editor for a grid entry.
		/// </summary>
		/// <param name="entry">The entry to look the editor for.</param>
		/// <returns>Editor for the entry</returns>
		public virtual Editor GetEditor(GridEntry entry)
		{
			PropertyItem property = entry as PropertyItem;
			if (property != null)
				return Editors.GetEditor(property);

			CategoryItem category = entry as CategoryItem;
			if (category != null)
				return Editors.GetEditor(category);

			return null;
		}

		private void SwapSelectedObject()
		{
			//foreach (PropertyItem property in this.Properties)
			//{
			//  property.SetPropertySouce(value);
			//}
			DoReload();
		}

		private IEnumerable<CategoryItem> CollectCategories(IEnumerable<PropertyItem> pProperties)
		{
			Dictionary<string, CategoryItem> categoryItems		= new Dictionary<string, CategoryItem>();
			HashSet<string> refusedCategories	= new HashSet<string>();

			foreach (PropertyItem property in pProperties)
			{
				if (refusedCategories.Contains(property.CategoryName)) continue;
				CategoryItem category;

				if (categoryItems.ContainsKey(property.CategoryName))
					category = categoryItems[property.CategoryName];
				else
				{
					// dmh - hack for no given DisplayAttribute making Misc end up in refused categories, a default Category is assigned elsewhere or somethign
					DisplayAttribute displayAttr = property.GetAttribute<DisplayAttribute>();
					category = displayAttr == null || string.IsNullOrEmpty(displayAttr.GroupName)
									? CreateCategory(new DisplayAttribute { GroupName = property.CategoryName })
									: CreateCategory(displayAttr);

					if (category == null)
					{
						refusedCategories.Add(property.CategoryName);
						continue;
					}
				
					categoryItems[category.Name] = category;
				}

				category.AddProperty(property);
			}

			return categoryItems.Values.ToList();
		}

		private IEnumerable<PropertyItem> CollectProperties(object[] components)
		{
			if (components == null || components.Length == 0) throw new ArgumentNullException("components");

			// TODO: PropertyItem is to be wired with PropertyData rather than pure PropertyDescriptor in the next version!
			IEnumerable<PropertyDescriptor> descriptors = components.Length == 1	? MetadataRepository.GetProperties(components[0]).Select(prop => prop.Descriptor)
																					: ObjectServices.GetMergedProperties(components);

			return descriptors.Select(CreatePropertyItem).Where(item => item != null).ToList();
		}

		private static void VerifySelectedObjects(object[] value)
		{
			if (value != null && value.Length > 0)
			{
				// Ensure there are no nulls in the array
				for (int i = 0; i < value.Length; i++)
					if (value[i] == null)
					{
						object[] args = new object[] { i.ToString(CultureInfo.CurrentCulture), value.Length.ToString(CultureInfo.CurrentCulture) };
						// TODO: Move exception format to resources/settings!
						throw new ArgumentNullException(
							string.Format("Item {0} in the 'objs' array is null. The array must begin with at least {1} members.", args));
					}
			}
		}

		/// <summary>
		/// Invoked when an unhandled <see cref="UIElement.KeyDown"/> attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event.
		/// </summary>
		/// <param name="e">The <see cref="T:System.Windows.Input.KeyEventArgs"/> that contains the event data.</param>
		protected override void OnKeyDown(KeyEventArgs e)
		{
			if (e.Key == Key.Tab && e.OriginalSource is DependencyObject) //tabbing over the property editors
			{
				DependencyObject source = e.OriginalSource as DependencyObject;
				UIElement element = Keyboard.Modifiers == ModifierKeys.Shift ? GetTabElement(source, -1) : GetTabElement(source, 1);
				if (element == null) return;
				element.Focus();
				e.Handled = true;
				return;
			}

			base.OnKeyDown(e);
		}

		/// <summary>
		/// Gets the tab element on which the focus can be placed.
		/// </summary>
		/// <remarks>
		/// If an element is not enabled it will not be returned.
		/// </remarks>
		/// <param name="source">The source.</param>
		/// <param name="delta">The delta.</param>
		private UIElement GetTabElement(DependencyObject source, int delta)
		{
			// !!! dmh - added check for UIElement, Hyperlink can end here and throw an exception in FindVisualChild
			if (source == null || !(source is UIElement)) return null;
			PropertyContainer container = null;
			if (source is SearchTextBox && HasCategories)
			{
				ItemsPresenter itemspres = FindVisualChild<ItemsPresenter>(this);
				if (itemspres != null)
				{
					CategoryContainer catcontainer = FindVisualChild<CategoryContainer>(itemspres);
					if (catcontainer != null)
						container = FindVisualChild<PropertyContainer>(catcontainer);
				}
			}
			else
				container = FindVisualParent<PropertyContainer>(source);

			StackPanel spanel = FindVisualParent<StackPanel>(container);
			if (spanel != null && spanel.Children.Contains(container))
			{
				int index = spanel.Children.IndexOf(container);
				index = delta > 0	? (index == spanel.Children.Count - 1 ? 0 : index + delta)
									: (index == 0 ? spanel.Children.Count - 1 : index + delta);
				//loop inside the list
				if (index < 0)
					index = spanel.Children.Count - 1;
				if (index >= spanel.Children.Count)
					index = 0;

				PropertyContainer next = VisualTreeHelper.GetChild(spanel, index) as PropertyContainer; //this has always a Grid as visual child

				Grid grid = FindVisualChild<Grid>(next);
				if (grid != null && grid.Children.Count > 1)
				{
					PropertyEditorContentPresenter	pecp = grid.Children[1] as PropertyEditorContentPresenter;
					DependencyObject				final = VisualTreeHelper.GetChild(pecp, 0);
					if ((final as UIElement).IsEnabled && (final as UIElement).Focusable && !(next.DataContext as PropertyItem).IsReadOnly)
						return final as UIElement;
					return GetTabElement(final, delta);
				}
			}
			return null;
		}

		private static T FindVisualParent<T>(DependencyObject element) where T : class
		{
			if (element == null) return default(T);
			object parent = VisualTreeHelper.GetParent(element);
			return parent is T ? parent as T : (parent != null ? FindVisualParent<T>(parent as DependencyObject) : null);
		}

		private static T FindVisualChild<T>(DependencyObject element) where T : class
		{
			if (element == null) return default(T);
			if (element is T) return element as T;
			if (VisualTreeHelper.GetChildrenCount(element) > 0)
				for (int i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++)
				{
					object child = VisualTreeHelper.GetChild(element, i);
					if (child is SearchTextBox) continue; //speeds up things a bit
					if (child is T)
						return child as T;
					if (child != null)
					{
						T res = FindVisualChild<T>(child as DependencyObject);
						if (res == null) continue;
						return res;
					}
				}
			return null;
		}

		#region Internal API

		internal CategoryItem CreateCategory(DisplayAttribute attribute) // !!! dmh - switch to DisplayAttribute
		{
#if DEBUG
			// Check the attribute argument to be passed
			Diagnostics.Debug.Assert(attribute != null);
#else
			if (attribute == null) return null;
#endif

			// Check browsable restrictions
			//if (!ShouldDisplayCategory(attribute.Category)) return null;

			// Create a new CategoryItem
			CategoryItem categoryItem	= new CategoryItem(this, attribute);
			categoryItem.IsBrowsable	= ShouldDisplayCategory(categoryItem.Name);

			// Return resulting item
			return categoryItem;
		}

		private PropertyItem CreatePropertyItem(PropertyDescriptor descriptor)
		{
			// Check browsable restrictions
			//if (!ShoudDisplayProperty(descriptor)) return null;

			DependencyPropertyDescriptor dpDescriptor = DependencyPropertyDescriptor.FromProperty(descriptor);
			// Provide additional checks for dependency properties
			if (dpDescriptor != null)
			{
				// Check whether dependency properties are not prohibited
				if (PropertyDisplayMode == PropertyDisplayMode.Native) return null;

				// Check whether attached properties are to be displayed
				if (dpDescriptor.IsAttached && !ShowAttachedProperties) return null;
			}
			else if (PropertyDisplayMode == PropertyDisplayMode.Dependency) return null;

			// Check whether readonly properties are to be displayed
			if (descriptor.IsReadOnly && !ShowReadOnlyProperties) return null;

			// Note: superceded by ShouldDisplayProperty method call
			// Check whether property is browsable and add it to the collection
			// if (!descriptor.IsBrowsable) return null;

			//PropertyItem item = new PropertyItem(this, this.SelectedObject, descriptor);      

			PropertyItem item = currentObjects.Length > 1
										? new PropertyItem(this, currentObjects, descriptor)
										: new PropertyItem(this, SelectedObject, descriptor);

			//item.OverrideIsBrowsable(new bool?(ShoudDisplayProperty(descriptor)));
			item.IsBrowsable = ShoudDisplayProperty(descriptor);

			return item;
		}

		private bool ShoudDisplayProperty(PropertyDescriptor propertyDescriptor)
		{
#if DEBUG
			// Check the attribute argument to be passed
			Diagnostics.Debug.Assert(propertyDescriptor != null);
#else
			if (propertyDescriptor == null) return false;
#endif

			// Check whether owning category is not restricted to ouput
			bool showWithinCategory = ShouldDisplayCategory(propertyDescriptor.Category);
			if (!showWithinCategory) return false;

			// Check the explicit declaration
			BrowsablePropertyAttribute attribute = browsableProperties.FirstOrDefault(item => item.PropertyName == propertyDescriptor.Name);
			if (attribute != null) return attribute.Browsable;

			// Check the wildcard
			BrowsablePropertyAttribute wildcard = browsableProperties.FirstOrDefault(item => item.PropertyName == BrowsablePropertyAttribute.All);
			return wildcard != null ? wildcard.Browsable : propertyDescriptor.IsBrowsable;
		}

		private bool ShouldDisplayCategory(string categoryName)
		{
			if (string.IsNullOrEmpty(categoryName)) return false;

			// Check the explicit declaration
			BrowsableCategoryAttribute attribute = browsableCategories.FirstOrDefault(item => item.CategoryName == categoryName);
			if (attribute != null) return attribute.Browsable;

			// Check the wildcard
			BrowsableCategoryAttribute wildcard = browsableCategories.FirstOrDefault(item => item.CategoryName == BrowsableCategoryAttribute.All);
			return wildcard == null || wildcard.Browsable;
		}

		#endregion

		#region Private members

		public void DoReload()
		{
			// Create list of expanded properties in expanded state
			expandedItems.Clear();
			SaveExpandedPropertiesState(Properties);

			if (SelectedObject == null)
			{
				Categories = new GridEntryCollection<CategoryItem>();
				Properties = new GridEntryCollection<PropertyItem>();
			}
			else
			{
				// Collect BrowsableCategoryAttribute items
				IEnumerable<BrowsableCategoryAttribute> categoryAttributes = PropertyGridUtils.GetAttributes<BrowsableCategoryAttribute>(SelectedObject);
				browsableCategories = new List<BrowsableCategoryAttribute>(categoryAttributes);

				// Collect BrowsablePropertyAttribute items
				IEnumerable<BrowsablePropertyAttribute> propertyAttributes = PropertyGridUtils.GetAttributes<BrowsablePropertyAttribute>(SelectedObject);
				browsableProperties = new List<BrowsablePropertyAttribute>(propertyAttributes);

				// Collect categories and properties
				IEnumerable<PropertyItem> propertyItems = CollectProperties(currentObjects);

				// TODO: This needs more elegant implementation
				GridEntryCollection<CategoryItem> categoryItems = new GridEntryCollection<CategoryItem>(CollectCategories(propertyItems));
				if (categories != null && categories.Count > 0)
					CopyCategoryFrom(categories, categoryItems);

				// Fetch and apply category orders
				IEnumerable<CategoryOrderAttribute> categoryOrders =
					PropertyGridUtils.GetAttributes<CategoryOrderAttribute>(SelectedObject);
				foreach (CategoryOrderAttribute orderAttribute in categoryOrders)
				{
					CategoryItem category = categoryItems[orderAttribute.Category];
					// don't apply Order if it was applied before (Order equals zero or more), 
					// so the first discovered Order value for the same category wins
					if (category != null && category.Order < 0)
						category.Order = orderAttribute.Order;
				}

				// dmh - apply category default expanded state
				IEnumerable<CategoryExpandedAttribute> categorysExpanded =
					PropertyGridUtils.GetAttributes<CategoryExpandedAttribute>(SelectedObject);
				foreach (CategoryExpandedAttribute expandedAttribute in categorysExpanded)
				{
					CategoryItem category = categoryItems[expandedAttribute.Category];
					if (category != null)
						category.IsExpanded = expandedAttribute.Expanded;
				}

				Categories = categoryItems; 
				Properties = new GridEntryCollection<PropertyItem>(propertyItems);
			}

			// Restore expanded states for properties
			RestoreExpandedPropertiesState(Properties); 
		}

		private static void CopyCategoryFrom(GridEntryCollection<CategoryItem> oldValue, IEnumerable<CategoryItem> newValue)
		{
			foreach (CategoryItem category in newValue)
			{
				CategoryItem prev = oldValue[category.Name];
				if (prev == null) continue;

				category.IsExpanded = prev.IsExpanded;
			}
		}

		private void HookPropertyChanged(PropertyItem item)
		{
			if (item == null) return;
			item.ValueChanged += OnPropertyItemValueChanged;
			// dmh - listen to sub properties
			//foreach (PropertyItem pi in item.PropertyValue.SubProperties)
			//	pi.ValueChanged += OnPropertyItemSubValueChanged;
			item.PropertyValue.SubPropertyChanged += OnPropertyItemSubValueChanged;
		}

		private void OnPropertyItemValueChanged(PropertyItem property, object oldValue, object newValue)
		{
			AttributeCollection attributeCollection = property.Attributes;
			if (attributeCollection != null)
				foreach (RefreshPropertiesAttribute refreshAttribute in attributeCollection.OfType<RefreshPropertiesAttribute>())
					switch (refreshAttribute.RefreshProperties)
					{
						case RefreshProperties.All		:	MetadataRepository.Clear();		DoReload();	break;
						case RefreshProperties.Repaint	:									DoReload();	break;
					}

			RaisePropertyValueChangedEvent(property, oldValue);
		}
		
		// !!! dmh - handler for sub properties being changed (complex property layout, etc)
		private void OnPropertyItemSubValueChanged(object sender, EventArgs e)
		{
			PropertyItemValue propertyItemValue = sender as PropertyItemValue;
			if (propertyItemValue == null)
				return;

			RaisePropertyValueChangedEvent(propertyItemValue.ParentProperty, EventArgs.Empty);
		}

		private void RestoreExpandedPropertiesState(GridEntryCollection<PropertyItem> propCollection)
		{
			foreach (PropertyItem expandedItem in expandedItems)
			{
				PropertyItem pi = SearchForProperty(propCollection, expandedItem);
				if (pi != null)
					pi.IsExpanded = true;
			}
		}

		private void SaveExpandedPropertiesState(GridEntryCollection<PropertyItem> propCollection)
		{
			if (propCollection == null)
				return;

			foreach (PropertyItem pi in propCollection)
			{
				if (pi.IsExpanded)
					expandedItems.Add(pi);

				if (pi.PropertyValue.SubProperties.Count > 0)
					SaveExpandedPropertiesState(pi.PropertyValue.SubProperties);
			}
		}

		private PropertyItem SearchForProperty(GridEntryCollection<PropertyItem> propCollection, PropertyItem expandedItem)
		{
			if (propCollection == null)
				return null;

			foreach (PropertyItem pi in propCollection)
			{
				if (pi.DisplayName == expandedItem.DisplayName & pi.Description == expandedItem.Description & pi.Name == expandedItem.Name)
					return pi;

				if (pi.PropertyValue.SubProperties.Count > 0)
				{
					PropertyItem foundItem = SearchForProperty(pi.PropertyValue.SubProperties, expandedItem);
					if (foundItem != null) return foundItem;
				}
			}

			return null;
		}

		private void UnhookPropertyChanged(PropertyItem item)
		{
			if (item == null) return;
			item.ValueChanged -= OnPropertyItemValueChanged;
			
			// dmh - listen to sub properties
			//foreach (PropertyItem pi in item.PropertyValue.SubProperties)
			//	pi.ValueChanged -= OnPropertyItemValueChanged;
			item.PropertyValue.SubPropertyChanged -= OnPropertyItemSubValueChanged;
		}

		#endregion

		#region Fields

		private List<BrowsableCategoryAttribute> browsableCategories = new List<BrowsableCategoryAttribute>();
		private List<BrowsablePropertyAttribute> browsableProperties = new List<BrowsablePropertyAttribute>();

		private object[] currentObjects;

		#endregion

		#region Properties

		private readonly	EditorCollection		editors				= new EditorCollection();
		private				List<PropertyItem>		expandedItems		= new List<PropertyItem>();

		/// <summary>
		/// Gets the editors collection.
		/// </summary>
		/// <value>The editors collection.</value>
		public EditorCollection Editors
		{
			get { return editors; }
		}

		/// <summary>
		/// Gets a value indicating whether this instance has properties.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance has properties; otherwise, <c>false</c>.
		/// </value>
		public bool HasProperties
		{
			get { return properties != null && properties.Count > 0; }
		}

		/// <summary>
		/// Gets a value indicating whether this instance has categories.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance has categories; otherwise, <c>false</c>.
		/// </value>
		public bool HasCategories
		{
			get { return categories != null && categories.Count > 0; }
		}

		#endregion

		#region ctor

		static PropertyGrid()
		{
			DefaultStyleKeyProperty.OverrideMetadata(thisType, new FrameworkPropertyMetadata(thisType));
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="PropertyGrid"/> class.
		/// </summary>
		public PropertyGrid()
		{
			//!!! AR Replace to instance event handler due to multithreading issues 
			//EventManager.RegisterClassHandler(typeof(PropertyGrid), GotFocusEvent, new RoutedEventHandler(ShowDescription), true);
			AddHandler(GotFocusEvent, new RoutedEventHandler(ShowDescription), true);
			// Assign Layout to be Alphabetical by default
			Layout = new CategorizedLayout();

			// Wire command bindings
			InitializeCommandBindings();
#if TRIAL
            SetTrial();
#endif
		}

		private void ShowDescription(object sender, RoutedEventArgs e)
		{
			if (e.OriginalSource == null || !(e.OriginalSource is FrameworkElement) || (e.OriginalSource as FrameworkElement).DataContext == null ||
			    !((e.OriginalSource as FrameworkElement).DataContext is PropertyItemValue) ||
// ReSharper disable PossibleNullReferenceException
			    ((e.OriginalSource as FrameworkElement).DataContext as PropertyItemValue).ParentProperty == null)
				return;
			object descri = ((e.OriginalSource as FrameworkElement).DataContext as PropertyItemValue).ParentProperty.ToolTip;
// ReSharper restore PossibleNullReferenceException
			CurrentDescription = descri == null ? string.Empty : descri.ToString();
		}

		#endregion

		#region CurrentDescription

		/// <summary>
		/// CurrentDescription Dependency Property
		/// </summary>
		public static readonly DependencyProperty CurrentDescriptionProperty = DependencyProperty.Register("CurrentDescription", typeof (string), typeof (PropertyGrid), new FrameworkPropertyMetadata("", OnCurrentDescriptionChanged));

		/// <summary>
		/// Gets or sets the CurrentDescription property.  
		/// </summary>
		public string CurrentDescription
		{
			get { return (string) GetValue(CurrentDescriptionProperty); }
			set { SetValue(CurrentDescriptionProperty, value); }
		}

		/// <summary>
		/// Handles changes to the CurrentDescription property.
		/// </summary>
		private static void OnCurrentDescriptionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((PropertyGrid) d).OnCurrentDescriptionChanged(e);
		}

		/// <summary>
		/// Provides derived classes an opportunity to handle changes to the CurrentDescription property.
		/// </summary>
		protected virtual void OnCurrentDescriptionChanged(DependencyPropertyChangedEventArgs e){}

		#endregion
	}
}