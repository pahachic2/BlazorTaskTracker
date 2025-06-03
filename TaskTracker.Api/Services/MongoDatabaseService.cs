using MongoDB.Driver;
using System.Linq.Expressions;
using Microsoft.Extensions.Options;
using TaskTracker.Api.Configuration;
using MongoDB.Bson;

namespace TaskTracker.Api.Services;

public class MongoDatabaseService<T> : IDatabaseService<T> where T : class
{
    private readonly IMongoCollection<T> _collection;

    public MongoDatabaseService(IOptions<DatabaseSettings> databaseSettings)
    {
        var mongoClient = new MongoClient(databaseSettings.Value.ConnectionString);
        var mongoDatabase = mongoClient.GetDatabase(databaseSettings.Value.DatabaseName);
        _collection = mongoDatabase.GetCollection<T>(typeof(T).Name.ToLower());
    }

    public async Task<IEnumerable<T>> GetAllAsync() =>
        await _collection.Find(_ => true).ToListAsync();

    public async Task<T?> GetByIdAsync(string id)
    {
        var filter = Builders<T>.Filter.Eq("_id", ObjectId.Parse(id));
        return await _collection.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> filter) =>
        await _collection.Find(filter).ToListAsync();

    public async Task<T> CreateAsync(T entity)
    {
        await _collection.InsertOneAsync(entity);
        return entity;
    }

    public async Task<T> UpdateAsync(string id, T entity)
    {
        var filter = Builders<T>.Filter.Eq("_id", ObjectId.Parse(id));
        await _collection.ReplaceOneAsync(filter, entity);
        return entity;
    }

    public async Task DeleteAsync(string id)
    {
        var filter = Builders<T>.Filter.Eq("_id", ObjectId.Parse(id));
        await _collection.DeleteOneAsync(filter);
    }
} 