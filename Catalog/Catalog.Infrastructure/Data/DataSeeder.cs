using System.Text.Json;
using MongoDB.Driver;

namespace Catalog.Infrastructure.Data;

public static class DataSeeder<T>
{
	public static void SeedData(IMongoCollection<T> collection, string dataFilePath)
	{
		bool dataExists = collection.Find(_ => true).Any();

		if (dataExists)
		{
			return;
		}

		InsertData(collection, dataFilePath);
	}

	private static void InsertData(IMongoCollection<T> collection, string dataFilePath)
	{
		string data = File.ReadAllText(dataFilePath);
		var items = JsonSerializer.Deserialize<List<T>>(data);

		if (items == null)
		{
			return;
		}

		InsertItems(collection, items);
	}

	private static void InsertItems(IMongoCollection<T> collection, List<T> items)
	{
		foreach (var item in items)
		{
			collection.InsertOne(item);
		}
	}
}