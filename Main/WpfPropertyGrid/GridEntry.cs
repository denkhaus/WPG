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
using System.ComponentModel;

namespace System.Windows.Controls.WpfPropertyGrid
{
	/// <summary>
	/// Specifies a base item for a property grid.
	/// </summary>
	public abstract class GridEntry : INotifyPropertyChanged, IPropertyFilterTarget, IDisposable
	{
		/// <summary>
		/// Gets the name of the encapsulated item.
		/// </summary>
		public string Name { get; protected set; }

		private bool isBrowsable;
		/// <summary>
		/// Gets or sets a value indicating whether this instance is browsable.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance is browsable; otherwise, <c>false</c>.
		/// </value>
		public bool IsBrowsable
		{
			get { return isBrowsable; }
			set
			{
				if (isBrowsable == value) return;
				isBrowsable = value;
				OnPropertyChanged("IsBrowsable");
				OnPropertyChanged("IsVisible");
				OnBrowsableChanged();
			}
		}

		/// <summary>
		/// Gets a value indicating whether this instance should be visible.
		/// </summary>
		public virtual bool IsVisible
		{
			get { return IsBrowsable && MatchesFilter; }
		}

		/// <summary>
		/// Gets or sets the owner of the item.
		/// </summary>
		/// <value>The owner of the item.</value>
		public PropertyGrid Owner { get; protected set; }

		private Editor editor;
		/// <summary>
		/// Gets or sets the editor.
		/// </summary>
		/// <value>The editor.</value>
		public virtual Editor Editor
		{
			get
			{
				if (editor == null && Owner != null)
					editor = Owner.GetEditor(this);
				return editor;
			}
			set
			{
				editor = value;
				OnPropertyChanged("Editor");
			}
		}

		#region Events

		/// <summary>
		/// Occurs when visibility state of the property is changed.
		/// </summary>
		public event EventHandler BrowsableChanged;

		/// <summary>
		/// Called when visibility state of the property is changed.
		/// </summary>
		protected virtual void OnBrowsableChanged()
		{
			EventHandler handler = BrowsableChanged;
			if (handler != null) handler(this, EventArgs.Empty);
		}

		#endregion

		#region IDisposable Members

		private bool disposed;

		/// <summary>
		/// Gets a value indicating whether this <see cref="PropertyItem"/> is disposed.
		/// </summary>
		/// <value><c>true</c> if disposed; otherwise, <c>false</c>.</value>
		protected bool Disposed
		{
			get { return disposed; }
		}

		/// <summary>
		/// Releases unmanaged and - optionally - managed resources
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		protected virtual void Dispose(bool disposing)
		{
			if (Disposed) return;
			if (disposing)
			{
			}

			disposed = true;
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}


		/// <summary>
		/// Releases unmanaged resources and performs other cleanup operations before the
		/// <see cref="PropertyItem"/> is reclaimed by garbage collection.
		/// </summary>
		~GridEntry()
		{
			Dispose(false);
		}

		#endregion

		#region IPropertyFilterTarget Members

		/// <summary>
		/// Occurs when filter is applied for the entry.
		/// </summary>
		public event EventHandler<PropertyFilterAppliedEventArgs> FilterApplied;

		/// <summary>
		/// Called when filter was applied for the entry.
		/// </summary>
		/// <param name="filter">The filter.</param>
		protected virtual void OnFilterApplied(PropertyFilter filter)
		{
			EventHandler<PropertyFilterAppliedEventArgs> handler = FilterApplied;
			if (handler != null) handler(this, new PropertyFilterAppliedEventArgs(filter));
		}

		/// <summary>
		/// Applies the filter for the entry.
		/// </summary>
		/// <param name="filter">The filter.</param>
		public abstract void ApplyFilter(PropertyFilter filter);

		/// <summary>
		/// Checks whether the entry matches the filtering predicate.
		/// </summary>
		/// <param name="predicate">The filtering predicate.</param>        
		/// <returns><c>true</c> if entry matches predicate; otherwise, <c>false</c>.</returns>
		public abstract bool MatchesPredicate(PropertyFilterPredicate predicate);

		private bool matchesFilter = true;
		/// <summary>
		/// Gets or sets a value indicating whether the entry matches filter.
		/// </summary>
		/// <value><c>true</c> if entry matches filter; otherwise, <c>false</c>.</value>
		public bool MatchesFilter
		{
			get { return matchesFilter; }
			protected set
			{
				if (matchesFilter == value) return;
				matchesFilter = value;
				OnPropertyChanged("MatchesFilter");
				OnPropertyChanged("IsVisible");
			}
		}

		#endregion

		#region INotifyPropertyChanged Members

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged(string propertyName)
		{
			PropertyChangedEventHandler handler = PropertyChanged;
			if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
		}

		#endregion
	}
}
