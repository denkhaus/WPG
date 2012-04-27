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
using System.ComponentModel;
using System.Globalization;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace System.Windows.Controls.WpfPropertyGrid
{
	/// <summary>
	/// Provides a wrapper around property value to be used at presentation level.
	/// </summary>
	public class PropertyItemValue : INotifyPropertyChanged, IDataErrorInfo
	{
		private readonly PropertyItem property;

		#region IDataErrorInfoValidation Stuff
		private readonly Dictionary<string, ValidationAttribute[]> validators;
		//private readonly Dictionary<string, Func<object, object>> getters; dmh - removed
		#endregion

		#region Events

		/// <summary>
		/// Occurs when exception is raised at Property Value.
		/// <remarks>This event is reserved for future implementations.</remarks>
		/// </summary>
		public event EventHandler<ValueExceptionEventArgs> PropertyValueException;
		/// <summary>
		/// Occurs when root value is changed.
		/// <remarks>This event is reserved for future implementations.</remarks>
		/// </summary>
		public event EventHandler RootValueChanged;
		/// <summary>
		/// Occurs when sub property changed.    
		/// </summary>
		public event EventHandler SubPropertyChanged;

		#endregion

		/// <summary>
		/// Gets the parent property.
		/// </summary>
		/// <value>The parent property.</value>
		public PropertyItem ParentProperty
		{
			get { return property; }
		}

		private readonly GridEntryCollection<PropertyItem> subProperties = new GridEntryCollection<PropertyItem>();
		public GridEntryCollection<PropertyItem> SubProperties
		{
			get { return subProperties; }
		}

		private readonly bool hasSubProperties;
		/// <summary>
		/// Gets a value indicating whether encapsulated value has sub-properties.
		/// </summary>
		/// <remarks>This property is reserved for future implementations.</remarks>
		/// <value>
		/// 	<c>true</c> if this instance has sub properties; otherwise, <c>false</c>.
		/// </value>
		public bool HasSubProperties
		{
			get { return hasSubProperties; }
		}

		#region ctor
		/// <summary>
		/// Initializes a new instance of the <see cref="PropertyItemValue"/> class.
		/// </summary>
		/// <param name="property">The property.</param>
		public PropertyItemValue(PropertyItem property)
		{
			if (property == null) throw new ArgumentNullException("property");
			this.property = property;

			#region IDataErrorInfo Validation attributes loading and property value getters

			// dmh - use property.Attributes instead of property.UnwrappedComponent.GetType().GetProperties() which can be empty
			//	   - removed getters. the actual value was unused
			validators = new Dictionary<string, ValidationAttribute[]> { { property.Name, property.Attributes.OfType<ValidationAttribute>().ToArray() } };

			#endregion
			
			hasSubProperties = property.Converter.GetPropertiesSupported();

			if (hasSubProperties)
			{
				object value = property.GetValue();

				if (value != null)
				{
					PropertyDescriptorCollection descriptors = property.Converter.GetProperties(value);
					if (descriptors != null)
						foreach (PropertyDescriptor d in descriptors)
						{
							subProperties.Add(new PropertyItem(property.Owner, value, d));
							// TODO: Move to PropertyData as a public property
							NotifyParentPropertyAttribute notifyParent = d.Attributes[KnownTypes.Attributes.NotifyParentPropertyAttribute] as NotifyParentPropertyAttribute;
							if (notifyParent != null && notifyParent.NotifyParent)
							{
								d.AddValueChanged(value, NotifySubPropertyChanged);
							}
						}
				}
			}

			this.property.PropertyChanged += ParentPropertyChanged;
		}
		#endregion

		void ParentPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "PropertyValue")
				NotifyRootValueChanged();

			if (e.PropertyName == "IsReadOnly")
			{
				OnPropertyChanged("IsReadOnly");
				OnPropertyChanged("IsEditable");
			}
		}

		#region PropertyValue implementation

		/// <summary>
		/// Gets a value indicating whether this instance can convert from string.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance can convert from string; otherwise, <c>false</c>.
		/// </value>
		public bool CanConvertFromString
		{
			get { return property.Converter != null && property.Converter.CanConvertFrom(typeof(string)) && !property.IsReadOnly; }
		}

		/// <summary>
		/// Clears the value.
		/// </summary>
		public void ClearValue()
		{
			property.ClearValue();
		}

		/// <summary>
		/// Converts the string to value.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>Value instance</returns>
		protected object ConvertStringToValue(string value)
		{
			if (property.PropertyType == typeof(string)) return value;
			//if (value.Length == 0) return null;
			if (string.IsNullOrEmpty(value)) return null;
			if (!property.Converter.CanConvertFrom(typeof(string)))
			{
				return null;
				throw new InvalidOperationException("Value to String conversion is not supported!");
			}
			return property.Converter.ConvertFromString(null, GetSerializationCulture(), value);
		}

		/// <summary>
		/// Converts the value to string.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns>String presentation of the value</returns>
		protected string ConvertValueToString(object value)
		{
			string collectionValue = string.Empty;
			if (value == null) return collectionValue;

			collectionValue = value as String;
			if (collectionValue != null) return collectionValue;

			TypeConverter converter = property.Converter;
			collectionValue = converter.CanConvertTo(typeof(string)) ? converter.ConvertToString(null, GetSerializationCulture(), value) : value.ToString();

			IList list = value as IList;
			if (list != null)
			{
				Type type = list.GetType().GetGenericArguments()[0];
				collectionValue = list.Count + " " + type.Name + "(s)";
			}

			// TODO: refer to resources or some constant
			//if (string.IsNullOrEmpty(collectionValue) && (value is IEnumerable))
			//    collectionValue = "(Collection)";

			return collectionValue;
		}

		/// <summary>
		/// Gets the value.
		/// </summary>
		/// <returns>Property value</returns>
		protected object GetValueCore()
		{
			return property.GetValue();
		}

		/// <summary>
		/// Gets a value indicating whether encapsulated property value is collection.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if encapsulated property value is collection; otherwise, <c>false</c>.
		/// </value>
		public bool IsCollection
		{
			get { return property.IsCollection; }
		}

		/// <summary>
		/// Gets a value indicating whether encapsulated property value is default value.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if encapsulated property value is default value; otherwise, <c>false</c>.
		/// </value>
		public bool IsDefaultValue
		{
			get { return property.IsDefaultValue; }
		}

		/// <summary>
		/// Sets the value.
		/// </summary>
		/// <param name="value">The value.</param>
		protected void SetValueCore(object value)
		{
			property.SetValue(value);
		}

		// TODO: DependencyProperty validation should be placed here
		/// <summary>
		/// Validates the value.
		/// </summary>
		/// <param name="valueToValidate">The value to validate.</param>
		protected void ValidateValue(object valueToValidate)
		{
			//throw new NotImplementedException();
			// Do nothing            
		}

		private void SetValueImpl(object value)
		{
			//this.ValidateValue(value);
			if (ParentProperty.Validate(value))
				SetValueCore(value);

			NotifyValueChanged();
			OnRootValueChanged();
		}

		/// <summary>
		/// Raises the <see cref="PropertyValueException"/> event.
		/// </summary>
		/// <param name="e">The <see cref="WpfPropertyGrid.ValueExceptionEventArgs"/> instance containing the event data.</param>
		protected virtual void OnPropertyValueException(ValueExceptionEventArgs e)
		{
			if (e == null) throw new ArgumentNullException("e");
			if (PropertyValueException != null) PropertyValueException(this, e);
		}

		/// <summary>
		/// Gets a value indicating whether exceptions should be cought.
		/// </summary>
		/// <value><c>true</c> if expceptions should be cought; otherwise, <c>false</c>.</value>
		protected virtual bool CatchExceptions
		{
			get { return true; } 
			//get { return (PropertyValueException != null); }
		}

		/// <summary>
		/// Gets or sets the string representation of the value.
		/// </summary>
		/// <value>The string value.</value>
		public string StringValue
		{
			get
			{
				string str = string.Empty;
				if (CatchExceptions)
				{
					try
					{
						str = ConvertValueToString(Value);
					}
					catch (Exception exception)
					{
						OnPropertyValueException(new ValueExceptionEventArgs("Cannot convert value to string", this, ValueExceptionSource.Get, exception));
					}
					//return str;
				}
				else 
					str = ConvertValueToString(Value);

				// !!! dmh - decrypt the value on the fly for display on the UI
				if (property.IsEncrypted && Decryptor != null)
					return Decryptor(str);
				
				return str;
			}
			set
			{			
				string str = value;

				// !!! dmh - encrypt the UI value on the fly 
				if (property.IsEncrypted && Encryptor != null)
					str = Encryptor(value);

				if (CatchExceptions)
				{
					try
					{
						Value = ConvertStringToValue(str);
					}
					catch (Exception exception)
					{
						OnPropertyValueException(new ValueExceptionEventArgs("Cannot create value from string", this, ValueExceptionSource.Set, exception));
					}
				}
				else
				{
					Value = ConvertStringToValue(str);
				}
			}
		}

		/// <summary>
		/// Gets or sets the value.
		/// </summary>
		/// <value>The value.</value>
		public object Value
		{
			get
			{
				object valueCore = null;
				if (CatchExceptions)
				{
					try
					{
						valueCore = GetValueCore();
					}
					catch (Exception exception)
					{
						OnPropertyValueException(new ValueExceptionEventArgs("Value Get Failed", this, ValueExceptionSource.Get, exception));
					}
					return valueCore;
				}
				return GetValueCore();
			}
			set
			{
				if (CatchExceptions)
				{
					try
					{
						SetValueImpl(value);
					}
					catch (Exception exception)
					{
						OnPropertyValueException(new ValueExceptionEventArgs("Value Set Failed", this, ValueExceptionSource.Set, exception));
					}
				}
				else
				{
					SetValueImpl(value);
				}
			}
		}

		#endregion

		#region Helper properties

		/// <summary>
		/// Gets a value indicating whether encapsulated property value is read only.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance is read only; otherwise, <c>false</c>.
		/// </value>
		public bool IsReadOnly
		{
			get { return property.IsReadOnly; }
		}

		/// <summary>
		/// Gets a value indicating whether encapsulated property value is editable.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance is editable; otherwise, <c>false</c>.
		/// </value>
		public bool IsEditable
		{
			get { return !property.IsReadOnly; }
		}

		

		public Func<string,string> Encryptor { get; set; }
		public Func<string,string> Decryptor { get; set; }

		#endregion

		/// <summary>
		/// Gets the serialization culture.
		/// </summary>
		/// <returns>Culture to serialize value.</returns>
		protected virtual CultureInfo GetSerializationCulture()
		{
			return ObjectServices.GetSerializationCulture(property.PropertyType);
		}

		#region INotifyPropertyChanged Members

		/// <summary>
		/// Occurs when a property value changes.
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// Called when property value is changed.
		/// </summary>
		/// <param name="propertyName">Name of the property.</param>
		protected void OnPropertyChanged(string propertyName)
		{
			if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}

		/// <summary>
		/// Notifies the root value changed.
		/// </summary>
		protected virtual void NotifyRootValueChanged()
		{
			OnPropertyChanged("IsDefaultValue");
			OnPropertyChanged("IsMixedValue");
			OnPropertyChanged("IsCollection");
			OnPropertyChanged("Collection");
			OnPropertyChanged("HasSubProperties");
			OnPropertyChanged("SubProperties");
			OnPropertyChanged("Source");
			OnPropertyChanged("CanConvertFromString");
			NotifyValueChanged();
			OnRootValueChanged();
		}

		private void NotifyStringValueChanged()
		{
			OnPropertyChanged("StringValue");
		}

		/// <summary>
		/// Notifies the sub property changed.
		/// </summary>
		protected void NotifySubPropertyChanged(object sender, EventArgs args)
		{
			NotifyValueChanged();
			OnSubPropertyChanged();
		}

		private void NotifyValueChanged()
		{
			OnPropertyChanged("Value");
			NotifyStringValueChanged();
		}

		private void OnRootValueChanged()
		{
			EventHandler handler = RootValueChanged;
			if (handler != null) handler(this, EventArgs.Empty);
		}

		private void OnSubPropertyChanged()
		{
			EventHandler handler = SubPropertyChanged;
			if (handler != null) handler(this, EventArgs.Empty);
		}

		#endregion

		#region IDataErrorInfo Members

		public string Error
		{
			get { return null; }
		}

		public string this[string columnName]
		{
			get
			{
				if (validators != null && validators.ContainsKey(property.Name))
				{
					string[] errorMessages = validators[property.Name]
						.Where(v => v.IsValid(property.PropertyValue.Value) == false)
						.Select(v => v.FormatErrorMessage(property.DisplayName)).ToArray();

					return string.Join(Environment.NewLine, errorMessages);
				}

				return string.Empty;
			}
		}

		#endregion
	}
}
