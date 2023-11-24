using Catalog.Core.Entities;
using Catalog.Core.Specs;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace Catalog.Infrastructure.Data;

public class CatalogContext : ICatalogContext
{
	public IMongoCollection<Product> Products { get; }
	public IMongoCollection<ProductBrand> Brands { get; }
	public IMongoCollection<ProductType> Types { get; }

	public CatalogContext(IConfiguration configuration)
	{
		var client = new MongoClient(configuration.GetValue<string>("DatabaseSettings:ConnectionString"));
		var database = client.GetDatabase(configuration.GetValue<string>("DatabaseSettings:DatabaseName"));

		Brands = InitializeCollection<ProductBrand>(new CollectionProperties(database, configuration, "BrandsCollection", "brands.json"));
		Types = InitializeCollection<ProductType>(new CollectionProperties(database, configuration, "TypesCollection", "types.json"));
		Products = InitializeCollection<Product>(new CollectionProperties(database, configuration, "CollectionName", "products.json"));
	}

	private static IMongoCollection<T> InitializeCollection<T>(CollectionProperties collectionProperties)
	{
		var collectionName = collectionProperties.Configuration.GetValue<string>($"DatabaseSettings:{collectionProperties.CollectionKey}");
		var collection = collectionProperties.Database.GetCollection<T>(collectionName);

		DataSeeder<T>.SeedData(collection, Path.Combine("Data", "SeedData", collectionProperties.SeedFileName));

		return collection;
	}
}