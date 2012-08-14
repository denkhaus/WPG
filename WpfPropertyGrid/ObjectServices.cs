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
using System.Globalization;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Collections.Generic;

namespace System.Windows.Controls.WpfPropertyGrid
{
	using Internal;

	internal static class ObjectServices
	{
		private static readonly Type[] cultureInvariantTypes = new[]
																{
																	typeof(CornerRadius),
																	typeof(Point3D),
																	typeof(Point4D),
																	typeof(Point3DCollection),
																	typeof(Matrix3D),
																	typeof(Quaternion),
																	typeof(Rect3D),
																	typeof(Size3D),
																	typeof(Vector3D),
																	typeof(Vector3DCollection),
																	typeof(PointCollection),
																	typeof(VectorCollection),
																	typeof(Point),
																	typeof(Rect),
																	typeof(Size),
																	typeof(Thickness),
																	typeof(Vector)
																};

		private static readonly string[] stringConverterMembers = { "Content", "Header", "ToolTip", "Tag" };

		#region DefaultStringConverter
		private static StringConverter defaultStringConverter;
		public static StringConverter DefaultStringConverter
		{
			get { return defaultStringConverter ?? (defaultStringConverter = new StringConverter()); }
		}
		#endregion

		#region DefaultFontStretchConverterDecorator
		private static FontStretchConverterDecorator defaultFontStretchConverterDecorator;
		public static FontStretchConverterDecorator DefaultFontStretchConverterDecorator
		{
			get { return defaultFontStretchConverterDecorator ?? (defaultFontStretchConverterDecorator = new FontStretchConverterDecorator()); }
		}
		#endregion

		#region DefaultFontStyleConverterDecorator
		private static FontStyleConverterDecorator defaultFontStyleConverterDecorator;
		public static FontStyleConverterDecorator DefaultFontStyleConverterDecorator
		{
			get { return defaultFontStyleConverterDecorator ?? (defaultFontStyleConverterDecorator = new FontStyleConverterDecorator()); }
		}
		#endregion

		#region DefaultFontWeightConverterDecorator
		private static FontWeightConverterDecorator defaultFontWeightConverterDecorator;
		public static FontWeightConverterDecorator DefaultFontWeightConverterDecorator
		{
			get { return defaultFontWeightConverterDecorator ?? (defaultFontWeightConverterDecorator = new FontWeightConverterDecorator()); }
		}
		#endregion

		// dmh - comment this out until it is actually added
		//[Obsolete("This member will be superceded by PropertyItem.SerializationCulture in the next versions of component", false)]
		public static CultureInfo GetSerializationCulture(Type propertyType)
		{
			CultureInfo currentCulture = CultureInfo.CurrentCulture;

			if (propertyType == null) return currentCulture;

			if ((Array.IndexOf(cultureInvariantTypes, propertyType) == -1) && !typeof(Geometry).IsAssignableFrom(propertyType))
				return currentCulture;

			return CultureInfo.InvariantCulture;
		}

		public static TypeConverter GetPropertyConverter(PropertyDescriptor propertyDescriptor)
		{
			if (propertyDescriptor == null)
				throw new ArgumentNullException("propertyDescriptor");

			if (stringConverterMembers.Contains(propertyDescriptor.Name)
			  && propertyDescriptor.PropertyType.IsAssignableFrom(typeof(object)))
				return DefaultStringConverter;
			if (typeof(FontStretch).IsAssignableFrom(propertyDescriptor.PropertyType))
				return DefaultFontStretchConverterDecorator;
			if (typeof(FontStyle).IsAssignableFrom(propertyDescriptor.PropertyType))
				return DefaultFontStyleConverterDecorator;
			if (typeof(FontWeight).IsAssignableFrom(propertyDescriptor.PropertyType))
				return DefaultFontWeightConverterDecorator;
			return propertyDescriptor.Converter;
		}

		#region MultiSelected Objects Support

		internal static IEnumerable<PropertyDescriptor> GetMergedProperties(IEnumerable<object> targets)
		{
			IEnumerable<PropertyData> props = MetadataRepository.GetCommonProperties(targets);

			return props.Select(data => targets.Select(target => MetadataRepository.GetProperty(target, data.Name).Descriptor)).Select(descriptors => new MergedPropertyDescriptor(descriptors.ToArray())).Cast<PropertyDescriptor>().ToList();
		}

		#endregion

		internal static object GetUnwrappedObject(object currentObject)
		{
			ICustomTypeDescriptor customTypeDescriptor = currentObject as ICustomTypeDescriptor;
			return customTypeDescriptor != null ? customTypeDescriptor.GetPropertyOwner(null) : currentObject;
		}
	}
}
