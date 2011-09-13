using System;
using System.Linq;
using System.Reflection;

namespace Ambry.Extensions
{
	internal static class ObjectExtensions
	{
		internal static int ToInt(this Object self)
		{
			return Convert.ToInt32(self);
		}

		//Some kind of caching to avoid repeated reflection might be needed. Dictionary<Type, Dictionary<PropertyName, func()>> should do it.
		internal static Object GetPropertyValue(this Object self, String propertyName)
		{
			Check.Argument(self, "self");
			Check.Argument(propertyName, "propertyName");

			var type = self.GetType();
			var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
			var readableNoParameters = properties.Where(p => p.CanRead & !p.GetIndexParameters().Any());
			var specified = readableNoParameters.FirstOrDefault(p => String.Equals(p.Name, propertyName, StringComparison.OrdinalIgnoreCase));

			if (specified != null)
			{
				return specified.GetValue(self, null);
			}

			return null;
		}
	}
}
