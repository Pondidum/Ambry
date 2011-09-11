using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ambry.SqlProviders
{
	public class MySqlProvider
	{

		private static readonly Dictionary<Type, String> TypeMap;

		static MySqlProvider()
		{
			TypeMap = new Dictionary<Type, string>();

			TypeMap[typeof(String)] = "Varchar(65535)";
			TypeMap[typeof(DateTime)] = "DateTime";
			TypeMap[typeof(int)] = "int";

		}

		public String GetSqlTypeFor(Type type)
		{
			if (!TypeMap.ContainsKey(type))
			{
				throw new NotSupportedException(String.Format("The type {0} is not supported yet.", type.Name));
			}

			return TypeMap[type];
		}

	}
}
