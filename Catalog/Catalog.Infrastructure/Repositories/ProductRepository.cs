using Catalog.Core.Entities;
using Catalog.Core.Repositories;
using Catalog.Core.Specs;
using Catalog.Infrastructure.Data;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Catalog.Infrastructure.Repositories;

public class ProductRepository : IProductRepository, IBrandRepository, ITypesRepository
{
	private readonly ICatalogContext _context;

	public ProductRepository(ICatalogContext context)
	{
		_context = context;
	}

	public async Task<Pagination<Product>> GetProducts(CatalogSpecParams catalogSpecParams)
	{
		var filter = BuildFilter(catalogSpecParams);
		var sortedData = await ApplySorting(catalogSpecParams, filter);

		return new Pagination<Product>
		{
			PageSize = catalogSpecParams.PageSize,
			PageIndex = catalogSpecParams.PageIndex,
			Data = sortedData,
			Count = await _context.Products.CountDocumentsAsync(filter)
		};
	}

	private static FilterDefinition<Product> BuildFilter(CatalogSpecParams catalogSpecParams)
	{
		var builder = Builders<Product>.Filter;
		var filter = builder.Empty;

		var filterStrategies = new Dictionary<string, Func<string, FilterDefinition<Product>>>
		{
			{ nameof(CatalogSpecParams.Search), search => builder.Regex(x => x.Name, new BsonRegularExpression(search)) },
			{ nameof(CatalogSpecParams.BrandId), brandId => builder.Eq(x => x.Brands.Id, brandId) },
			{ nameof(CatalogSpecParams.TypeId), typeId => builder.Eq(x => x.Types.Id, typeId) }
		};

		foreach (string property in filterStrategies.Keys)
		{
			var propertyValue = (string)catalogSpecParams.GetType().GetProperty(property)?.GetValue(catalogSpecParams);
			if (!string.IsNullOrEmpty(propertyValue) && filterStrategies.TryGetValue(property, out var strategy))
			{
				filter &= strategy.Invoke(propertyValue);
			}
		}

		return filter;
	}

	private async Task<IReadOnlyList<Product>> ApplySorting(CatalogSpecParams catalogSpecParams, FilterDefinition<Product> filter)
	{
		var sortDefinition = catalogSpecParams.Sort switch
		{
			SortingOptions.PriceAsc => Builders<Product>.Sort.Ascending("Price"),
			SortingOptions.PriceDesc => Builders<Product>.Sort.Descending("Price"),
			_ => Builders<Product>.Sort.Ascending("Name")
		};

		return await _context.Products.Find(filter)
			.Sort(sortDefinition)
			.Skip(catalogSpecParams.PageSize * (catalogSpecParams.PageIndex - 1))
			.Limit(catalogSpecParams.PageSize)
			.ToListAsync();
	}


	public async Task<Product> GetProduct(string id)
	{
		return await _context
			.Products
			.Find(p => p.Id == id)
			.FirstOrDefaultAsync();
	}

	public async Task<IEnumerable<Product>> GetProductByName(string name)
	{
		FilterDefinition<Product> filter = Builders<Product>.Filter.Eq(p => p.Name, name);

		return await _context
			.Products
			.Find(filter)
			.ToListAsync();

	}

	public async Task<IEnumerable<Product>> GetProductByBrand(string name)
	{
		FilterDefinition<Product> filter = Builders<Product>.Filter.Eq(p => p.Brands.Name, name);

		return await _context
			.Products
			.Find(filter)
			.ToListAsync();
	}

	public async Task<Product> CreateProduct(Product product)
	{
		await _context.Products.InsertOneAsync(product);
		return product;
	}

	public async Task<bool> UpdateProduct(Product product)
	{
		var updateResult = await _context
			.Products
			.ReplaceOneAsync(p => p.Id == product.Id, product);

		return updateResult.IsAcknowledged && updateResult.ModifiedCount > 0;
	}

	public async Task<bool> DeleteProduct(string id)
	{
		FilterDefinition<Product> filter = Builders<Product>.Filter.Eq(p => p.Id, id);

		DeleteResult deleteResult = await _context
			.Products
			.DeleteOneAsync(filter);

		return deleteResult.IsAcknowledged && deleteResult.DeletedCount > 0;
	}

	public async Task<IEnumerable<ProductBrand>> GetAllBrands()
	{
		return await _context
			.Brands
			.Find(b => true)
			.ToListAsync();
	}

	public async Task<IEnumerable<ProductType>> GetAllTypes()
	{
		return await _context
			.Types
			.Find(t => true)
			.ToListAsync();
	}
}