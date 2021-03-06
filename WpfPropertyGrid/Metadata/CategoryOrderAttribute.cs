﻿/*
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
	/// Specifies the order of category.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true, Inherited = true)]
	public sealed class CategoryOrderAttribute : Attribute
	{
		// dmh - add localization support
		private readonly Type resourceType;
		private string categoryName;

		/// <summary>
		/// Gets the category name.
		/// </summary>
		public string Category 
		{ 
			// dmh - modified to support localization
			get
			{
				return resourceType == null ? categoryName : LocalizationResourceHelper.LookupResource(resourceType, categoryName);
			}
			private set { categoryName = value; }
		}

		/// <summary>
		/// Gets the category order.
		/// </summary>
		/// <value>The order.</value>
		public int Order { get; private set; }

		// dmh - add public getter for resource type (FxCop)
		public Type ResourceType { get { return resourceType; } }

		/// <summary>
		/// Initializes a new instance of the <see cref="CategoryOrderAttribute"/> class.
		/// </summary>
		/// <param name="category"></param>
		/// <param name="order">The order.</param>
		public CategoryOrderAttribute(string category, int order)
		{
			Category	= category;
			Order		= order;
		}

		// dmh - add localization support
		public CategoryOrderAttribute(Type resourceType, string category, int order)
		{
			this.resourceType = resourceType;
			Category	= category;
			Order		= order;
		}		
	}
}
