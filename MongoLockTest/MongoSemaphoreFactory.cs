using MongoDB.Bson;
using MongoDB.Driver;

namespace MongoLockTest
{
    public class MongoSemaphoreFactory
    {
        private readonly MongoClient _mongoClient;
        private readonly IMongoDatabase _database;
        private readonly IMongoCollection<MongoSemaphoreEntity> _collection;

        private readonly TimeSpan defaultLockDuration = TimeSpan.FromSeconds(60);

        public MongoSemaphoreFactory()
        {
            _mongoClient = new MongoClient($"mongodb://localhost:27017");
            _database = _mongoClient.GetDatabase("lockTestDb");
            _collection = _database.GetCollection<MongoSemaphoreEntity>("lockCollection");
        }


        public MongoSemaphore CreateSemaphore(ObjectId lockId, TimeSpan? lockDuration = default)
        {
            if (lockDuration == default)
            {
                lockDuration = defaultLockDuration;
            }

            return new MongoSemaphore(_collection, lockId, lockDuration.Value);
        }
    }
}
