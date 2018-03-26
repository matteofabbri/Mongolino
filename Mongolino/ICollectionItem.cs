using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Mongolino
{
    public interface ICollectionItem
    {
        [BsonRepresentation(BsonType.ObjectId)]
        string Id { get; set; }
    }
}
