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
	/// dmh - Controls the default expanded state of a category in the case of categories being displayed w/ expanders
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true, Inherited = true)]
	public sealed class CategoryExpandedAttribute : Attribute
	{
		private readonly string categoryName;
		private readonly Type resourceType;

		public string Category 
		{ 
			get 
			{ 
				return resourceType != null ? LocalizationResourceHelper.LookupResource(resourceType, categoryName) : categoryName;
			} 
		}
		
		public bool Expanded { get; private set; }
		
		// dmh - add public getter for resource type (FxCop)
		public Type ResourceType { get { return resourceType; } }

		public CategoryExpandedAttribute(Type resourceType, string category, bool expanded)
		{
			this.resourceType = resourceType;
			categoryName = category;
			Expanded = expanded;
		}

		public CategoryExpandedAttribute(string category, bool expanded)
		{
			categoryName = category;
			Expanded = expanded;
		}
	}
}
