using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DarkDocumentStore.Extensions
{
	static class ObjectExtensions
	{
		public static String ToJson(this Object self)
		{
			return "json";
		}

		public static int ToInt(this Object self)
		{
			return Convert.ToInt32(self);
		}
	}
}
