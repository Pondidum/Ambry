using System;
using System.Data;
using System.Web.Script.Serialization;

namespace Ambry
{
	public abstract class Record
	{
		[ScriptIgnore]
		public DateTime? Updated { get; internal set; }

		[ScriptIgnore]
		public int? ID { get; internal set; }

		internal static TRecord Read<TRecord>(IDataReader reader) where TRecord : Record
		{
			var id = reader.GetInt32(0);
			var updated = reader.GetDateTime(1);
			var json = reader.GetString(2);

			var result = JsonSerializer.Deserialize<TRecord>(json);
			result.ID = id;
			result.Updated = updated;

			return result;
		}
	}
}
