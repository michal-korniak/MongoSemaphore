using MongoDB.Bson;

namespace MongoLockTest
{
    public class MongoSemaphoreEntity
    {
        public ObjectId Id { get; set; }
        public DateTime EndTime { get; set; }
    }
}
