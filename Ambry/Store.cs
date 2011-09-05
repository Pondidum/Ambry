using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Ambry.Extensions;

namespace Ambry
{
	public class Store
	{
		private readonly DB _db;
		private readonly IndexManager _indexes;
		private readonly ContentManager _contents;

		public Store(DB db)
		{
			_db = db;
			_indexes = new IndexManager(db);
			_contents = new ContentManager(db);
		}


		public void Save<TRecord>(TRecord record) where TRecord : Record
		{
			if (record.ID.HasValue)
			{
				Update(record);
			}
			else
			{
				Insert(record);
			}
		}

		public void Insert<TRecord>(TRecord record) where TRecord : Record
		{
			Check.Argument(record, "record");

			if (record.ID.HasValue) throw new InvalidOperationException("The record has an ID already, and cannot be inserted.  Did you mean to call Update()?");

			using (var connection = _db.OpenConnection())
			{
				var date = DateTime.Now;

				_contents.CreateRecord(connection, record, date);
				_indexes.Insert(connection, record, date);
			}
		}

		public void Update<TRecord>(TRecord record) where TRecord : Record
		{
			Check.Argument(record, "record");

			if (!record.ID.HasValue) throw new InvalidOperationException("The record has no ID, and cannot be updated.  Did you mean to call Insert()?");

			using (var connection = _db.OpenConnection())
			{
				var date = DateTime.Now;

				_contents.UpdateRecord(connection, record, date);
				_indexes.Update(connection, record, date);
			}
		}

		public void Delete<TRecord>(TRecord record) where TRecord : Record
		{
			Check.Argument(record, "record");

			if (!record.ID.HasValue) throw new InvalidOperationException("The record has no ID, and cannot be deleted.");

			using (var connection = _db.OpenConnection())
			{
				_indexes.Delete(connection, record);
				_contents.DeleteRecord(connection, record);
			}
		}

		public TRecord GetByID<TRecord>(int id) where TRecord : Record
		{
			Check.ID(id, "ID");

			using (var connection = _db.OpenConnection())
			{
				return _contents.GetRecordByID<TRecord>(connection, id);
			}
		}

		public IList<TRecord> GetByProperty<TRecord>(Expression<Func<TRecord, Object>> property, Object value) where TRecord : Record
		{
			using (var connection = _db.OpenConnection())
			{
				return _indexes.GetByProperty(connection, property, value);
			}
		}
		
	}
}