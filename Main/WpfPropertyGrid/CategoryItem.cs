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
using System.Collections.ObjectModel;
using System.Linq;
using System.ComponentModel.DataAnnotations;

namespace System.Windows.Controls.WpfPropertyGrid
{
	/// <summary>
	/// Special grid entry that provides information about property category and gives access to underlying properties.
	/// </summary>
	public class CategoryItem : GridEntry
	{
		#region Properties

		/// <summary>
		/// Gets or sets the attribute the category was created with.
		/// </summary>
		/// <value>The attribute.</value>
		public Attribute Attribute { get; set; }

		#region Order
		private int order = -1;
		/// <summary>
		/// Gets or sets the order of the category.
		/// </summary>
		public int Order
		{
			get { return order; }
			set
			{
				if (order == value) return;
				order = value;
				OnPropertyChanged("Order");
			}
		}
		#endregion

		#region IsExpanded
		private bool isExpanded = true;
		/// <summary>
		/// Gets or sets a value indicating whether this category is expanded.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this category is expanded; otherwise, <c>false</c>.
		/// </value>
		public bool IsExpanded
		{
			get { return isExpanded; }
			set
			{
				if (isExpanded == value) return;
				isExpanded = value;
				OnPropertyChanged("IsExpanded");
			}
		}
		#endregion

		#region Properties
		private readonly GridEntryCollection<PropertyItem> properties = new GridEntryCollection<PropertyItem>();
		/// <summary>
		/// Get all the properties in the category.
		/// </summary>
		/// <value></value>
		/// <returns>
		/// An enumerable collection of all the properties in the category.
		/// </returns>
		public ReadOnlyObservableCollection<PropertyItem> Properties
		{
			get { return new ReadOnlyObservableCollection<PropertyItem>(properties); }
		}
		#endregion

		/// <summary>
		/// Gets the <see cref="WpfPropertyGrid.PropertyItem"/> with the specified property name.
		/// </summary>
		/// <value></value>
		public PropertyItem this[string propertyName]
		{
			get { return properties[propertyName]; }
		}

		private IComparer<PropertyItem> comparer = new PropertyItemComparer();
		/// <summary>
		/// Gets or sets the comparer used to sort properties.
		/// </summary>
		/// <value>The comparer. </value>
		public IComparer<PropertyItem> Comparer
		{
			get { return comparer; }
			set
			{
				if (comparer == value) return;
				comparer = value;
				properties.Sort(comparer);
				OnPropertyChanged("Comparer");
			}
		}

		private bool hasVisibleProperties;
		/// <summary>
		/// Gets or sets a value indicating whether this instance has visible properties.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance has visible properties; otherwise, <c>false</c>.
		/// </value>
		public bool HasVisibleProperties
		{
			get { return hasVisibleProperties; }
			private set
			{
				if (hasVisibleProperties == value) return;
				hasVisibleProperties = value;
				OnPropertyChanged("HasVisibleProperties");
			}
		}

		/// <summary>
		/// Gets a value indicating whether this instance should be visible.
		/// </summary>
		public override bool IsVisible
		{
			get { return base.IsVisible && HasVisibleProperties; }
		}
		#endregion

		#region ctor
		/// <summary>
		/// Initializes a new instance of the <see cref="CategoryItem"/> class.
		/// </summary>
		/// <param name="owner">The owner.</param>
		/// <param name="name">The name.</param>
		public CategoryItem(PropertyGrid owner, string name)
		{
			if (owner == null) throw new ArgumentNullException("owner");
			if (string.IsNullOrEmpty(name)) throw new ArgumentNullException("name");

			Owner	= owner;
			Name	= name;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CategoryItem"/> class.
		/// </summary>
		/// <param name="owner">The owner.</param>
		/// <param name="category">The category.</param>
		public CategoryItem(PropertyGrid owner, DisplayAttribute category)
		// : this(owner, category.GetGroupName())  //!!! dmh - switch to displayAttribute & use GetGroupName so it will use resource string if present
		{
			Owner = owner;
			if (!string.IsNullOrEmpty(category.GroupName))
			{
				if (category.ResourceType != null)
				{
					try		{ Name = category.GetGroupName(); }
					catch	{ Name = category.GroupName; }
				}
				else Name = category.GroupName;
			}
			Attribute = category;

			// !!! dmh - be sure to get order from the display attribute for category as well.
			/* do not use order from display attribute for category! use CategoryOrderAttribute
			try 
			{ 
				int? orderAttempt = category.GetOrder(); 
				if (orderAttempt.HasValue)
					order = orderAttempt.Value;
			}
			catch { }
			*/
		}

		#endregion

		static readonly Func<PropertyItem, bool> isVisibleProperty = prop => prop.IsBrowsable && prop.MatchesFilter;

		/// <summary>
		/// Adds the property.
		/// </summary>
		/// <param name="property">The property.</param>
		public void AddProperty(PropertyItem property)
		{
			if (property == null) throw new ArgumentNullException("property");
			if (properties.Contains(property)) throw new ArgumentException("Cannot add a duplicated property " + property.Name);

			int index = properties.BinarySearch(property, comparer);
			if (index < 0)
				index = ~index;

			properties.Insert(index, property);

			HasVisibleProperties = property.IsBrowsable || properties.Any(isVisibleProperty);

			property.BrowsableChanged += PropertyBrowsableChanged;
		}

		private void PropertyBrowsableChanged(object sender, EventArgs e)
		{
			HasVisibleProperties = properties.Any(isVisibleProperty);
		}

		/// <summary>
		/// Checks whether the entry matches the filtering predicate.
		/// </summary>
		/// <param name="predicate">The filtering predicate.</param>
		/// <returns>
		/// 	<c>true</c> if entry matches predicate; otherwise, <c>false</c>.
		/// </returns>   
		public override bool MatchesPredicate(PropertyFilterPredicate predicate)
		{
			return properties.All(property => property.MatchesPredicate(predicate));
		}

		/// <summary>
		/// Applies the filter for the entry.
		/// </summary>
		/// <param name="filter">The filter.</param>
		public override void ApplyFilter(PropertyFilter filter)
		{
			bool propertiesMatch = false;
			foreach (PropertyItem entry in Properties)
			{
				if (PropertyMatchesFilter(filter, entry))
					propertiesMatch = true;
			}

			HasVisibleProperties	= properties.Any(isVisibleProperty);
			MatchesFilter			= propertiesMatch;

			if (propertiesMatch && !IsExpanded)
				IsExpanded = true;

			OnFilterApplied(filter);
		}

		private static bool PropertyMatchesFilter(PropertyFilter filter, PropertyItem entry)
		{
			entry.ApplyFilter(filter);
			return entry.MatchesFilter;
		}
	}
}