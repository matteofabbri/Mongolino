using System;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;

namespace Mongolino
{
    public class GridFS
    {
        public static string ConnectionString { get; set; }

        public static string Database { get; set; } = "gridfs";

        static readonly Lazy<GridFSBucket> _bucket = new Lazy<GridFSBucket>(() =>
        {
            var client = ConnectionString != null ? new MongoClient(ConnectionString) : new MongoClient();
            var database = client.GetDatabase(Database);
            return new GridFSBucket(database);
        });

        public static GridFSBucket Bucket => _bucket.Value;
    }
}
