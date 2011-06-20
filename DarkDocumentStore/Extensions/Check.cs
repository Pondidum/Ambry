using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DarkDocumentStore.Extensions
{
	static class Check
	{

		public static void Argument(Object self)
		{
			Argument(self, "self");
		}

		public static void Argument(Object self, String name)
		{
			if (self == null)
			{
				throw new ArgumentNullException(name);
			}
		}

		public static void Argument(String self, String name)
		{
			if (string.IsNullOrEmpty(self))
			{
				throw new ArgumentException(name);
			}
		}

		public static T IfNotNull<T>(this T self, String name) where T : class 
		{
			Check.Argument(self, name);
			return self;
		}

	}
}
