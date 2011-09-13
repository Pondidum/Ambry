using System;
using System.Data.Common;
using System.Text;
using Ambry.Extensions;

namespace Ambry
{
	internal class ContentManager
	{
		private readonly DatabaseConnector _db;

		public ContentManager(DatabaseConnector db)
		{
			_db = db;
		}

		internal int? CreateRecord(DbConnection connection, Record record, DateTime date)
		{
			var sb = new StringBuilder();

			sb.AppendLine("Insert Into {0} (Updated, Content) ", record.GetType().Name);
			sb.AppendLine("Values (@date , @content);");

			sb.AppendLine("Select LAST_INSERT_ID()");

			using (var command = _db.CreateCommand(connection, sb.ToString()))
			{
				command.Parameters.Add(_db.CreateParameter("date", date));
				command.Parameters.Add(_db.CreateParameter("content", JsonSerializer.Serialize(record)));

				record.ID = command.ExecuteScalar().ToInt();
				record.Updated = date;

				return record.ID;
			}

		}

		internal void UpdateRecord(DbConnection connection, Record record, DateTime date)
		{
			var sb = new StringBuilder();

			sb.AppendLine("Update {0} Set ", record.GetType().Name);
			sb.AppendLine("Updated = @date, ");
			sb.AppendLine("Content = @content ");
			sb.AppendLine("Where ID = @id");

			using (var command = _db.CreateCommand(connection, sb.ToString()))
			{
				record.Updated = date;

				command.Parameters.Add(_db.CreateParameter("date", date));
				command.Parameters.Add(_db.CreateParameter("content", JsonSerializer.Serialize(record)));
				command.Parameters.Add(_db.CreateParameter("id", record.ID));

				command.ExecuteScalar();
			}
		}

		internal void DeleteRecord(DbConnection connection, Record record)
		{
			var sb = new StringBuilder();

			sb.AppendLine("Delete From {0} ", record.GetType().Name);
			sb.AppendLine("Where ID = @id");

			using (var command = _db.CreateCommand(connection, sb.ToString()))
			{
				command.Parameters.Add(_db.CreateParameter("id", record.ID));

				command.ExecuteScalar();
				record.ID = null;
			}
		}

		internal TRecord GetRecordByID<TRecord>(DbConnection connection, int id) where TRecord : Record
		{
			var sb = new StringBuilder();

			sb.AppendLine("Select ID, Updated, Content");
			sb.AppendLine("From {0} ", typeof(TRecord).Name);
			sb.AppendLine("Where ID = @id");

			using (var command = _db.CreateCommand(connection, sb.ToString()))
			{
				command.Parameters.Add(_db.CreateParameter("id", id));

				using (var reader = command.ExecuteReader())
				{
					reader.Read();

					return Record.Read<TRecord>(reader);
				}
			}
		}

	}
}