using System;

namespace Ambry
{
	internal class JsonSerializer
	{
		private static readonly JsonSerializerImpl Serializer;

		static JsonSerializer()
		{
			Serializer = new JsonSerializerImpl();
		}

		public static String Serialize(Object obj)
		{
			return Serializer.Serialize(obj);
		}

		public  static T Deserialize<T>(String json)
		{
			return Serializer.Deserialize<T>(json);
		}

	}
}
