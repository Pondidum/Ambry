using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DarkDocumentStore.Extensions
{
	static class ObjectExtensions
	{
		internal static String ToJson(this Object self)
		{
			return "json";
		}

		internal static int ToInt(this Object self)
		{
			return Convert.ToInt32(self);
		}

		//return type eventually needs to be the actual type.
		//also some kind of caching to avoid repeated reflection might be needed. Dictionary<Type, Dictionary<PropertyName, func()>> should do it.
		internal static String GetPropertyValue(this Object self, String propertyName)
		{
			Check.Argument(self, "self");
			Check.Argument(propertyName, "propertyName");

			var type = self.GetType();
			var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
			var readableNoParameters = properties.Where(p => p.CanRead & !p.GetIndexParameters().Any());
			var specified = readableNoParameters.FirstOrDefault(p => String.Equals(p.Name, propertyName, StringComparison.OrdinalIgnoreCase));

			if (specified != null)
			{
				return Convert.ToString(specified.GetValue(self, null));
			}

			return String.Empty;
		}
	}
}
