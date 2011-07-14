using System;
using System.Web.Script.Serialization;
using Ambry.Extensions;

namespace Ambry
{
	internal class JsonSerializerImpl
	{
		private readonly JavaScriptSerializer _serializer;

		internal JsonSerializerImpl()
		{
			_serializer = new JavaScriptSerializer();
		}

		public String Serialize(Object obj)
		{
			Check.Argument(obj, "obj");
			
			return _serializer.Serialize(obj);
		}

		public T Deserialize<T>(String json)
		{
			Check.Argument(json, "json");

			return _serializer.Deserialize<T>(json);
		}
	}
}
