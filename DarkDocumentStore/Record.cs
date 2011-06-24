using System;
using System.Web.Script.Serialization;

namespace DarkDocumentStore
{
	public abstract class Record
	{
		[ScriptIgnore]
		public DateTime? Updated { get; internal set; }

		[ScriptIgnore]
		public int? ID { get; internal set; }

		internal String ToJson()
		{
			
		}
	}
}
