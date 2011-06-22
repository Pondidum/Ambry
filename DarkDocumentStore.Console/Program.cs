using System;
using System.Data;
using System.Data.Common;

namespace DarkDocumentStore.Console
{
	class Program
	{

		private class TestRecord : Record
		{
			public string Name { get; set; }
		}

		private const string ConnectionString = "Server=192.168.0.145;Database=DarkDataStore;Uid=datastore;Pwd=testing;";

		static void Main(string[] args)
		{

			try
			{
				var factory = DbProviderFactories.GetFactory("MySql.Data.MySqlClient");
				
				var store = new Store(factory, ConnectionString);
				//var builder = new StoreBuilder(store);

				//builder.CreateTable<TestRecord>();
				//builder.CreateIndex<TestRecord>(r => r.Name);

				
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
