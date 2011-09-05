using System;
using System.Data.Common;

namespace Ambry.Extensions
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
