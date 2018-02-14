using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace Mongolino
{
    public class DBObject<T> where T : DBObject<T>
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string FullText
        {
            get
            {
                var enu = GetType()
                           .GetProperties()
                           .Where(x => x.PropertyType == typeof(string) && x.Name != "FullText")
                           .SelectMany(x => (x.GetValue(this) as string)?.Split(Helper.Removable.Value) ?? new string[0])
                           .Distinct(StringComparer.InvariantCultureIgnoreCase)
                           .OrderByDescending(x => x);

                return string.Join(" ", enu);
            }
            set
            {
            }
        }

        #region STATICS

        static readonly Lazy<IMongoCollection<T>> _collection = new Lazy<IMongoCollection<T>>(()=> 
        {
            var type = typeof(T);

            var configuration = Extensions.TypesConfiguration.ContainsKey(type.FullName) ? Extensions.TypesConfiguration[type.FullName] : new ConfigurationSection
            {
                Collection = type.Name.ToLowerInvariant(),
                ConnectionString = null,
                Database = "default"
            };

            var client = configuration.ConnectionString != null ? new MongoClient(configuration.ConnectionString) : new MongoClient();
            var collection = client.GetDatabase(configuration.Database).GetCollection<T>(configuration.Collection);
            collection.Indexes.CreateOne(Builders<T>.IndexKeys.Text(x => x.FullText));
            return collection;
        });

        static IMongoCollection<T> MongoCollection => _collection.Value;

        public static void AscendingIndex(Expression<Func<T, object>> func)
        {
            MongoCollection.Indexes.CreateOne(Builders<T>.IndexKeys.Ascending(func));
        }

        public static void DescendingIndex(Expression<Func<T, object>> func)
        {
            MongoCollection.Indexes.CreateOne(Builders<T>.IndexKeys.Descending(func));
        }

        public static IEnumerable<T> FullTextSearch(string text)
        {
            var filter = Builders<T>.Filter.Text(text);
            var res = MongoCollection.Find(filter);
            return res.ToEnumerable();
        }

        public static async Task<IEnumerable<T>> FullTextSearchAsync(string text)
        {
            var filter = Builders<T>.Filter.Text(text);
            var res = await MongoCollection.FindAsync(filter);

            return res.ToEnumerable();
        }

        public static T Random
        {
            get
            {
                if (Empty) return default(T);

                var res = MongoCollection.Find(Builders<T>.Filter.Empty);

                var rnd = new Random(Guid.NewGuid().GetHashCode());

                return res.Skip(rnd.Next((int)res.Count() - 1)).FirstOrDefault();
            }
        }

        public static IEnumerable<T> TakeRandom(int v)
        {
            for (int i = 0; i < v; i++) yield return Random;
        }

        public static IEnumerable<T> All => MongoCollection.Find(Builders<T>.Filter.Empty).ToEnumerable();

        public static bool Empty => Count() == 0;

        public static IQueryable<T> Queryable() => MongoCollection.AsQueryable();

        public static IEnumerable<K> Select<K>(Func<T, K> sel)
        {
            var filter = Builders<T>.Filter.Empty;
            return MongoCollection.Find(filter).ToEnumerable().Select(sel);
        }

        public static IEnumerable<T> SortBy(Expression<Func<T, object>> sel)
        {
            var filter = Builders<T>.Filter.Empty;
            var res = MongoCollection.Find(filter).SortBy(sel);
            return res.ToEnumerable();
        }

        public static IEnumerable<T> SortByDescending(Expression<Func<T, object>> sel)
        {
            var filter = Builders<T>.Filter.Empty;
            var res = MongoCollection.Find(filter).SortByDescending(sel);
            return res.ToEnumerable();
        }

        public static bool Any(Expression<Func<T, bool>> p) => MongoCollection.Find(p).Any();

        public static async Task<bool> AnyAsync(Expression<Func<T, bool>> p)
        {
            var res = await MongoCollection.FindAsync(p);
            return await res.AnyAsync();
        }

        public static long Count() => MongoCollection.Count(Builders<T>.Filter.Empty);

        public static long Count(Expression<Func<T, bool>> sel) => MongoCollection.Count(sel);

        public static Task<long> CountAsync(Expression<Func<T, bool>> sel) => MongoCollection.CountAsync(sel);

        public static Task<long> CountAsync() => MongoCollection.CountAsync(Builders<T>.Filter.Empty);

        public static void Add(T obj)
        {
            MongoCollection.InsertOneAsync(obj);
        }

        public static T Create(T obj)
        {
            MongoCollection.InsertOne(obj);
            return obj;
        }

        public static async Task<T> CreateAsync(T obj)
        {
            await MongoCollection.InsertOneAsync(obj);
            return obj;
        }

        public static void Replace(T obj)
        {
            var filter = Builders<T>.Filter.Eq(x => x.Id, obj.Id);
            MongoCollection.ReplaceOne(filter, obj);
        }


        public static async Task ReplaceAsync(T obj)
        {
            var filter = Builders<T>.Filter.Eq(x => x.Id, obj.Id);
            await MongoCollection.ReplaceOneAsync(filter, obj);
        }


        public static T AddToAsync<K>(T obj, Expression<Func<T, IEnumerable<K>>> sel, K value)
        {
            var filter = Builders<T>.Filter.Eq(x => x.Id, obj.Id);
            var update = Builders<T>.Update.AddToSet(sel, value);
            MongoCollection.UpdateOneAsync(filter, update);
            return obj;
        }

        public static async Task<T> AddToAsync<K>(T obj, Expression<Func<T, IEnumerable<K>>> sel, IEnumerable<K> value)
        {
            var filter = Builders<T>.Filter.Eq(x => x.Id, obj.Id);
            var update = Builders<T>.Update.AddToSetEach(sel, value);
            await MongoCollection.UpdateOneAsync(filter, update);
            return obj;
        }

        public static T AddTo<K>(T obj, Expression<Func<T, IEnumerable<K>>> sel, K value)
        {
            var filter = Builders<T>.Filter.Eq(x => x.Id, obj.Id);
            var update = Builders<T>.Update.AddToSet(sel, value);
            MongoCollection.UpdateOne(filter, update);
            return obj;
        }

        public static async Task UpdateAsync(T obj)
        {
            var filter = Builders<T>.Filter.Eq(x => x.Id, obj.Id);
            await MongoCollection.ReplaceOneAsync(filter, obj);
        }

        public static async Task UpdateAsync<K>(T obj, Expression<Func<T, K>> sel, K value)
        {
            var filter = Builders<T>.Filter.Eq(x => x.Id, obj.Id);
            var update = Builders<T>.Update.Set(sel, value);
            await MongoCollection.UpdateOneAsync(filter, update);
        }

        public static async Task AppendAsync<K>(T obj, Expression<Func<T, IEnumerable<K>>> sel, K value)
        {
            var filter = Builders<T>.Filter.Eq(x => x.Id, obj.Id);
            var update = Builders<T>.Update.AddToSet(sel, value);
            await MongoCollection.UpdateOneAsync(filter, update);
        }

        public static async Task IncreaseAsync<K>(T obj, Expression<Func<T, K>> sel, K value)
        {
            var filter = Builders<T>.Filter.Eq(x => x.Id, obj.Id);
            var update = Builders<T>.Update.Inc(sel, value);
            await MongoCollection.UpdateOneAsync(filter, update);
        }

        public static void Delete(T obj)
        {
            MongoCollection.DeleteOne(Builders<T>.Filter.Eq(x => x.Id, obj.Id));
        }

        public static void Delete(IEnumerable<T> obj)
        {
            MongoCollection.DeleteMany(Builders<T>.Filter.In(x => x.Id, obj.Select(x=>x.Id)));
        }

        public static async Task DeleteAsync(Expression<Func<T, bool>> p)
        {
            await MongoCollection.DeleteManyAsync(p);
        }

        public static async Task DeleteAsync(T obj)
        {
            await MongoCollection.DeleteOneAsync(Builders<T>.Filter.Eq(x => x.Id, obj.Id));
        }

        public static async Task DeleteAsync(IEnumerable<T> obj)
        {
            await MongoCollection.DeleteManyAsync(Builders<T>.Filter.In(x => x.Id, obj.Select(x => x.Id)));
        }

        public static T GetOrCreate(Expression<Func<T, bool>> p, T obj)
        {
            var f = FirstOrDefault(p);

            return f == default(T) ?  Create(obj) : f;
        }

        public static T FirstOrDefault() => MongoCollection.Find(Builders<T>.Filter.Empty).FirstOrDefault();

        public static T FirstOrDefault(Expression<Func<T, bool>> p) => MongoCollection.Find(p).FirstOrDefault();

        public static async Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> p) => await (await MongoCollection.FindAsync(p)).FirstOrDefaultAsync();

        public static T First(Expression<Func<T, bool>> p) => MongoCollection.Find(p).FirstOrDefault();

        public static async Task<IEnumerable<T>> WhereAsync(Expression<Func<T, bool>> p)
        {
            return (await MongoCollection.FindAsync(p)).ToEnumerable();
        }

        public static IEnumerable<T> Search(string search) => MongoCollection.Find(Builders<T>.Filter.Text(search)).ToEnumerable();

        public static int Sum(Func<T,int> sum) => MongoCollection.Find(Builders<T>.Filter.Empty).ToEnumerable().Sum(sum);

        public static double Sum(Func<T, double> sum) => MongoCollection.Find(Builders<T>.Filter.Empty).ToEnumerable().Sum(sum);

        public static decimal Sum(Func<T, decimal> sum) => MongoCollection.Find(Builders<T>.Filter.Empty).ToEnumerable().Sum(sum);

        public static float Sum(Func<T, float> sum) => MongoCollection.Find(Builders<T>.Filter.Empty).ToEnumerable().Sum(sum);

        #endregion
    }
}
