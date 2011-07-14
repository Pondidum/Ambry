using System;
using System.Web.Script.Serialization;

namespace Ambry
{
	public abstract class Record
	{
		[ScriptIgnore]
		public DateTime? Updated { get; internal set; }

		[ScriptIgnore]
		public int? ID { get; internal set; }

	}
}
