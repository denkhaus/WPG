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
using System.Windows.Input;

namespace System.Windows.Controls.WpfPropertyGrid
{
	/// <summary>
	/// Provides a standard set of property grid related commands.
	/// </summary>
	public static class PropertyGridCommands
	{
		private static readonly Type thisType = typeof(PropertyGridCommands);

		#region Commands

		private static readonly RoutedUICommand showFilter					= new RoutedUICommand("Show Filter",					"ShowFilter",					thisType);
		private static readonly RoutedUICommand hideFilter					= new RoutedUICommand("Hide Filter",					"HideFilter",					thisType);
		private static readonly RoutedUICommand toggleFilter				= new RoutedUICommand("Toggle Filter",					"ToggleFilter",					thisType);
		private static readonly RoutedUICommand resetFilter					= new RoutedUICommand("Reset Filter",					"ResetFilter",					thisType);
		private static readonly RoutedUICommand reload						= new RoutedUICommand("Reload",							"Reload",						thisType);
		private static readonly RoutedUICommand showReadOnlyProperties		= new RoutedUICommand("Show Read-Only Properties",		"ShowReadOnlyProperties",		thisType);
		private static readonly RoutedUICommand hideReadOnlyProperties		= new RoutedUICommand("Hide Read-Only Properties",		"HideReadOnlyProperties",		thisType);
		private static readonly RoutedUICommand toggleReadOnlyProperties	= new RoutedUICommand("Toggle Read-Only Properties",	"ToggleReadOnlyProperties",		thisType);
		private static readonly RoutedUICommand showAttachedProperties		= new RoutedUICommand("Show Attached Properties",		"ShowAttachedProperties",		thisType);
		private static readonly RoutedUICommand hideAttachedProperties		= new RoutedUICommand("Hide Attached Properties",		"HideAttachedProperties",		thisType);
		private static readonly RoutedUICommand toggleAttachedProperties	= new RoutedUICommand("Toggle Attached Properties",		"ToggleAttachedProperties",		thisType);
		private static readonly RoutedUICommand closePropertyTab			= new RoutedUICommand("Close Property Tab",				"ClosePropertyTabCommand",		thisType);

		/// <summary>
		/// Represents a command for the control to show property filter box.
		/// </summary>
		public static RoutedUICommand ShowFilter { get { return showFilter; } }

		/// <summary>
		/// Represents a command for the control to hide property filter box.
		/// </summary>
		public static RoutedUICommand HideFilter { get { return hideFilter; } }

		/// <summary>
		/// Represents a command for the control to toggle visibility of property filter box.
		/// </summary>
		public static RoutedUICommand ToggleFilter { get { return toggleFilter; } }

		/// <summary>
		/// Represents a command that resets current grid filter.
		/// </summary>
		public static RoutedUICommand ResetFilter { get { return resetFilter; } }

		/// <summary>
		/// Represents a command that reloads current grid properties.
		/// </summary>
		public static RoutedUICommand Reload { get { return reload; } }

		/// <summary>
		/// Represents a command for the control to show all read-only properties.
		/// </summary>
		public static RoutedUICommand ShowReadOnlyProperties { get { return showReadOnlyProperties; } }

		/// <summary>
		/// Represetns a command for the control to hide all read-only properties.
		/// </summary>
		public static RoutedUICommand HideReadOnlyProperties { get { return hideReadOnlyProperties; } }

		/// <summary>
		/// Represents a command for the control to toggle visibility of read-only properties.
		/// </summary>
		public static RoutedUICommand ToggleReadOnlyProperties { get { return toggleReadOnlyProperties; } }

		/// <summary>
		/// Represents a command for the control to show all attached properties.
		/// </summary>
		public static RoutedUICommand ShowAttachedProperties { get { return showAttachedProperties; } }

		/// <summary>
		/// Represents a command for the control to hide all attached properties.
		/// </summary>
		public static RoutedUICommand HideAttachedProperties { get { return hideAttachedProperties; } }

		/// <summary>
		/// Represents a command for the control to toggle visibility of attached properties.
		/// </summary>
		public static RoutedUICommand ToggleAttachedProperties { get { return toggleAttachedProperties; } }

		/// <summary>
		/// Represents a command for the control to close active property tab.
		/// </summary>
		public static RoutedUICommand ClosePropertyTab { get { return closePropertyTab; } }

		#endregion
	}
}
