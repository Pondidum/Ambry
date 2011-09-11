using System;
using System.Data;
using System.Data.Common;

namespace Ambry.Console
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
				var db = new Ambry.DB(factory, ConnectionString);
				var store = new Store(db);
				//var builder = new StoreBuilder(store);

				//builder.CreateTable<TestRecord>();
				//builder.CreateIndex<TestRecord>(r => r.Name);

				//var obj = new TestRecord();
				//obj.Name = "Dave";

				//store.Save(obj);

				var byID = store.GetByID<TestRecord>(1 );
				var byName = store.GetByProperty<TestRecord>(x => x.Name, "Dave");

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
