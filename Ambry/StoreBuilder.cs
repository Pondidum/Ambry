using System;
using System.Data.Common;
using System.Linq.Expressions;
using System.Text;
using Ambry.Extensions;

namespace Ambry
{
	public class StoreBuilder
	{
		private readonly DB _db ;
		private readonly SqlProviders.MySqlProvider _sqlProvider;

		public StoreBuilder(DB db, SqlProviders.MySqlProvider sqlProvider)
		{
			_db = db.IfNotNull("db");
			_sqlProvider = sqlProvider.IfNotNull("sqlProvider");
		}

		public void CreateTable<TRecord>() where TRecord : Record
		{
			var sb = new StringBuilder();

			sb.AppendLine("Create Table {0} (", typeof(TRecord).Name);
			sb.AppendLine("  ID       int            not null auto_increment primary key, ");
			sb.AppendLine("  Updated  DateTime       not null, ");
			sb.AppendLine("  Content  Varchar(65535) not null ");
			sb.AppendLine(") Engine=InnoDB");

			using (var connection = _db.OpenConnection())
			{
				_db.CreateCommand(connection, sb.ToString()).ExecuteNonQuery();
			}

		}

		public void CreateIndex<TRecord>(Expression<Func<TRecord, Object>> property) where TRecord : Record
		{
			var type = typeof(TRecord);
			var propertyName = Utilities.GetPropertyName(property);
			var propertyType = Utilities.GetPropertyType(property);

			var sb = new StringBuilder();

			sb.AppendLine("Create Table {0} (", IndexManager.GetIndexTableName(type.Name, propertyName));
			sb.AppendLine("  ID            int      not null auto_increment primary key, ");
			sb.AppendLine("  EntryID       int      not null, ");
			sb.AppendLine("  EntryUpdated  DateTime not null, ");
			sb.AppendLine("  Value {0}              not null, ", _sqlProvider.GetSqlTypeFor(propertyType));		//make to use property data types   //Varchar(65535)
			sb.AppendLine("  Foreign Key (EntryID)  References {0} (ID) on delete cascade", type.Name);
			sb.AppendLine(") Engine=InnoDB");

			using (var connection = _db.OpenConnection())
			{
				_db.CreateCommand(connection, sb.ToString()).ExecuteNonQuery();
			}
		}

		public void DeleteTable<TRecord>() where TRecord : Record
		{
			var type = typeof(TRecord);

			using (var connection = _db.OpenConnection())
			{
				var indexManager = new IndexManager(_db);
				var indexes = indexManager.GetIndexesFor<TRecord>(connection);

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
			var propertyName = Utilities.GetPropertyName(property);
			var indexName = IndexManager.GetIndexTableName(type.Name, propertyName);

			using (var connection = _db.OpenConnection())
			{
				DeleteIndex(connection, indexName);
			}
		}




		private void DeleteTable(DbConnection connection, String tableName)
		{
			Check.Argument(tableName, "tableName");

			var sql = String.Format("Drop Table {0}", tableName);

			_db.CreateCommand(connection, sql).ExecuteNonQuery();
		}

		private void DeleteIndex(DbConnection connection, String indexName)
		{
			Check.Argument(indexName, "indexName");

			var sql = String.Format("Drop Table {0}", indexName);

			_db.CreateCommand(connection, sql).ExecuteNonQuery();
		}

		//private static String GetIndexTableName(String table, String property)
		//{
		//    return String.Format("Index_{0}_{1}", table, property);
		//}

		
	}
}
