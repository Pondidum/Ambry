using System;

namespace DarkDocumentStore
{
	public abstract class Record
	{
		public DateTime? Updated { get; internal set; }
		public int? ID { get; internal set; }
	}
}
