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

using System.Reflection;

namespace System.Windows.Controls.WpfPropertyGrid
{
	// dmh - added localization resource utility for localized versions of WPG attributes
	static internal class LocalizationResourceHelper
	{
		public static string LookupResource(IReflect resourceManagerProvider, string resourceKey)
		{
			PropertyInfo property = resourceManagerProvider.GetProperty(resourceKey, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
			// Fallback with the key name
			if (property == null)
				return resourceKey;
			return (string) property.GetValue(null, null); // returns string directly from res file
		}	
	}
}
