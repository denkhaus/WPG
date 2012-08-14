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
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace System.Windows.Controls.WpfPropertyGrid
{
	/// <summary>
	/// Defines a wrapper around object property to be used at presentation level.
	/// </summary>
	public class PropertyItem : GridEntry
	{
		private readonly	PropertyItemValue parentValue;
		private				PropertyItemValue value;

		private class ComponentContext: ITypeDescriptorContext
		{
			private readonly object instance;

			public ComponentContext(object instance)
			{
				this.instance = instance;
			}

			public bool OnComponentChanging() { return false; }

			public void OnComponentChanged(){}

			public IContainer Container { get { return null; } }

			public object Instance { get { return instance; } }

			public PropertyDescriptor PropertyDescriptor { get { return null; } }

			public object GetService(Type serviceType) { return null;}
		}

		#region Fields
		private readonly	object					component;
		private				object					unwrappedComponent;
		private readonly	PropertyDescriptor		descriptor;
		private readonly	AttributesContainer		metadata;
		#endregion

		#region Filtering API

		/// <summary>
		/// Applies the filter for the entry.
		/// </summary>
		/// <param name="filter">The filter.</param>
		public override void ApplyFilter(PropertyFilter filter)
		{
			MatchesFilter = (filter == null) || filter.Match(this);
			OnFilterApplied(filter);
		}

		/// <summary>
		/// Checks whether the entry matches the filtering predicate.
		/// </summary>
		/// <param name="predicate">The filtering predicate.</param>
		/// <returns>
		/// 	<c>true</c> if entry matches predicate; otherwise, <c>false</c>.
		/// </returns>        
		public override bool MatchesPredicate(PropertyFilterPredicate predicate)
		{
			return predicate != null && (predicate.Match(DisplayName) || (PropertyType != null) && predicate.Match(PropertyType.Name));
		}

		#endregion

		#region ParentValue
		// TODO: Reserved for future implementations.
		/// <summary>
		/// Gets the parent value.
		/// <remarks>This property is reserved for future implementations</remarks>
		/// </summary>
		/// <value>The parent value.</value>
		public PropertyItemValue ParentValue
		{
			get { return parentValue; }
		}
		#endregion

		#region PropertyValue
		/// <summary>
		/// Gets the property value.
		/// </summary>
		/// <value>The property value.</value>
		public PropertyItemValue PropertyValue
		{
			get { return value ?? (value = CreatePropertyValueInstance()); }
		}

		// !!! dmh - added IsEncrypted to support on the fly decryption/encryption of strings on the UI
		public bool IsEncrypted
		{
			get;
			private set;
		}

		// !!! dmh - added IsExpanded for programmic control / styling of complex properties 
		private bool isExpanded;
		public bool IsExpanded
		{
			get { return isExpanded; }
			set 
			{
				if (isExpanded == value) return;
				isExpanded = value;
				OnPropertyChanged("IsExpanded");
			}
		}

		/// <summary>
		/// Creates the property value instance.
		/// </summary>
		/// <returns>A new instance of <see cref="PropertyItemValue"/>.</returns>
		protected PropertyItemValue CreatePropertyValueInstance()
		{
			return new PropertyItemValue(this);
		}
		#endregion

		#region PropertyDescriptor
		/// <summary>
		/// Gets PropertyDescriptor instance for the underlying property.
		/// </summary>
		public PropertyDescriptor PropertyDescriptor
		{
			get { return descriptor; }
		}
		#endregion

		#region ctor/init

		/// <summary>
		/// Initializes a new instance of the <see cref="PropertyItem"/> class.
		/// </summary>
		/// <param name="owner">The owner.</param>
		/// <param name="component">The component property belongs to.</param>
		/// <param name="descriptor">The property descriptor</param>
		public PropertyItem(PropertyGrid owner, object component, PropertyDescriptor descriptor) : this(null)
		{
			if (owner == null) throw new ArgumentNullException("owner");
			if (component == null) throw new ArgumentNullException("component");
			if (descriptor == null) throw new ArgumentNullException("descriptor");

			Owner				= owner;
			Name				= descriptor.Name;
			this.component		= component;
			this.descriptor		= descriptor;

			IsBrowsable			= descriptor.IsBrowsable;
			isReadOnly			= descriptor.IsReadOnly;

			// !!! dmh - modify these to check if we have a display attribute to use instead

			DisplayAttribute dispAttr = (DisplayAttribute)descriptor.Attributes[typeof(DisplayAttribute)];
			if (dispAttr != null && !string.IsNullOrEmpty(dispAttr.Description))
				if (dispAttr.ResourceType != null)
				{
					try
					{
						description = dispAttr.GetDescription();
					}
					catch
					{
						description = dispAttr.Description;
					}
				}
				else description = dispAttr.Description;
			else description = DisplayName;

			if (dispAttr != null && !string.IsNullOrEmpty(dispAttr.GroupName))
			{
				if (dispAttr.ResourceType != null)
				{
					try		{ categoryName = dispAttr.GetGroupName(); }
					catch	{ categoryName = dispAttr.GroupName; }
				}
				else categoryName = dispAttr.GroupName;
			}
			else categoryName = descriptor.Category;

			if (dispAttr != null && !string.IsNullOrEmpty(dispAttr.Prompt))
			{
				if (dispAttr.ResourceType != null)
				{
					try { prompt = dispAttr.GetPrompt(); }
					catch { prompt = dispAttr.Prompt; }
				}
				else prompt = dispAttr.Prompt;
			}
			else prompt = DisplayName;

			isLocalizable = descriptor.IsLocalizable;

			// !!! dmh - check if property is encrypted
			// do not use 'descriptor.Attributes[typeof(NinjaTrader.Gui.EncryptAttribute)]'
			//			-> this returns a default NinjaTrader.Gui.EncryptAttribute unless it takes parameters
			if (descriptor.Attributes.OfType<Metadata.EncryptAttribute>().Any())
			{
				IsEncrypted = true;
				
				PropertyValue.Encryptor = owner.PropertyEncryptor;
				PropertyValue.Decryptor = owner.PropertyDecryptor;
			}

			metadata = new AttributesContainer(descriptor.Attributes);
			this.descriptor.AddValueChanged(component, ComponentValueChanged);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="PropertyItem"/> class.
		/// </summary>
		/// <param name="parentValue">The parent value.</param>
		protected PropertyItem(PropertyItemValue parentValue)
		{
			this.parentValue = parentValue;
		}

		private void ComponentValueChanged(object sender, EventArgs e)
		{
			OnPropertyChanged("PropertyValue");
		}

		#endregion

		#region Events

		/// <summary>
		/// Occurs when property value is changed.
		/// </summary>
		public event Action<PropertyItem, object, object> ValueChanged;

		private void OnValueChanged(object oldValue, object newValue)
		{
			Action<PropertyItem, object, object> handler = ValueChanged;
			if (handler != null)
				handler(this, oldValue, newValue);
		}

		#endregion

		#region Properties

		#region DisplayName
		private string displayName;
		/// <summary>
		/// Gets the display name for the property.
		/// </summary>
		/// <value></value>
		/// <returns>
		/// The display name for the property.
		/// </returns>
		public string DisplayName
		{
			get
			{
				if (string.IsNullOrEmpty(displayName))
				{
					// !!! modified by dmh -> moved GetDisplayName() here since it was only usage.
					// !!! modified by dmh - read our display name from DisplayAttribute if there is one
					// Try getting Parenthesize attribute
					DisplayAttribute					displayAttr			= GetAttribute<DisplayAttribute>();
					ParenthesizePropertyNameAttribute	parenthesizeAttr	= GetAttribute<ParenthesizePropertyNameAttribute>();

					// dmh - if we are using a runtime adjusted property - use it's descriptor as it's already been localized 
					//if (descriptor is NinjaTrader.Gui.PropertyDescriptorExtended)
					//    displayName = descriptor.DisplayName;
					//else
					//{
						// dmh - if we still have a null _displayName return some default here
						string dispName;
						if (displayAttr != null && !string.IsNullOrEmpty(displayAttr.Name))
						{
							if (displayAttr.ResourceType != null)
							{
								try		{ dispName = displayAttr.GetName(); }
								catch	{ dispName = displayAttr.Name;		}
							}
							else dispName = displayAttr.Name;
						}
						else dispName = descriptor.DisplayName;

						// if property needs parenthesizing then apply parenthesis to resulting display name
						displayName = (parenthesizeAttr != null && parenthesizeAttr.NeedParenthesis) ? string.Format("({0})", dispName) : dispName;
					//}
				}

				return displayName;
			}
			set
			{
				if (displayName == value) return;
				displayName = value;
				OnPropertyChanged("DisplayName");
			}
		}
		#endregion

		#region CategoryName
		private readonly string categoryName;
		/// <summary>
		/// Gets the name of the category that this property resides in.
		/// </summary>
		/// <value></value>
		/// <returns>
		/// The name of the category that this property resides in.
		/// </returns>
		public string CategoryName
		{
			get { return categoryName; }
		}
		#endregion

		#region Description
		private string description;
		/// <summary>
		/// Gets the description of the encapsulated property.
		/// </summary>
		/// <value></value>
		/// <returns>
		/// The description of the encapsulated property.
		/// </returns>
		public string Description
		{
			get { return description; }
			set
			{
				if (description == value) return;
				description = value;
				OnPropertyChanged("Description");
			}
		}
		#endregion

		#region Prompt
		private string prompt;
		/// <summary>
		/// Gets the prompt of the encapsulated property.
		/// </summary>
		/// <value></value>
		/// <returns>
		/// The prompt of the encapsulated property.
		/// </returns>
		public string Prompt
		{
			get { return prompt; }
			set
			{
				if (prompt == value) return;
				prompt = value;
				OnPropertyChanged("Prompt");
			}
		}
		#endregion

		#region IsAdvanced
		/// <summary>
		/// Gets a value indicating whether the encapsulated property is an advanced property.
		/// </summary>    
		/// <returns>true if the encapsulated property is an advanced property; otherwise, false.</returns>
		// TODO: move intilialization to ctor
		public bool IsAdvanced
		{
			get
			{
				EditorBrowsableAttribute browsable = (EditorBrowsableAttribute)Attributes[typeof(EditorBrowsableAttribute)];
				return browsable != null && browsable.State == EditorBrowsableState.Advanced;
			}
		}
		#endregion

		#region IsLocalizable
		private readonly bool isLocalizable;
		/// <summary>
		/// Gets a value indicating whether the encapsulated property is localizable.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance is localizable; otherwise, <c>false</c>.
		/// </value>
		public bool IsLocalizable
		{
			get { return isLocalizable; }
		}
		#endregion

		#region IsReadOnly
		private bool isReadOnly;
		/// <summary>
		/// Gets a value indicating whether the encapsulated property is read-only.
		/// </summary>
		/// <value></value>
		/// <returns>true if the encapsulated property is read-only; otherwise, false.
		/// </returns>
		public bool IsReadOnly
		{
			get { return isReadOnly; }
			set
			{
				if (isReadOnly == value) return;
				isReadOnly = value;
				OnPropertyChanged("IsReadOnly");
			}
		}
		#endregion

		#region PropertyType
		/// <summary>
		/// Gets the type of the encapsulated property.
		/// </summary>
		/// <value></value>
		/// <returns>
		/// The type of the encapsulated property.
		/// </returns>
		public virtual Type PropertyType
		{
			get
			{
				if (descriptor == null) return null;
				return descriptor.PropertyType;
			}
		}
		#endregion

		#region StandardValues
		/// <summary>
		/// Gets the standard values that the encapsulated property supports.
		/// </summary>
		/// <value></value>
		/// <returns>
		/// A <see cref="T:System.Collections.ICollection"/> of standard values that the encapsulated property supports.
		/// </returns>
		public ICollection StandardValues
		{
			get
			{
				return Converter.GetStandardValuesSupported() ? (ICollection)Converter.GetStandardValues(new ComponentContext(Component)) : new ArrayList(0);
			}
		}
		#endregion

		#region Component
		/// <summary>
		/// Gets the component the property belongs to.
		/// </summary>
		/// <value>The component.</value>
		public object Component
		{
			get { return component; }
		}
		#endregion

		#region UnwrappedComponent

		/// <summary>
		/// Gets the component the property belongs to.
		/// </summary>
		/// <remarks>
		/// This property returns a real unwrapped component even if a custom type descriptor is used.
		/// </remarks>
		public object UnwrappedComponent
		{
			get { return unwrappedComponent ?? (unwrappedComponent = ObjectServices.GetUnwrappedObject(component)); }
		}

		#endregion

		#region ToolTip

		/// <summary>
		/// Gets the tool tip.
		/// </summary>
		/// <value>The tool tip.</value>
		public object ToolTip
		{
			get
			{
				DisplayAttribute attribute = GetAttribute<DisplayAttribute>();
				return (attribute != null && !string.IsNullOrEmpty(attribute.GetDescription())) ? attribute.GetDescription() : DisplayName;
			}
		}

		#endregion

		#region Attributes
		/// <summary>
		/// Gets the custom attributes bound to property.
		/// </summary>
		/// <value>The attributes.</value>
		public virtual AttributeCollection Attributes
		{
			get { return descriptor == null ? null : descriptor.Attributes; }
		}
		#endregion

		#region Metadata
		/// <summary>
		/// Gets the custom attributes container.
		/// </summary>
		/// <value>The custom attributes container.</value>
		public AttributesContainer Metadata
		{
			get { return metadata; }
		}
		#endregion

		#region Converter
		/// <summary>
		/// Gets the converter.
		/// </summary>
		/// <value>The converter.</value>
		public TypeConverter Converter
		{
			get { return ObjectServices.GetPropertyConverter(descriptor); }
		}
		#endregion

		#region CanClearValue
		/// <summary>
		/// Gets a value indicating whether this instance can clear value.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance can clear value; otherwise, <c>false</c>.
		/// </value>
		public bool CanClearValue
		{
			get { return descriptor.CanResetValue(component); }
		}
		#endregion

		#region IsDefaultValue
		// TODO: support this (UI should also react on it)
		/// <summary>
		/// Gets a value indicating whether this instance is default value.
		/// <remarks>This property is reserved for future implementations.</remarks>
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance is default value; otherwise, <c>false</c>.
		/// </value>
		public bool IsDefaultValue
		{
			get { throw new NotImplementedException(); }
		}
		#endregion

		#region IsCollection
		/// <summary>
		/// Gets a value indicating whether this instance is collection.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance is collection; otherwise, <c>false</c>.
		/// </value>
		public bool IsCollection
		{
			get { return typeof(IList).IsAssignableFrom(PropertyType); }
		}
		#endregion

		#endregion

		#region Methods
		/// <summary>
		/// Clears the value.
		/// </summary>
		public void ClearValue()
		{
			if (!CanClearValue) return;

			object oldValue = GetValue();
			descriptor.ResetValue(component);
			OnValueChanged(oldValue, GetValue());
			OnPropertyChanged("PropertyValue");
		}

		/// <summary>
		/// Gets the value.
		/// </summary>
		/// <returns>Property value</returns>
		public object GetValue()
		{
			if (descriptor == null) return null;
			object target = GetViaCustomTypeDescriptor(component, descriptor);
			return descriptor.GetValue(target);
		}

		private void SetValueCore(object pValue)
		{
			if (descriptor == null) return;

			// Check whether underlying dependency property passes validation
			if (!IsValidDependencyPropertyValue(descriptor, pValue))
			{
				OnPropertyChanged("PropertyValue");
				return;
			}

			object target = GetViaCustomTypeDescriptor(component, descriptor);

			if (target != null)
				descriptor.SetValue(target, pValue);
		}

		/// <summary>
		/// Sets the value.
		/// </summary>
		/// <param name="pValue">The value.</param>
		public void SetValue(object pValue)
		{
			// Check whether the property is not readonly
			if (IsReadOnly) return;

			object oldValue = GetValue();
			try
			{
				if (pValue != null && pValue.Equals(oldValue)) return;

				if (PropertyType == typeof (object) || pValue == null && PropertyType.IsClass || pValue != null && PropertyType.IsInstanceOfType(pValue))
					SetValueCore(pValue);
				else if (pValue != null && pValue is NamedBrush)
				{
					SetValueCore(((NamedBrush)pValue).Brush);
				}
				else if (pValue != null)
				{
					object convertedValue = Converter.ConvertFrom(pValue);
					SetValueCore(convertedValue);
				}
				OnValueChanged(oldValue, GetValue());
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
				// TODO: Provide error feedback!
			}
			OnPropertyChanged("PropertyValue");
		}
		#endregion

		/// <summary>
		/// Releases unmanaged and - optionally - managed resources
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		protected override void Dispose(bool disposing)
		{
			if (!Disposed)
			{
				if (disposing)
					descriptor.RemoveValueChanged(component, ComponentValueChanged);
				base.Dispose(disposing);
			}
		}

		#region Public helpers

		/// <summary>
		/// Gets the attribute bound to property.
		/// </summary>
		/// <typeparam name="T">Attribute type to look for</typeparam>
		/// <returns>Attribute bound to property or null.</returns>
		public virtual T GetAttribute<T>() where T : Attribute
		{
			return Attributes == null ? null : Attributes[typeof (T)] as T;
		}

		#endregion

		#region Private helpers
		private static object GetViaCustomTypeDescriptor(object obj, PropertyDescriptor descriptor)
		{
			ICustomTypeDescriptor customTypeDescriptor = obj as ICustomTypeDescriptor;
			return customTypeDescriptor != null ? customTypeDescriptor.GetPropertyOwner(descriptor) : obj;
		}

		/// <summary>
		/// Validates the specified value.
		/// </summary>
		/// <param name="pValue">The value.</param>
		/// <returns>
		/// 	<c>true</c> if value can be applied for the property; otherwise, <c>false</c>.
		/// </returns>  
		public bool Validate(object pValue)
		{
			return IsValidDependencyPropertyValue(descriptor, pValue);
		}

		private static bool IsValidDependencyPropertyValue(PropertyDescriptor descriptor, object value)
		{
			bool result = true;

			DependencyPropertyDescriptor dpd = DependencyPropertyDescriptor.FromProperty(descriptor);
			if (dpd != null && dpd.DependencyProperty != null)
				result = dpd.DependencyProperty.IsValidValue(value);

			return result;
		}

		#endregion
	}
}
