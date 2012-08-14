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
using System.Linq;

namespace System.Windows.Controls.WpfPropertyGrid
{
	/// <summary>
	/// Holds custom attributes collection and provides facilities accessing attribute values from UI.
	/// </summary>
	public class AttributesContainer
	{
		private readonly	AttributeCollection			attributes;
		private const		string						attributeSuffix		= "Attribute";
		private readonly	Dictionary<string, Type>	keys				= new Dictionary<string, Type>();

		/// <summary>
		/// Initializes a new instance of the <see cref="AttributesContainer"/> class.
		/// </summary>
		/// <param name="attributes">The collection of attributes.</param>
		public AttributesContainer(AttributeCollection attributes)
		{
			if (attributes == null) throw new ArgumentNullException("attributes");

			this.attributes = attributes;

			foreach (Type type in from Attribute attr in this.attributes select attr.GetType())
				RegisterAttribute(type.Name, type);
		}

		/// <summary>
		/// Registers the attribute within the container.
		/// </summary>
		/// <param name="name">The public name of the attribute. The "Attribute" suffix will be automatically removed.</param>
		/// <param name="attributeType">Type of the attribute.</param>
		/// <returns>true is attribute was successfully registered, otherwise false</returns>
		public bool RegisterAttribute(string name, Type attributeType)
		{
			if (string.IsNullOrEmpty(name)) return false;
			if (attributeType == null) return false;

			string attributeName = name.EndsWith(attributeSuffix, StringComparison.Ordinal)
									? name.Remove(name.Length - attributeSuffix.Length, attributeSuffix.Length) : name;

			if (attributeName.Length == 0) return false;

			if (!keys.ContainsKey(attributeName))
			{
				keys.Add(attributeName, attributeType);
				return true;
			}
			return false;
		}

		/// <summary>
		/// Gets the attribute with the specified key.
		/// </summary>
		/// <value>Attribute with the specified key.</value>
		public object this[string key]
		{
			get
			{
				if (attributes != null)
				{
					Type type;
					if (keys.TryGetValue(key, out type))
						return attributes[type];
				}
				return null;
			}
		}
	}
}
