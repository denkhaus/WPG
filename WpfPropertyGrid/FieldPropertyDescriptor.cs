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
using System.Reflection;

namespace System.Windows.Controls.WpfPropertyGrid
{
	// Boris Tschirner - Field Property Descriptor
	public class FieldPropertyDescriptor : PropertyDescriptor
	{
		public FieldPropertyDescriptor(FieldInfo field) : 
				base(field.Name, (Attribute[])field.GetCustomAttributes(typeof(Attribute), true))
		{
			Field = field;
		}

		public FieldInfo Field { get; private set; }

		public override bool Equals(object obj)
		{
			FieldPropertyDescriptor other = obj as FieldPropertyDescriptor;
			return other != null && other.Field.Equals(Field);
		}

		public override int GetHashCode() { return Field.GetHashCode(); }
		public override bool IsReadOnly { get { return false; } }
		public override void ResetValue(object component) {}
		public override bool CanResetValue(object component) { return false; }
		public override bool ShouldSerializeValue(object component) { return true; }
		public override Type ComponentType { get { return Field.DeclaringType; } }
		public override Type PropertyType { get { return Field.FieldType; } }
		public override object GetValue(object component) { return Field.GetValue(component); }
		public override void SetValue(object component, object value) 
		{
			Field.SetValue(component, value); 
			OnValueChanged(component, EventArgs.Empty);
		}
	}
}
