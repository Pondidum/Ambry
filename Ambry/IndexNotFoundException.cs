using System;
using System.Runtime.Serialization;

namespace Ambry
{
	[Serializable]
	public class IndexNotFoundException : Exception
	{

		public IndexNotFoundException(String indexName) 
			: base(String.Format("The index '{0}' was not found.", indexName))
		{
		}
		
		protected IndexNotFoundException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}

}
