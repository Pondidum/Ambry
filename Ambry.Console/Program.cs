using System;
using System.Data;
using System.Data.Common;
using Ambry.SqlProviders;

namespace Ambry.Console
{
	class Program
	{

		private class TestRecord : Record
		{
			public string Name { get; set; }
		}

		private class TypesRecord : Record
		{
			public String Name { get; set; }
			public DateTime DateOfBirth { get; set; }
			public int Age { get; set; }
		}

		private const string ConnectionString = "Server=192.168.0.145;Database=DarkDataStore;Uid=datastore;Pwd=testing;";

		static void Main(string[] args)
		{

			try
			{
				var factory = DbProviderFactories.GetFactory("MySql.Data.MySqlClient");

				var store = new Store(factory, ConnectionString);
				var builder = new StoreBuilder(factory, ConnectionString, new MySqlProvider());

				//builder.CreateTable<TypesRecord>();
				builder.CreateIndex<TypesRecord>(x => x.Name);
				//builder.CreateIndex<TypesRecord>(x => x.Age);
				//builder.CreateIndex<TypesRecord>(x => x.DateOfBirth);

				var test = new TypesRecord
							{
								Age = 1337,
								DateOfBirth = new DateTime(2011, 10, 09, 08, 07, 6),
								Name = "Testing"
							};

				store.Save(test);
				store.GetByProperty<TypesRecord>(x => x.Age, 21);
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
