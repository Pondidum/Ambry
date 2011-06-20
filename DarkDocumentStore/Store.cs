using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using DarkDocumentStore.Extensions;

namespace DarkDocumentStore
{
	public class Store
	{
		private readonly DbProviderFactory _factory;
		private readonly string _connectionString;

		public Store(DbProviderFactory factory, string connectionString)
		{
			_factory = factory.IfNotNull("factory");
			_connectionString = connectionString;
		}

		public void CreateTable<TRecord>() where TRecord : IRecord
		{
			var type = typeof(TRecord);
			var sb = new StringBuilder();

			sb.AppendLine("Create Table {0} (", type.Name);
			sb.AppendLine("  ID       int            not null auto_increment primary key, ");
			sb.AppendLine("  Updated  DateTime       not null, ");
			sb.AppendLine("  Content  Varchar(65535) not null ");
			sb.AppendLine(") Engine=InnoDB");

			using (var connection = OpenConnection())
			{
				CreateCommand(connection, sb.ToString()).ExecuteNonQuery();
			}

		}

		public void CreateIndex<TRecord>(Expression<Func<TRecord, Object>> property) where TRecord : IRecord
		{
			var type = typeof(TRecord);
			var propertyName = PropertyName(property);
			var sb = new StringBuilder();

			sb.AppendLine("Create Table {0} (", GetIndexTableName(type.Name, propertyName));
			sb.AppendLine("  ID            int      not null auto_increment primary key, ");
			sb.AppendLine("  EntryID       int      not null, ");
			sb.AppendLine("  EntryUpdated  DateTime not null, ");
			sb.AppendLine("  Value Varchar(65535)   not null, ");		//make to use property data types
			sb.AppendLine("  Foreign Key (EntryID)  References {0} (ID) on delete cascade", type.Name);
			sb.AppendLine(") Engine=InnoDB");

			using (var connection = OpenConnection())
			{
				CreateCommand(connection, sb.ToString()).ExecuteNonQuery();
			}
		}

		public void DeleteIndex<TRecord>(Expression<Func<TRecord, Object>> property) where TRecord : IRecord
		{
			var type = typeof(TRecord);
			var propertyName = PropertyName(property);

			DeleteIndex(GetIndexTableName(type.Name, propertyName ));
		}

		public void DeleteIndex(String indexName)
		{
			var sql = String.Format("Drop Table {0}", indexName);

			using (var connection = OpenConnection())
			{
				CreateCommand(connection, sql).ExecuteNonQuery();
			}
		}

		public void DeleteTable<TRecord>() where TRecord : IRecord
		{
			var indexes = GetIndexes<TRecord>();

			if (!indexes.Any()) return;

			foreach (var index in indexes)
			{
				DeleteIndex(index);
			}
		}

		public IEnumerable<String> GetIndexes<TRecord>() where TRecord : IRecord
		{
			var type = typeof(TRecord);
			var name = String.Format("Index_{0}_", type.Name);

			return GetAllTables().Where(t => t.StartsWith(name));
		}


		private List<String> GetAllTables()
		{
			var tables = new List<String>();

			using (var connection = OpenConnection())
			{
				var command = CreateCommand(connection, "Show Tables");

				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						tables.Add(reader.GetString(0));
					}
				}
			}

			return tables;
		}

		private String GetIndexTableName(String table, String property)
		{
			return String.Format("Index_{0}_{1}", table, property);
		}

		private DbConnection OpenConnection()
		{
			var connection = _factory.CreateConnection();

			connection.ConnectionString = _connectionString;
			connection.Open();

			return connection;
		}

		private DbCommand CreateCommand(DbConnection connection, String sql)
		{
			var command = _factory.CreateCommand();

			command.Connection = connection;
			command.CommandType = CommandType.Text;
			command.CommandText = sql;

			return command;
		}

		private string PropertyName<T>(Expression<Func<T, Object>> property)
		{
			var lambda = (LambdaExpression)property;

			MemberExpression memberExpression;

			if (lambda.Body is UnaryExpression)
			{
				var unaryExpression = (UnaryExpression)lambda.Body;
				memberExpression = (MemberExpression)unaryExpression.Operand;
			}
			else
			{
				memberExpression = (MemberExpression)lambda.Body;
			}

			return memberExpression.Member.Name;
		}

	}
}