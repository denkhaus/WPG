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

using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;

namespace System.Windows.Controls.WpfPropertyGrid.Internal
{
	[DebuggerDisplay("{Name}")]
	internal class MergedPropertyDescriptor : PropertyDescriptor
	{
		// Fields
		private				TriState				canReset;
		private				MultiMergeCollection	collection;
		private readonly	PropertyDescriptor[]	descriptors;
		private				Hashtable				handlers;
		private				bool					internalValueSet;
		private				TriState				localizable;
		private				TriState				readOnly;


		// Methods
		public MergedPropertyDescriptor(PropertyDescriptor[] descriptors) : base(descriptors[0].Name, null)
		{
			this.descriptors = descriptors;
		}

		public override Type ComponentType
		{
			get { return descriptors[0].ComponentType; }
		}

		public override TypeConverter Converter
		{
			get { return descriptors[0].Converter; }
		}

		public override string DisplayName
		{
			get { return descriptors[0].DisplayName; }
		}

		public override bool IsLocalizable
		{
			get
			{
				if (localizable == TriState.Unknown)
				{
					localizable = TriState.Yes;
					foreach (PropertyDescriptor descriptor in descriptors)
						if (!descriptor.IsLocalizable)
						{
							localizable = TriState.No;
							break;
						}
				}
				return (localizable == TriState.Yes);
			}
		}

		public override bool IsReadOnly
		{
			get
			{
				if (readOnly == TriState.Unknown)
				{
					readOnly = TriState.No;
					foreach (PropertyDescriptor descriptor in descriptors)
					{
						if (descriptor.IsReadOnly)
						{
							readOnly = TriState.Yes;
							break;
						}
					}
				}
				return (readOnly == TriState.Yes);
			}
		}

		public PropertyDescriptor this[int index]
		{
			get { return descriptors[index]; }
		}

		public override Type PropertyType
		{
			get { return descriptors[0].PropertyType; }
		}

		public override sealed void AddValueChanged(object component, EventHandler handler)
		{
			if (component == null) throw new ArgumentNullException("component");
			if (handler == null) throw new ArgumentNullException("handler");

			var targets = component as Array;
			if (targets == null) throw new ArgumentException("Descriptor expects an array of objects!");

			if (handlers == null) handlers = new Hashtable();

			for (int i = 0; i < descriptors.Length; i++)
			{
				object target = targets.GetValue(i);

				descriptors[i].AddValueChanged(target, OnValueChanged);
				var h = (EventHandler) handlers[target];
				handlers[target] = Delegate.Combine(h, handler);
			}
		}

		public override sealed void RemoveValueChanged(object component, EventHandler handler)
		{
			if (component == null) throw new ArgumentNullException("component");
			if (handler == null) throw new ArgumentNullException("handler");

			var targets = component as Array;
			if (targets == null) throw new ArgumentException("Descriptor expects an array of objects!");

			for (int i = 0; i < descriptors.Length; i++)
			{
				object target = targets.GetValue(i);

				descriptors[i].AddValueChanged(target, OnValueChanged);
				if (handlers != null)
				{
					var h = (EventHandler) handlers[target];
					h = (EventHandler) Delegate.Remove(h, handler);

					if (h != null)
						handlers[target] = h;
					else
						handlers.Remove(target);
				}
			}
		}

		protected override void OnValueChanged(object component, EventArgs e)
		{
			if (internalValueSet) return;

			if (component != null && handlers != null)
			{
				var handler = (EventHandler) handlers[component];
				if (handler != null)
					handler(component, e);
			}
		}

		public override bool CanResetValue(object component)
		{
			if (canReset == TriState.Unknown)
			{
				canReset = TriState.Yes;
				var a = (Array) component;
				for (int i = 0; i < descriptors.Length; i++)
				{
					if (!descriptors[i].CanResetValue(GetPropertyOwnerForComponent(a, i)))
					{
						canReset = TriState.No;
						break;
					}
				}
			}
			return (canReset == TriState.Yes);
		}

		private object CopyValue(object value)
		{
			if (value != null)
			{
				Type type = value.GetType();
				if (type.IsValueType)
				{
					return value;
				}
				object obj2 = null;
				var cloneable = value as ICloneable;
				if (cloneable != null)
					obj2 = cloneable.Clone();
				if (obj2 == null)
				{
					// TODO: Reuse ObjectServices here?
					TypeConverter converter = TypeDescriptor.GetConverter(value);
					if (converter.CanConvertTo(typeof (InstanceDescriptor)))
					{
						var descriptor =
							(InstanceDescriptor) converter.ConvertTo(null, CultureInfo.InvariantCulture, value, typeof (InstanceDescriptor));
						if ((descriptor != null) && descriptor.IsComplete)
							obj2 = descriptor.Invoke();
					}
					if (obj2 == null && converter.CanConvertTo(typeof(string)) && converter.CanConvertFrom(typeof(string)))
					{
						object obj3 = converter.ConvertToInvariantString(value);
						obj2 = converter.ConvertFromInvariantString((string) obj3);
					}
				}
				if ((obj2 == null) && type.IsSerializable)
				{
					var formatter = new BinaryFormatter();
					var serializationStream = new MemoryStream();
					formatter.Serialize(serializationStream, value);
					serializationStream.Position = 0L;
					obj2 = formatter.Deserialize(serializationStream);
				}
				if (obj2 != null)
					return obj2;
			}
			return value;
		}

		protected override AttributeCollection CreateAttributeCollection()
		{
			IEnumerable<Attribute> attributes = null;

			foreach (PropertyDescriptor descriptor in descriptors)
			{
				Attribute[] buffer = new Attribute[descriptor.Attributes.Count];
				descriptor.Attributes.CopyTo(buffer, 0);
				attributes = (attributes == null) ? buffer : attributes.Intersect(buffer);
			}

			return new AttributeCollection(attributes.ToArray());
		}

		public override object GetEditor(Type editorBaseType)
		{
			return descriptors[0].GetEditor(editorBaseType);
		}

		private object GetPropertyOwnerForComponent(Array a, int i)
		{
			object propertyOwner = a.GetValue(i);
			if (propertyOwner is ICustomTypeDescriptor)
			{
				propertyOwner = ((ICustomTypeDescriptor) propertyOwner).GetPropertyOwner(descriptors[i]);
			}
			return propertyOwner;
		}

		public override object GetValue(object component)
		{
			bool flag;
			return GetValue((Array) component, out flag);
		}

		public object GetValue(Array components, out bool allEqual)
		{
			allEqual = true;
			object obj2 = descriptors[0].GetValue(GetPropertyOwnerForComponent(components, 0));
			if (obj2 is ICollection)
			{
				if (collection == null)
					collection = new MultiMergeCollection((ICollection) obj2);
				else
				{
					if (collection.Locked)
						return collection;

					collection.SetItems((ICollection) obj2);
				}
			}
			for (int i = 1; i < descriptors.Length; i++)
			{
				object obj3 = descriptors[i].GetValue(GetPropertyOwnerForComponent(components, i));
				if (collection != null)
				{
					if (!collection.MergeCollection((ICollection) obj3))
					{
						allEqual = false;
						return null;
					}
				}
				else if ((obj2 != null || obj3 != null) && (obj2 == null || !obj2.Equals(obj3)))
				{
					allEqual = false;
					return null;
				}
			}

			if (collection != null && collection.Count == 0)
				return null;

			if (collection == null)
				return obj2;

			return collection;
		}

		internal object[] GetValues(Array components)
		{
			var objArray = new object[components.Length];

			for (int i = 0; i < components.Length; i++)
				objArray[i] = descriptors[i].GetValue(GetPropertyOwnerForComponent(components, i));

			return objArray;
		}

		public override void ResetValue(object component)
		{
			var a = (Array) component;

			for (int i = 0; i < descriptors.Length; i++)
				descriptors[i].ResetValue(GetPropertyOwnerForComponent(a, i));
		}

		private void SetCollectionValues(Array a, ICollection listValue)
		{
			try
			{
				if (collection != null)
					collection.Locked = true;

				var array = new object[listValue.Count];
				listValue.CopyTo(array, 0);
				for (int i = 0; i < descriptors.Length; i++)
				{
					var list = descriptors[i].GetValue(GetPropertyOwnerForComponent(a, i)) as IList;
					if (list != null)
					{
						list.Clear();
						foreach (object obj2 in array)
							list.Add(obj2);
					}
				}
			}
			finally
			{
				if (collection != null)
					collection.Locked = false;
			}
		}

		public override void SetValue(object component, object value)
		{
			var a = (Array) component;

			if ((value is IList) && typeof (IList).IsAssignableFrom(PropertyType))
			{
				//TODO: Check whether internalValueSet should be configured here too...
				SetCollectionValues(a, (IList) value);
			}
			else
			{
				internalValueSet = true;
				for (int i = 0; i < descriptors.Length; i++)
				{
					object obj2 = CopyValue(value);
					object owner = GetPropertyOwnerForComponent(a, i);
					descriptors[i].SetValue(owner, obj2);

					//OnValueChanged(owner, new PropertyChangedEventArgs(descriptors[i].Name));
				}
				internalValueSet = false;
				OnValueChanged(component, new PropertyChangedEventArgs(Name));
			}
		}

		public override bool ShouldSerializeValue(object component)
		{
			var a = (Array) component;
			return !descriptors.Where((t, i) => !t.ShouldSerializeValue(GetPropertyOwnerForComponent(a, i))).Any();
		}

		// Properties

		#region Nested type: MultiMergeCollection

		private class MultiMergeCollection : ICollection
		{
			private object[] items;
			private bool locked;

			public MultiMergeCollection(ICollection original)
			{
				SetItems(original);
			}

			public bool Locked
			{
				get { return locked; }
				set { locked = value; }
			}

			#region ICollection Members

			public void CopyTo(Array array, int index)
			{
				if (items != null)
					Array.Copy(items, 0, array, index, items.Length);
			}

			public IEnumerator GetEnumerator()
			{
				if (items != null)
					return items.GetEnumerator();

				return new object[0].GetEnumerator();
			}

			public int Count
			{
				get
				{
					if (items != null)
						return items.Length;

					return 0;
				}
			}

			bool ICollection.IsSynchronized
			{
				get { return false; }
			}

			object ICollection.SyncRoot
			{
				get { return this; }
			}

			#endregion

			public bool MergeCollection(ICollection newCollection)
			{
				if (!locked)
				{
					if (items.Length != newCollection.Count)
					{
						items = new object[0];
						return false;
					}

					var array = new object[newCollection.Count];
					newCollection.CopyTo(array, 0);
					if (array.Where((t, i) => (t == null) != (items[i] == null) || (items[i] != null && !items[i].Equals(t))).Any())
					{
						items = new object[0];
						return false;
					}
				}
				return true;
			}

			public void SetItems(ICollection collection)
			{
				if (!locked)
				{
					items = new object[collection.Count];
					collection.CopyTo(items, 0);
				}
			}
		}

		#endregion

		#region Nested type: TriState

		private enum TriState
		{
			Unknown,
			Yes,
			No
		}

		#endregion
	}
}