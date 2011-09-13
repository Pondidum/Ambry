using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq.Expressions;
using Ambry.Extensions;

namespace Ambry
{
	/// <summary>
	/// Provides the ability to save, load and delete records from the DocumentStore.
	/// </summary>
	public class Store
	{
		private readonly DatabaseConnector _connector;
		private readonly IndexManager _indexes;
		private readonly ContentManager _contents;

		public Store(DbProviderFactory factory, string connectionString)
		{
			_connector = new DatabaseConnector(factory, connectionString);
			_indexes = new IndexManager(_connector);
			_contents = new ContentManager(_connector);
		}

		/// <summary>
		/// Saves a record to the DocumentStore.
		/// </summary>
		/// <typeparam name="TRecord"></typeparam>
		/// <param name="record"></param>
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

		/// <summary>
		/// Deletes a record from the store by it's ID.
		/// </summary>
		/// <typeparam name="TRecord"></typeparam>
		/// <param name="record"></param>
		public void Delete<TRecord>(TRecord record) where TRecord : Record
		{
			Check.Argument(record, "record");

			if (!record.ID.HasValue) throw new InvalidOperationException("The record has no ID, and cannot be deleted.");

			using (var connection = _connector.OpenConnection())
			{
				_indexes.Delete(connection, record);
				_contents.DeleteRecord(connection, record);
			}
		}

		/// <summary>
		/// Gets a single record from the store by it's ID.
		/// </summary>
		/// <typeparam name="TRecord"></typeparam>
		/// <param name="id"></param>
		/// <returns></returns>
		public TRecord GetByID<TRecord>(int id) where TRecord : Record
		{
			Check.ID(id, "ID");

			using (var connection = _connector.OpenConnection())
			{
				return _contents.GetRecordByID<TRecord>(connection, id);
			}
		}

		/// <summary>
		/// Gets a list of records from the store by property value.  The relevant indexes must exist.
		/// </summary>
		/// <typeparam name="TRecord"></typeparam>
		/// <param name="property"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public IList<TRecord> GetByProperty<TRecord>(Expression<Func<TRecord, Object>> property, Object value) where TRecord : Record
		{
			using (var connection = _connector.OpenConnection())
			{
				return _indexes.GetByProperty(connection, property, value);
			}
		}
		


		private void Insert<TRecord>(TRecord record) where TRecord : Record
		{
			Check.Argument(record, "record");

			if (record.ID.HasValue) throw new InvalidOperationException("The record has an ID already, and cannot be inserted.  Did you mean to call Update()?");

			using (var connection = _connector.OpenConnection())
			{
				var date = DateTime.Now;

				_contents.CreateRecord(connection, record, date);
				_indexes.Insert(connection, record, date);
			}
		}

		private void Update<TRecord>(TRecord record) where TRecord : Record
		{
			Check.Argument(record, "record");

			if (!record.ID.HasValue) throw new InvalidOperationException("The record has no ID, and cannot be updated.  Did you mean to call Insert()?");

			using (var connection = _connector.OpenConnection())
			{
				var date = DateTime.Now;

				_contents.UpdateRecord(connection, record, date);
				_indexes.Update(connection, record, date);
			}
		}

	}
}