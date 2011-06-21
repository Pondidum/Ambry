using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
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

		public void CreateTable<TRecord>() where TRecord : Record
		{
			var sb = new StringBuilder();

			sb.AppendLine("Create Table {0} (", typeof(TRecord).Name);
			sb.AppendLine("  ID       int            not null auto_increment primary key, ");
			sb.AppendLine("  Updated  DateTime       not null, ");
			sb.AppendLine("  Content  Varchar(65535) not null ");
			sb.AppendLine(") Engine=InnoDB");

			using (var connection = OpenConnection())
			{
				CreateCommand(connection, sb.ToString()).ExecuteNonQuery();
			}

		}

		public void CreateIndex<TRecord>(Expression<Func<TRecord, Object>> property) where TRecord : Record
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

		public void DeleteTable<TRecord>() where TRecord : Record
		{
			var type = typeof(TRecord);
			var indexes = GetIndexesFor<TRecord>();

			if (!indexes.Any()) return;

			using (var connection = OpenConnection())
			{
				foreach (var index in indexes)
				{
					DeleteIndex(connection, index);
				}

				DeleteTable(connection, type.Name);
			}
		}

		public void DeleteIndex<TRecord>(Expression<Func<TRecord, Object>> property) where TRecord : Record
		{
			var type = typeof(TRecord);
			var propertyName = PropertyName(property);
			var indexName = GetIndexTableName(type.Name, propertyName);

			using (var connection = OpenConnection())
			{
				DeleteIndex(connection, indexName);
			}
		}

		public IEnumerable<String> GetIndexesFor<TRecord>() where TRecord : Record
		{
			var type = typeof(TRecord);
			var name = String.Format("Index_{0}_", type.Name);

			return GetAllTables().Where(t => t.StartsWith(name));
		}



		public void Insert<TRecord>(TRecord record) where TRecord : Record
		{
			Check.Argument(record, "record");

			if (record.ID.HasValue) throw new InvalidOperationException("The record has an ID already, and cannot be inserted.  Did you mean to call Update()?");

			using (var connection = OpenConnection())
			{
				var date = DateTime.Now;
				var id = InsertTable(connection, record, date);

				if (!id.HasValue)
				{
					return;  //uh oh.
				}

				var indexes = GetIndexesFor<TRecord>();

				if (!indexes.Any()) return;

				foreach (var index in indexes)
				{
					var propertyName = GetIndexPropertyName(index);
					var value = record.GetPropertyValue(propertyName);

					InsertIndex(connection, record, index, date, value);
				}
			}
		}

		private int? InsertTable(DbConnection connection, Record record, DateTime date)
		{
			var sb = new StringBuilder();

			sb.AppendLine("Insert Into {0} (Updated, Content) ", record.GetType().Name);
			sb.AppendLine("Values (@date , @content);");

			sb.AppendLine("Select LAST_INSERT_ID()");

			using (var command = CreateCommand(connection, sb.ToString()))
			{
				command.Parameters.Add(_factory.CreateParameter("date", date));
				command.Parameters.Add(_factory.CreateParameter("content", record.ToJson()));

				record.ID = command.ExecuteScalar().ToInt();
				record.Updated = date;

				return record.ID;
			}

		}

		private void InsertIndex(DbConnection connection, Record record, String indexName, DateTime date, Object value)
		{
			var sb = new StringBuilder();

			sb.AppendLine("Insert Into {0} (EntryID, EntryUpdated, Value) ", indexName);
			sb.AppendLine("Values (@entryID, @date, @value)");

			using (var command = CreateCommand(connection, sb.ToString()))
			{
				command.Parameters.Add(_factory.CreateParameter("entryID", record.ID));
				command.Parameters.Add(_factory.CreateParameter("date", date));
				command.Parameters.Add(_factory.CreateParameter("value", value));

				command.ExecuteScalar();
			}
		}



		public void Update<TRecord>(TRecord record) where TRecord : Record
		{
			Check.Argument(record, "record");

			if (!record.ID.HasValue) throw new InvalidOperationException("The record has no ID, and cannot be updated.  Did you mean to call Insert()?");

			using (var connection = OpenConnection())
			{
				var date = DateTime.Now;

				UpdateTable(connection, record, date);

				var indexes = GetIndexesFor<TRecord>();

				if (!indexes.Any()) return;

				foreach (var index in indexes)
				{
					var propertyName = GetIndexPropertyName(index);
					var value = record.GetPropertyValue(propertyName);

					UpdateIndex(connection, record, index, date, value);
				}
			}
		}

		private void UpdateTable(DbConnection connection, Record record, DateTime date)
		{
			var sb = new StringBuilder();

			sb.AppendLine("Update {0} Set ", record.GetType().Name);
			sb.AppendLine("Updated = @date, ");
			sb.AppendLine("Content = @content ");
			sb.AppendLine("Where ID = @id");

			using (var command = CreateCommand(connection, sb.ToString()))
			{
				command.Parameters.Add(_factory.CreateParameter("date", date));
				command.Parameters.Add(_factory.CreateParameter("content", record.ToJson()));
				command.Parameters.Add(_factory.CreateParameter("id", record.ID));

				command.ExecuteScalar();

				record.Updated = date;
			}
		}

		private void UpdateIndex(DbConnection connection, Record record, String indexName, DateTime date, Object value)
		{
			var sb = new StringBuilder();

			sb.AppendLine("Update {0} Set ", indexName);
			sb.AppendLine("EntryUpdated = @date, ");
			sb.AppendLine("Value = @value ");
			sb.AppendLine("Where EntryID = @id");

			using (var command = CreateCommand(connection, sb.ToString()))
			{
				command.Parameters.Add(_factory.CreateParameter("date", date));
				command.Parameters.Add(_factory.CreateParameter("value", value));
				command.Parameters.Add(_factory.CreateParameter("id", record.ID));

				command.ExecuteScalar();
			}
		}



		private void DeleteTable(DbConnection connection, String tableName)
		{
			Check.Argument(tableName, "tableName");

			var sql = String.Format("Drop Table {0}", tableName);

			CreateCommand(connection, sql).ExecuteNonQuery();
		}

		private void DeleteIndex(DbConnection connection, String indexName)
		{
			Check.Argument(indexName, "indexName");

			var sql = String.Format("Drop Table {0}", indexName);

			CreateCommand(connection, sql).ExecuteNonQuery();
		}



		private IEnumerable<String> GetAllTables()
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



		private static String GetIndexTableName(String table, String property)
		{
			return String.Format("Index_{0}_{1}", table, property);
		}

		private static String GetIndexPropertyName(string indexName)
		{
			return indexName.Substring(indexName.LastIndexOf("_") + 1);
		}

		private static string PropertyName<T>(Expression<Func<T, Object>> property)
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