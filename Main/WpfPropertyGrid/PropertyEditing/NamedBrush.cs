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

using System.Windows.Media;

namespace System.Windows.Controls.WpfPropertyGrid
{
	/// <summary>
	/// Provides basic information for named colors.
	/// </summary>
	public sealed class NamedBrush
	{
		#region Properties

		/// <summary>
		/// Gets or sets the brush.
		/// </summary>
		/// <value>The brush.</value>
		public Brush Brush { get; private set; }

		/// <summary>
		/// Gets or sets the name.
		/// </summary>
		/// <value>The name.</value>
		public string Name { get; private set; }

		#endregion

		/// <summary>
		/// Initializes a new instance of the <see cref="NamedBrush"/> class.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="color">The color.</param>
		public NamedBrush(string name, Color color)
		{
			if (string.IsNullOrEmpty(name)) throw new ArgumentNullException("name");

			Name	= name;
			Brush	= (Brush)new SolidColorBrush(color).GetAsFrozen();
		}
		/// <summary>
		/// Initializes a new instance of the <see cref="NamedBrush"/> class.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="brush">The brush.</param>
		public NamedBrush(string name, Brush brush)
		{
			if (string.IsNullOrEmpty(name)) throw new ArgumentNullException("name");

			Name	= name;
			Brush	= brush;
			Brush.Freeze();
		}

		/// <summary>
		/// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
		/// </returns>
		public override String ToString()
		{
			return Name;
		}
	}
}
