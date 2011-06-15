using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DarkDocumentStore.Extensions;

namespace DarkDocumentStore
{
	class IndexStore
	{
		private readonly Dictionary<string, List<IndexData>> _indexes;

		public IndexStore()
		{
			_indexes = new Dictionary<string, List<IndexData>>();
		}

		public void LoadIndexes()
		{
			_indexes.Clear();

		}

		public IList<IndexData> GetIndexesFor(Record record)
		{
			Check.Argument(record, "record");

			if (_indexes.ContainsKey(record.Name) )
			{
				return _indexes[record.Name].ToList();
			}

			return new List<IndexData>(0);
		}
	}
}
