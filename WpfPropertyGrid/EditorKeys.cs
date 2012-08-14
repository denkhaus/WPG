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
namespace System.Windows.Controls.WpfPropertyGrid
{
	/// <summary>
	/// Contains a set of editor keys.
	/// </summary>
	public static class EditorKeys
	{
		private static readonly Type thisType = typeof(EditorKeys);
		private static readonly ComponentResourceKey namedColorEditorKey		= new ComponentResourceKey(thisType, "NamedColorEditor");
		private static readonly ComponentResourceKey passwordEditorKey			= new ComponentResourceKey(thisType, "PasswordEditor");
		private static readonly ComponentResourceKey defaultEditorKey			= new ComponentResourceKey(thisType, "DefaultEditor");
		private static readonly ComponentResourceKey booleanEditorKey			= new ComponentResourceKey(thisType, "BooleanEditor");
		private static readonly ComponentResourceKey doubleEditorKey			= new ComponentResourceKey(thisType, "DoubleEditor");
		private static readonly ComponentResourceKey enumEditorKey				= new ComponentResourceKey(thisType, "EnumEditor");
		private static readonly ComponentResourceKey sliderEditorKey			= new ComponentResourceKey(thisType, "SliderEditor");
		private static readonly ComponentResourceKey fontFamilyEditorKey		= new ComponentResourceKey(thisType, "FontFamilyEditor");
		private static readonly ComponentResourceKey brushEditorKey				= new ComponentResourceKey(thisType, "BrushEditor");
		private static readonly ComponentResourceKey penEditorKey				= new ComponentResourceKey(thisType, "PenEditor");
		private static readonly ComponentResourceKey dashStyleEditorKey			= new ComponentResourceKey(thisType, "DashStyleEditor");
		private static readonly ComponentResourceKey defaultCategoryEditorKey	= new ComponentResourceKey(thisType, "DefaultCategoryEditor");
		private static readonly ComponentResourceKey complexPropertyEditorKey	= new ComponentResourceKey(thisType, "ComplexPropertyEditor");

		/// <summary>
		/// Gets the NamedBrush editor key.
		/// </summary>
		/// <value>The named color editor key.</value>
		public static ComponentResourceKey NamedColorEditorKey { get { return namedColorEditorKey; } }

		/// <summary>
		/// Gets the password editor key.
		/// </summary>
		/// <value>The password editor key.</value>
		public static ComponentResourceKey PasswordEditorKey { get { return passwordEditorKey; } }

		/// <summary>
		/// Gets the default editor key.
		/// </summary>
		/// <value>The default editor key.</value>
		public static ComponentResourceKey DefaultEditorKey { get { return defaultEditorKey; } }

		/// <summary>
		/// Gets the boolean editor key.
		/// </summary>
		/// <value>The boolean editor key.</value>
		public static ComponentResourceKey BooleanEditorKey { get { return booleanEditorKey; } }

		/// <summary>
		/// Gets the double editor key.
		/// </summary>
		/// <value>The double editor key.</value>
		public static ComponentResourceKey DoubleEditorKey { get { return doubleEditorKey; } }

		/// <summary>
		/// Gets the enum editor key.
		/// </summary>
		/// <value>The enum editor key.</value>
		public static ComponentResourceKey EnumEditorKey { get { return enumEditorKey; } }

		/// <summary>
		/// Gets the slider editor key.
		/// </summary>
		/// <value>The slider editor key.</value>
		public static ComponentResourceKey SliderEditorKey { get { return sliderEditorKey; } }

		/// <summary>
		/// Gets the font family editor key.
		/// </summary>
		/// <value>The font family editor key.</value>
		public static ComponentResourceKey FontFamilyEditorKey { get { return fontFamilyEditorKey; } }

		/// <summary>
		/// Gets the brush editor key.
		/// </summary>
		/// <value>The brush editor key.</value>
		public static ComponentResourceKey BrushEditorKey { get { return brushEditorKey; } }

		/// <summary>
		/// Gets the pen editor key.
		/// </summary>
		/// <value>The pen editor key.</value>
		public static ComponentResourceKey PenEditorKey { get { return penEditorKey; } }

		/// <summary>
		/// Gets the DashStyle editor key.
		/// </summary>
		/// <value>The DashStyle editor key.</value>
		public static ComponentResourceKey DashStyleEditorKey { get { return dashStyleEditorKey; } }

		/// <summary>
		/// Gets the default category editor key.
		/// </summary>
		/// <value>The default category editor key.</value>
		public static ComponentResourceKey DefaultCategoryEditorKey { get { return defaultCategoryEditorKey; } }

		/// <summary>
		/// Gets the default complex property editor key.
		/// </summary>
		public static ComponentResourceKey ComplexPropertyEditorKey { get { return complexPropertyEditorKey; } }
	}
}
