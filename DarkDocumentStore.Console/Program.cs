using System;
using System.Data;
using System.Data.Common;

namespace DarkDocumentStore.Console
{
	class Program
	{

		private class TestRecord : Record
		{
			//public int? ID { get; set; }
			public string Name { get; set; }
		}

		static void Main(string[] args)
		{

			try
			{

				var factory = DbProviderFactories.GetFactory("MySql.Data.MySqlClient");
				var connectionString = "Server=192.168.0.145;Database=DarkDataStore;Uid=datastore;Pwd=testing;";

				var store = new Store(factory, connectionString);
				
				//store.CreateTable<TestRecord>();
				//store.CreateIndex<TestRecord>(r => r.Name);


				var record = new TestRecord();
				record.Name = "testing!";

				store.Insert(record);
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
