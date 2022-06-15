using MongoDB.Bson;
using MongoDB.Driver;

namespace MongoLockTest
{
    public class MongoSemaphore : IAsyncDisposable, IDisposable
    {
        private readonly IMongoCollection<MongoSemaphoreEntity> _collection;
        private readonly ObjectId _lockId;
        private readonly TimeSpan _lockDuration;

        public MongoSemaphore(IMongoCollection<MongoSemaphoreEntity> collection, ObjectId lockId, TimeSpan lockDuration)
        {
            _collection = collection;
            _lockId = lockId;
            _lockDuration = lockDuration;
        }

        public async Task WaitAsync(TimeSpan? timeout = default)
        {
            if (timeout == default)
            {
                timeout = Timeout.InfiniteTimeSpan;
            }
            CancellationToken timeoutCancelationToken = new CancellationTokenSource(timeout.Value).Token;

            bool isSucess;
            do
            {
                try
                {
                    await InsertOrUpdateLockEntity(timeoutCancelationToken);
                    isSucess = true;
                }
                catch (MongoWriteException ex) when (ex.WriteError.Code == 11000 && ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
                {
                    isSucess = false;
                    await Task.Delay(TimeSpan.FromSeconds(1), timeoutCancelationToken);
                }
            } while (!isSucess);
        }

        private async Task InsertOrUpdateLockEntity(CancellationToken cancellationToken)
        {
            FilterDefinition<MongoSemaphoreEntity> filter = Builders<MongoSemaphoreEntity>.Filter.And
            (
            Builders<MongoSemaphoreEntity>.Filter.Eq(e => e.Id, _lockId),
                Builders<MongoSemaphoreEntity>.Filter.Lt(e => e.EndTime, DateTime.UtcNow)
            );

            UpdateDefinition<MongoSemaphoreEntity> update = Builders<MongoSemaphoreEntity>.Update
                .Set(e => e.Id, _lockId)
                .Set(e => e.EndTime, DateTime.UtcNow + _lockDuration);

            UpdateOptions updateOptions = new()
            {
                IsUpsert = true
            };

            await _collection.UpdateOneAsync(filter, update, updateOptions, cancellationToken);
        }

        public async Task ReleaseAsync()
        {
            FilterDefinition<MongoSemaphoreEntity> filter = Builders<MongoSemaphoreEntity>.Filter.Eq(e => e.Id, _lockId);
            await _collection.DeleteOneAsync(filter);
        }

        public async ValueTask DisposeAsync()
        {
            await ReleaseAsync();
        }

        public void Dispose()
        {
            ReleaseAsync().Wait();
        }
    }
}