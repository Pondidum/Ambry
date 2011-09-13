using System;
using System.Data.Common;
using System.Linq.Expressions;
using System.Text;
using Ambry.Extensions;

namespace Ambry
{
	public class StoreBuilder
	{
		private readonly DatabaseConnector _connector ;
		private readonly SqlProviders.MySqlProvider _sqlProvider;

		public StoreBuilder(DbProviderFactory factory, string connectionString, SqlProviders.MySqlProvider sqlProvider)
		{
			Check.Argument(factory, "factory");
			Check.Argument(connectionString, "connectionString");
			Check.Argument(sqlProvider, "sqlProvider");

			_connector = new DatabaseConnector(factory, connectionString);
			_sqlProvider = sqlProvider;
		}

		public void CreateTable<TRecord>() where TRecord : Record
		{
			var sb = new StringBuilder();

			sb.AppendLine("Create Table {0} (", typeof(TRecord).Name);
			sb.AppendLine("  ID       int            not null auto_increment primary key, ");
			sb.AppendLine("  Updated  DateTime       not null, ");
			sb.AppendLine("  Content  Varchar(65535) not null ");
			sb.AppendLine(") Engine=InnoDB");

			using (var connection = _connector.OpenConnection())
			{
				_connector.CreateCommand(connection, sb.ToString()).ExecuteNonQuery();
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

			using (var connection = _connector.OpenConnection())
			{
				_connector.CreateCommand(connection, sb.ToString()).ExecuteNonQuery();
			}
		}

		public void DeleteTable<TRecord>() where TRecord : Record
		{
			var type = typeof(TRecord);

			using (var connection = _connector.OpenConnection())
			{
				var indexManager = new IndexManager(_connector);
				var indexes = indexManager.GetIndexesFor(connection, type);

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

			using (var connection = _connector.OpenConnection())
			{
				DeleteIndex(connection, indexName);
			}
		}




		private void DeleteTable(DbConnection connection, String tableName)
		{
			Check.Argument(tableName, "tableName");

			var sql = String.Format("Drop Table {0}", tableName);

			_connector.CreateCommand(connection, sql).ExecuteNonQuery();
		}

		private void DeleteIndex(DbConnection connection, String indexName)
		{
			Check.Argument(indexName, "indexName");

			var sql = String.Format("Drop Table {0}", indexName);

			_connector.CreateCommand(connection, sql).ExecuteNonQuery();
		}
		
	}
}
