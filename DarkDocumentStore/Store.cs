using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
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

		public TRecord GetByID<TRecord>(int id) where TRecord : Record
		{
			Check.ID(id, "ID");

			using (var connection = OpenConnection())
			{
				return GetByID<TRecord>(connection, id);
			}
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

				var indexes = GetIndexesFor<TRecord>(connection);

				if (!indexes.Any()) return;

				foreach (var index in indexes)
				{
					var propertyName = GetIndexPropertyName(index);
					var value = record.GetPropertyValue(propertyName);

					InsertIndex(connection, record, index, date, value);
				}
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

				var indexes = GetIndexesFor<TRecord>(connection);

				if (!indexes.Any()) return;

				foreach (var index in indexes)
				{
					var propertyName = GetIndexPropertyName(index);
					var value = record.GetPropertyValue(propertyName);

					UpdateIndex(connection, record, index, date, value);
				}
			}
		}

		public void Delete<TRecord>(TRecord record) where TRecord : Record
		{
			Check.Argument(record, "record");

			if (!record.ID.HasValue) throw new InvalidOperationException("The record has no ID, and cannot be deleted.");

			using (var connection = OpenConnection())
			{
				var indexes = GetIndexesFor<TRecord>(connection);

				foreach (var index in indexes)
				{
					DeleteIndex(connection, index, record);
				}

				DeleteTable(connection, record);
			}
		}



		private TRecord GetByID<TRecord>(DbConnection connection, int id) where TRecord : Record
		{
			var sb = new StringBuilder();

			sb.AppendLine("Select ID, Updated, Content");
			sb.AppendLine("From {0} ", typeof (TRecord).Name);
			sb.AppendLine("Where ID = @id");

			using (var command = CreateCommand(connection, sb.ToString()))
			{
				command.Parameters.Add(_factory.CreateParameter("id", id));

				using (var reader = command.ExecuteReader())
				{
					reader.Read();

					var result = reader.GetString(2).ToObject<TRecord>();

					return result;
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



		private void UpdateTable(DbConnection connection, Record record, DateTime date)
		{
			var sb = new StringBuilder();

			sb.AppendLine("Update {0} Set ", record.GetType().Name);
			sb.AppendLine("Updated = @date, ");
			sb.AppendLine("Content = @content ");
			sb.AppendLine("Where ID = @id");

			using (var command = CreateCommand(connection, sb.ToString()))
			{
				record.Updated = date;

				command.Parameters.Add(_factory.CreateParameter("date", date));
				command.Parameters.Add(_factory.CreateParameter("content", record.ToJson()));
				command.Parameters.Add(_factory.CreateParameter("id", record.ID));

				command.ExecuteScalar();
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



		private void DeleteTable(DbConnection connection, Record record)
		{
			var sb = new StringBuilder();

			sb.AppendLine("Delete From {0} ", record.GetType().Name);
			sb.AppendLine("Where ID = @id");

			using (var command = CreateCommand(connection, sb.ToString()))
			{
				command.Parameters.Add(_factory.CreateParameter("id", record.ID));

				command.ExecuteScalar();
				record.ID = null;
			}
		}

		private void DeleteIndex(DbConnection connection, String indexName, Record record)
		{
			var sb = new StringBuilder();

			sb.AppendLine("Delete From {0} ", indexName);
			sb.AppendLine("Where EntryID = @id");

			using (var command = CreateCommand(connection, sb.ToString()))
			{
				command.Parameters.Add(_factory.CreateParameter("id", record.ID));

				command.ExecuteScalar();
			}
		}



		internal IEnumerable<String> GetIndexesFor<TRecord>(DbConnection connection) where TRecord : Record
		{
			var type = typeof(TRecord);
			var name = String.Format("Index_{0}_", type.Name);

			return GetAllTables(connection).Where(t => t.StartsWith(name));
		}

		private IEnumerable<String> GetAllTables(DbConnection connection)
		{
			var tables = new List<String>();

			var command = CreateCommand(connection, "Show Tables");

			using (var reader = command.ExecuteReader())
			{
				while (reader.Read())
				{
					tables.Add(reader.GetString(0));
				}
			}

			return tables;
		}



		internal DbConnection OpenConnection()
		{
			var connection = _factory.CreateConnection();

			connection.ConnectionString = _connectionString;
			connection.Open();

			return connection;
		}

		internal DbCommand CreateCommand(DbConnection connection, String sql)
		{
			var command = _factory.CreateCommand();

			command.Connection = connection;
			command.CommandType = CommandType.Text;
			command.CommandText = sql;

			return command;
		}

		private static String GetIndexPropertyName(string indexName)
		{
			return indexName.Substring(indexName.LastIndexOf("_") + 1);
		}

	}
}