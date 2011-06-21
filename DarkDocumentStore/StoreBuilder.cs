using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using DarkDocumentStore.Extensions;

namespace DarkDocumentStore
{
	public class StoreBuilder
	{
		private readonly Store _store;

		public StoreBuilder(Store store)
		{
			_store = store.IfNotNull("store");
		}

		public void CreateTable<TRecord>() where TRecord : Record
		{
			var sb = new StringBuilder();

			sb.AppendLine("Create Table {0} (", typeof(TRecord).Name);
			sb.AppendLine("  ID       int            not null auto_increment primary key, ");
			sb.AppendLine("  Updated  DateTime       not null, ");
			sb.AppendLine("  Content  Varchar(65535) not null ");
			sb.AppendLine(") Engine=InnoDB");

			using (var connection = _store.OpenConnection())
			{
				_store.CreateCommand(connection, sb.ToString()).ExecuteNonQuery();
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

			using (var connection = _store.OpenConnection())
			{
				_store.CreateCommand(connection, sb.ToString()).ExecuteNonQuery();
			}
		}

		public void DeleteTable<TRecord>() where TRecord : Record
		{
			var type = typeof(TRecord);
			var indexes = _store.GetIndexesFor<TRecord>();

			if (!indexes.Any()) return;

			using (var connection = _store.OpenConnection())
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

			using (var connection = _store.OpenConnection())
			{
				DeleteIndex(connection, indexName);
			}
		}




		private void DeleteTable(DbConnection connection, String tableName)
		{
			Check.Argument(tableName, "tableName");

			var sql = String.Format("Drop Table {0}", tableName);

			_store.CreateCommand(connection, sql).ExecuteNonQuery();
		}

		private void DeleteIndex(DbConnection connection, String indexName)
		{
			Check.Argument(indexName, "indexName");

			var sql = String.Format("Drop Table {0}", indexName);

			_store.CreateCommand(connection, sql).ExecuteNonQuery();
		}



		private static String GetIndexTableName(String table, String property)
		{
			return String.Format("Index_{0}_{1}", table, property);
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
