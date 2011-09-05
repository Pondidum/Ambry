using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Ambry.Extensions;

namespace Ambry
{
	public class IndexManager
	{
		private readonly DB _db;

		public IndexManager(DB db)
		{
			_db = db;
		}

		public void Insert<TRecord>(DbConnection connection, TRecord record, DateTime date) where TRecord : Record
		{

			if (!record.ID.HasValue)
			{
				return;  //uh oh.
			}

			var indexes = GetIndexesFor<TRecord>(connection);
			if (!indexes.Any()) return;

			foreach (var index in indexes)
			{
				var propertyName = GetIndexPropertyName(index);
				var value = record.GetPropertyValue(propertyName);

				CreateIndexEntry(connection, record, index, date, value);
			}
		}

		public void Update<TRecord>(DbConnection connection, TRecord record, DateTime date) where TRecord : Record
		{

			if (!record.ID.HasValue)
			{
				return;  //uh oh.
			}

			var indexes = GetIndexesFor<TRecord>(connection);
			if (!indexes.Any()) return;

			foreach (var index in indexes)
			{
				var propertyName = GetIndexPropertyName(index);
				var value = record.GetPropertyValue(propertyName);

				UpdateIndexEntry(connection, record, index, date, value);  //only diff between create and update for indexes
			}
		}


		public void Delete<TRecord>(DbConnection connection, TRecord record) where TRecord : Record
		{
			var indexes = GetIndexesFor<TRecord>(connection);

			foreach (var index in indexes)
			{
				DeleteIndexEntry(connection, index, record);
			}
		}

		private void CreateIndexEntry(DbConnection connection, Record record, String indexName, DateTime date, Object value)
		{
			var sb = new StringBuilder();

			sb.AppendLine("Insert Into {0} (EntryID, EntryUpdated, Value) ", indexName);
			sb.AppendLine("Values (@entryID, @date, @value)");

			using (var command = _db.CreateCommand(connection, sb.ToString()))
			{
				command.Parameters.Add(_db.CreateParameter("entryID", record.ID));
				command.Parameters.Add(_db.CreateParameter("date", date));
				command.Parameters.Add(_db.CreateParameter("value", value));

				command.ExecuteScalar();
			}
		}

		private void UpdateIndexEntry(DbConnection connection, Record record, String indexName, DateTime date, Object value)
		{
			var sb = new StringBuilder();

			sb.AppendLine("Update {0} Set ", indexName);
			sb.AppendLine("EntryUpdated = @date, ");
			sb.AppendLine("Value = @value ");
			sb.AppendLine("Where EntryID = @id");

			using (var command = _db.CreateCommand(connection, sb.ToString()))
			{
				command.Parameters.Add(_db.CreateParameter("date", date));
				command.Parameters.Add(_db.CreateParameter("value", value));
				command.Parameters.Add(_db.CreateParameter("id", record.ID));

				command.ExecuteScalar();
			}
		}

		private void DeleteIndexEntry(DbConnection connection, String indexName, Record record)
		{
			var sb = new StringBuilder();

			sb.AppendLine("Delete From {0} ", indexName);
			sb.AppendLine("Where EntryID = @id");

			using (var command = _db.CreateCommand(connection, sb.ToString()))
			{
				command.Parameters.Add(_db.CreateParameter("id", record.ID));

				command.ExecuteScalar();
			}
		}

		public IList<TRecord> GetByProperty<TRecord>(DbConnection connection, Expression<Func<TRecord, Object>> property, Object value) where TRecord : Record
		{
			var indexes = GetIndexesFor<TRecord>(connection);
			var propertyName = Utilities.PropertyName(property);

			var index = indexes.FirstOrDefault(i => i == propertyName);

			var sb = new StringBuilder();

			sb.AppendLine("Select t.ID, t.Updated, t.Content");
			sb.AppendLine("From {0} t", typeof(TRecord).Name);
			sb.AppendLine("Join {0} i on t.ID = i.EntryID ", index);
			sb.AppendLine("Where Value = @value");

			using (var command = _db.CreateCommand(connection, sb.ToString()))
			{
				command.Parameters.Add(_db.CreateParameter("value", value));

				using (var reader = command.ExecuteReader())
				{
					var results = new List<TRecord>();

					while (reader.Read())
					{
						results.Add(Record.Read<TRecord>(reader));
					}

					return results;
				}
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

			var command = _db.CreateCommand(connection, "Show Tables");

			using (var reader = command.ExecuteReader())
			{
				while (reader.Read())
				{
					tables.Add(reader.GetString(0));
				}
			}

			return tables;
		}

		private static String GetIndexPropertyName(string indexName)
		{
			return indexName.Substring(indexName.LastIndexOf("_") + 1);
		}

	}
}