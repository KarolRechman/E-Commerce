using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace Catalog.Core.Specs;

public class CollectionProperties
{
	public CollectionProperties(IMongoDatabase database, IConfiguration configuration, string collectionKey, string seedFileName)
	{
		Database = database;
		Configuration = configuration;
		CollectionKey = collectionKey;
		SeedFileName = seedFileName;
	}

	public IMongoDatabase Database { get; }
	public IConfiguration Configuration { get; }
	public string CollectionKey { get; }
	public string SeedFileName { get; }
}