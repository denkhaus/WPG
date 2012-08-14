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
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace System.Windows.Controls.WpfPropertyGrid
{
	[DebuggerDisplay("{Name}")]
	// For the moment this class is a wrapper around PropertyDescriptor. Later on it will be migrated into a separate independent unit.
	// It will be able in future creating dynamic objects without using reflection
	public class PropertyData : IEquatable<PropertyData>
	{
		#region Fields

		private static readonly List<Type> cultureInvariantTypes = new List<Type> 
																			{
																				KnownTypes.Wpf.CornerRadius,
																				KnownTypes.Wpf.Point3D,
																				KnownTypes.Wpf.Point4D,
																				KnownTypes.Wpf.Point3DCollection,
																				KnownTypes.Wpf.Matrix3D,
																				KnownTypes.Wpf.Quaternion,
																				KnownTypes.Wpf.Rect3D,
																				KnownTypes.Wpf.Size3D,
																				KnownTypes.Wpf.Vector3D,
																				KnownTypes.Wpf.Vector3DCollection,
																				KnownTypes.Wpf.PointCollection,
																				KnownTypes.Wpf.VectorCollection,
																				KnownTypes.Wpf.Point,
																				KnownTypes.Wpf.Rect,
																				KnownTypes.Wpf.Size,
																				KnownTypes.Wpf.Thickness, 
																				KnownTypes.Wpf.Vector
																			};

		#endregion

		public PropertyDescriptor Descriptor { get; private set; }

		public string Name
		{
			get { return Descriptor.Name; }
		}

		public string DisplayName
		{
			get { return Descriptor.DisplayName; }
		}

		public string Description
		{
			get { return Descriptor.Description; }
		}

		public string Category
		{
			get { return Descriptor.Category; }
		}

		public Type PropertyType
		{
			get { return Descriptor.PropertyType; }
		}

		public Type ComponentType
		{
			get { return Descriptor.ComponentType; }
		}

		public bool IsBrowsable
		{
			get { return Descriptor.IsBrowsable; }
		}

		public bool IsReadOnly
		{
			get { return Descriptor.IsReadOnly; }
		}

		// TODO: Cache value?
		public bool IsMergable
		{
			get { return MergablePropertyAttribute.Yes.Equals(Descriptor.Attributes[KnownTypes.Attributes.MergablePropertyAttribute]); }
		}

		// TODO: Cache value?
		public bool IsAdvanced
		{
			get
			{
				EditorBrowsableAttribute attr = Descriptor.Attributes[KnownTypes.Attributes.EditorBrowsableAttribute] as EditorBrowsableAttribute;
				return attr != null && attr.State == EditorBrowsableState.Advanced;
			}
		}

		public bool IsLocalizable
		{
			get { return Descriptor.IsLocalizable; }
		}

		public bool IsCollection
		{
			get { return KnownTypes.Collections.IList.IsAssignableFrom(PropertyType); }
		}

		public DesignerSerializationVisibility SerializationVisibility
		{
			get { return Descriptor.SerializationVisibility; }
		}

		private CultureInfo serializationCulture;
		public CultureInfo SerializationCulture
		{
			get
			{
				return serializationCulture ??
						(serializationCulture =
							(cultureInvariantTypes.Contains(PropertyType) || KnownTypes.Wpf.Geometry.IsAssignableFrom(PropertyType))
								? CultureInfo.InvariantCulture
								: CultureInfo.CurrentCulture);
			}
		}

		public PropertyData(PropertyDescriptor descriptor)
		{
			Descriptor = descriptor;
		}

		#region System.Object overrides

		public override int GetHashCode()
		{
			return Descriptor.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			PropertyData data = obj as PropertyData;
			return (data != null) && Descriptor.Equals(data.Descriptor);
		}

		#endregion

		#region IEquatable<PropertyData> Members

		public bool Equals(PropertyData other)
		{
			return Descriptor.Equals(other.Descriptor);
		}

		#endregion
	}

	public static class MetadataRepository
	{
		private class AttributeSet	: Dictionary<string, HashSet<Attribute>> { }
		private class PropertySet	: Dictionary<string, PropertyData> { }

		private static readonly Dictionary<Type, PropertySet>			properties			= new Dictionary<Type, PropertySet>();
		private static readonly Dictionary<Type, AttributeSet>			propertyAttributes	= new Dictionary<Type, AttributeSet>();
		private static readonly Dictionary<Type, HashSet<Attribute>>	typeAttributes		= new Dictionary<Type, HashSet<Attribute>>();

		private static readonly Attribute[] propertyFilter = new Attribute[] { new PropertyFilterAttribute(PropertyFilterOptions.SetValues | PropertyFilterOptions.UnsetValues | PropertyFilterOptions.Valid) };


		public static void Clear()
		{
			properties			.Clear();
			propertyAttributes	.Clear();
			typeAttributes		.Clear();
		}

		#region Property Management

		public static IEnumerable<PropertyData> GetProperties(object target)
		{
			return DoGetProperties(target).ToList().AsReadOnly();
		}

		private static IEnumerable<PropertyData> DoGetProperties(object target)
		{
			if (target == null) throw new ArgumentNullException("target");

			PropertySet result;
			if (!properties.TryGetValue(target.GetType(), out result))
				result = CollectProperties(target);

			return result.Values;
		}

		public static IEnumerable<PropertyData> GetCommonProperties(IEnumerable<object> targets)
		{
			if (targets == null) return Enumerable.Empty<PropertyData>();

			IEnumerable<PropertyData> result = targets.Select(target => DoGetProperties(target).Where(prop => prop.IsBrowsable && prop.IsMergable)).Aggregate<IEnumerable<PropertyData>, IEnumerable<PropertyData>>(null, (current, localLoopProperties) => (current == null) ? localLoopProperties : current.Intersect(localLoopProperties));

			return result ?? Enumerable.Empty<PropertyData>();
		}

		public static PropertyData GetProperty(object target, string propertyName)
		{
			if (target == null) throw new ArgumentNullException("target");
			if (string.IsNullOrEmpty(propertyName)) throw new ArgumentNullException("propertyName");

			PropertySet propertySet;

			if (!properties.TryGetValue(target.GetType(), out propertySet))
				propertySet = CollectProperties(target);

			PropertyData property;

			if (propertySet.TryGetValue(propertyName, out property))
				return property;

			return null;
		}

		private static PropertySet CollectProperties(object target)
		{
			Type targetType = target.GetType();
			PropertySet result;

			if (!properties.TryGetValue(targetType, out result))
			{
				result = new PropertySet();

				foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(target, propertyFilter))
				{
					result.Add(descriptor.Name, new PropertyData(descriptor));
					CollectAttributes(target, descriptor);
				}

				// Boris Tschirner - Collect Fields as well
				foreach (FieldInfo field in target.GetType().GetFields())
				{
					FieldPropertyDescriptor fieldDesc = new FieldPropertyDescriptor(field);
					result.Add(fieldDesc.Name, new PropertyData(fieldDesc));
					CollectAttributes(target, fieldDesc);
				}
				properties.Add(targetType, result);
			}
			return result;
		}

		#endregion Property Management

		#region Attribute Management

		public static IEnumerable<Attribute> GetAttributes(object target)
		{
			if (target == null) throw new ArgumentNullException("target");

			return CollectAttributes(target).ToList().AsReadOnly();
		}

		private static IEnumerable<Attribute> CollectAttributes(object target)
		{
			Type targetType = target.GetType();
			HashSet<Attribute> attributes;

			if (!typeAttributes.TryGetValue(targetType, out attributes))
			{
				attributes = new HashSet<Attribute>();

				foreach (Attribute attribute in TypeDescriptor.GetAttributes(target))
					attributes.Add(attribute);

				typeAttributes.Add(targetType, attributes);
			}

			return attributes;
		}

		private static void CollectAttributes(object target, PropertyDescriptor descriptor)
		{
			Type targetType = target.GetType();
			AttributeSet attributeSet;

			if (!propertyAttributes.TryGetValue(targetType, out attributeSet))
			{
				// Create an empty attribute sequence
				attributeSet = new AttributeSet();
				propertyAttributes.Add(targetType, attributeSet);
			}

			HashSet<Attribute> attributes;

			if (!attributeSet.TryGetValue(descriptor.Name, out attributes))
			{
				attributes = new HashSet<Attribute>();

				foreach (Attribute attribute in descriptor.Attributes)
					attributes.Add(attribute);

				attributeSet.Add(descriptor.Name, attributes);
			}

			return;
		}

		public static IEnumerable<Attribute> GetAttributes(object target, string propertyName)
		{
			if (target == null) throw new ArgumentNullException("target");
			if (string.IsNullOrEmpty(propertyName)) throw new ArgumentNullException("propertyName");

			Type targetType = target.GetType();

			if (!propertyAttributes.ContainsKey(targetType))
				CollectProperties(target);

			AttributeSet attributeSet;

			if (propertyAttributes.TryGetValue(targetType, out attributeSet))
			{
				HashSet<Attribute> result;
				if (attributeSet.TryGetValue(propertyName, out result))
					return result.ToList().AsReadOnly();
			}

			return Enumerable.Empty<Attribute>();
		}

		#endregion Attribute Management
	}
}
