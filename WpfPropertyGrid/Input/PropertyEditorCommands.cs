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
	///Provides a standard set of property editor related commands.
	/// </summary>
	public static class PropertyEditorCommands
	{
		private static readonly Type thisType = typeof(PropertyEditorCommands);

		private static readonly RoutedUICommand showDialogEditor	= new RoutedUICommand("Show Dialog Editor", "ShowDialogEditorCommand", thisType);
		private static readonly RoutedUICommand showExtendedEditor	= new RoutedUICommand("Show Extended Editor", "ShowExtendedEditorCommand", thisType);

		/// <summary>
		/// Defines a command to show dialog editor for a property.
		/// </summary>    
		public static RoutedUICommand ShowDialogEditor
		{
			get { return showDialogEditor; }
		}

		/// <summary>
		/// Defines a command to show extended editor for a property.
		/// </summary>
		public static RoutedUICommand ShowExtendedEditor
		{
			get { return showExtendedEditor; }
		}
	}
}
