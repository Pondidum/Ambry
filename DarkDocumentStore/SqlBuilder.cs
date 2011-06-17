using System.Text;
using System.Data.Common;
using DarkDocumentStore.Extensions;

namespace DarkDocumentStore
{
	internal class SqlBuilder
	{

		private readonly ISqlProvider _sqlProvider;
		private readonly IndexStore _indexStore;
		private readonly Record _record;
		
		internal SqlBuilder(ISqlProvider sqlProvider, IndexStore indexStore, Record record)
		{
			_sqlProvider = sqlProvider.IfNotNull("sqlProvider");
			_indexStore = indexStore.IfNotNull("indexStore");
			_record = record.IfNotNull("record");
		}

		//public string BuildInsert()
		//{
		//    var indexes = _indexStore.GetIndexesFor(_record);

		//    var factory = DbProviderFactories.GetFactory("");
		//    var command = factory.CreateCommand();

		//    command.CommandText = "";

		//    return "";
		//}

		//public string BuildUpdate()
		//{

		//}

		//public string BuildDelete()
		//{
			
		//}

	}

}
