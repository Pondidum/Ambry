using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace DarkDocumentStore.Extensions
{
	static class DbProviderFactoryExtensions
	{
		public static DbParameter CreateParameter(this DbProviderFactory self, String name, Object value)
		{
			Check.Argument(self, "self");
			Check.Argument(name, "name");

			var param = self.CreateParameter();

			param.ParameterName = name;
			param.Value = value;

			return param;
		}

	}
}
