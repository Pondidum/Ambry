using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using Ambry.Extensions;

namespace Ambry
{
	public class DB
	{
		private readonly DbProviderFactory _factory;
		private readonly string _connectionString;

		public DB(DbProviderFactory factory, string connectionString)
		{
			_factory = factory.IfNotNull("factory");
			_connectionString = connectionString;
		}

		public DbConnection OpenConnection()
		{
			var connection = _factory.CreateConnection();

			connection.ConnectionString = _connectionString;
			connection.Open();

			return connection;
		}

		public DbCommand CreateCommand(DbConnection connection, String sql)
		{
			var command = _factory.CreateCommand();

			command.Connection = connection;
			command.CommandType = CommandType.Text;
			command.CommandText = sql;

			return command;
		}

		public DbParameter CreateParameter(String name, Object value)
		{
			var param = _factory.CreateParameter();

			param.ParameterName = name;
			param.Value = value;

			return param;
		}
	}
}