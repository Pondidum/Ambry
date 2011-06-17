using System;
using System.Data;
using System.Data.Common;

namespace DarkDocumentStore.Console
{
	class Program
	{

		private class TestRecord : IRecord
		{
			public int ID { get; set; }
			public string Name { get; set; }
		}

		static void Main(string[] args)
		{

			try
			{

				var factory = DbProviderFactories.GetFactory("MySql.Data.MySqlClient");
				var connectionString = "Server=192.168.0.145;Database=DarkDataStore;Uid=datastore;Pwd=testing;";

				var store = new Store(factory, connectionString);
				var record = new Record(new TestRecord());

				store.CreateTable<TestRecord>();
				store.CreateIndex<TestRecord>(r => r.Name);

				//store.DeleteIndex<TestRecord>(r => r.Name);
			}
			catch (Exception ex)
			{
				System.Console.WriteLine(ex);
			}

			System.Console.WriteLine("Done.");
			System.Console.ReadKey();
		}
	}
}
