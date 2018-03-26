using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using MongoDB.Driver;
using Mongolino.Attributes;

namespace Mongolino
{
    public class Collection<T> where T: class, ICollectionItem
    {
        public Collection(string connectionString, string collectionName)
        {
            var type = typeof(T);

            if (connectionString != null)
            {
                var url = new MongoUrl(connectionString);
                var client = new MongoClient(connectionString);
                MongoCollection = client.GetDatabase(url.DatabaseName ?? "default").GetCollection<T>(collectionName ?? type.Name.ToLowerInvariant());
            }
            else
            {
                MongoCollection = (new MongoClient()).GetDatabase("default").GetCollection<T>(collectionName ?? type.Name.ToLowerInvariant());
            }

            foreach (var prop in type.GetProperties())
            {
                if (prop.GetCustomAttribute(typeof(AscendingIndexAttribute)) != null)
                {
                    MongoCollection.Indexes.CreateOne(
                        Builders<T>.IndexKeys.Ascending(new StringFieldDefinition<T>(prop.Name)));
                }

                if (prop.GetCustomAttribute(typeof(DescendingIndexAttribute)) != null)
                {
                    MongoCollection.Indexes.CreateOne(
                        Builders<T>.IndexKeys.Descending(new StringFieldDefinition<T>(prop.Name)));
                }

                if (prop.GetCustomAttribute(typeof(FullTextIndexAttribute)) != null)
                {
                    MongoCollection.Indexes.CreateOne(Builders<T>.IndexKeys.Text(new StringFieldDefinition<T>(prop.Name)));
                }
            }
        }

        private IMongoCollection<T> MongoCollection { get; }

        public void FullTextindex(Expression<Func<T, object>> func)
        {
            MongoCollection.Indexes.CreateOne(Builders<T>.IndexKeys.Text(func));
        }

        public void AscendingIndex(Expression<Func<T, object>> func)
        {
            MongoCollection.Indexes.CreateOne(Builders<T>.IndexKeys.Ascending(func));
        }

        public void DescendingIndex(Expression<Func<T, object>> func)
        {
            MongoCollection.Indexes.CreateOne(Builders<T>.IndexKeys.Descending(func));
        }

        public IEnumerable<T> FullTextSearch(string text)
        {
            var filter = Builders<T>.Filter.Text(text);
            var res = MongoCollection.Find(filter);
            return res.ToEnumerable();
        }

        public async Task<IEnumerable<T>> FullTextSearchAsync(string text)
        {
            var filter = Builders<T>.Filter.Text(text);
            var res = await MongoCollection.FindAsync(filter);

            return res.ToEnumerable();
        }

        public T Random
        {
            get
            {
                if (Empty) return default(T);

                var res = MongoCollection.Find(Builders<T>.Filter.Empty);

                var rnd = new Random(Guid.NewGuid().GetHashCode());

                return res.Skip(rnd.Next((int)res.Count() - 1)).FirstOrDefault();
            }
        }

        public IEnumerable<T> TakeRandom(int v)
        {
            for (int i = 0; i < v; i++) yield return Random;
        }

        public IEnumerable<T> All => MongoCollection.Find(Builders<T>.Filter.Empty).ToEnumerable();

        public bool Empty => Count() == 0;

        public IQueryable<T> Queryable() => MongoCollection.AsQueryable();

        public IEnumerable<K> Select<K>(Func<T, K> sel)
        {
            var filter = Builders<T>.Filter.Empty;
            return MongoCollection.Find(filter).ToEnumerable().Select(sel);
        }

        public IEnumerable<T> SortBy(Expression<Func<T, object>> sel)
        {
            var filter = Builders<T>.Filter.Empty;
            var res = MongoCollection.Find(filter).SortBy(sel);
            return res.ToEnumerable();
        }

        public IEnumerable<T> SortByDescending(Expression<Func<T, object>> sel)
        {
            var filter = Builders<T>.Filter.Empty;
            var res = MongoCollection.Find(filter).SortByDescending(sel);
            return res.ToEnumerable();
        }

        public bool Any(Expression<Func<T, bool>> p) => MongoCollection.Find(p).Any();

        public async Task<bool> AnyAsync(Expression<Func<T, bool>> p)
        {
            var res = await MongoCollection.FindAsync(p);
            return await res.AnyAsync();
        }

        public long Count() => MongoCollection.Count(Builders<T>.Filter.Empty);

        public long Count(Expression<Func<T, bool>> sel) => MongoCollection.Count(sel);

        public Task<long> CountAsync(Expression<Func<T, bool>> sel) => MongoCollection.CountAsync(sel);

        public Task<long> CountAsync() => MongoCollection.CountAsync(Builders<T>.Filter.Empty);

        public void Add(T obj)
        {
            MongoCollection.InsertOneAsync(obj);
        }

        public T Create(T obj)
        {
            MongoCollection.InsertOne(obj);
            return obj;
        }

        public async Task<T> CreateAsync(T obj)
        {
            await MongoCollection.InsertOneAsync(obj);
            return obj;
        }

        public void Replace(T obj)
        {
            var filter = Builders<T>.Filter.Eq(x => x.Id, obj.Id);
            MongoCollection.ReplaceOne(filter, obj);
        }


        public async Task ReplaceAsync(T obj)
        {
            var filter = Builders<T>.Filter.Eq(x => x.Id, obj.Id);
            await MongoCollection.ReplaceOneAsync(filter, obj);
        }


        public T AddToAsync<K>(T obj, Expression<Func<T, IEnumerable<K>>> sel, K value)
        {
            var filter = Builders<T>.Filter.Eq(x => x.Id, obj.Id);
            var update = Builders<T>.Update.AddToSet(sel, value);
            MongoCollection.UpdateOneAsync(filter, update);
            return obj;
        }

        public async Task<T> AddToAsync<K>(T obj, Expression<Func<T, IEnumerable<K>>> sel, IEnumerable<K> value)
        {
            var filter = Builders<T>.Filter.Eq(x => x.Id, obj.Id);
            var update = Builders<T>.Update.AddToSetEach(sel, value);
            await MongoCollection.UpdateOneAsync(filter, update);
            return obj;
        }

        public T AddTo<K>(T obj, Expression<Func<T, IEnumerable<K>>> sel, K value)
        {
            var filter = Builders<T>.Filter.Eq(x => x.Id, obj.Id);
            var update = Builders<T>.Update.AddToSet(sel, value);
            MongoCollection.UpdateOne(filter, update);
            return obj;
        }

        public async Task UpdateAsync(T obj)
        {
            var filter = Builders<T>.Filter.Eq(x => x.Id, obj.Id);
            await MongoCollection.ReplaceOneAsync(filter, obj);
        }

        public void Update<K>(T obj, Expression<Func<T, K>> sel, K value)
        {
            var filter = Builders<T>.Filter.Eq(x => x.Id, obj.Id);
            var update = Builders<T>.Update.Set(sel, value);
            MongoCollection.UpdateOne(filter, update);
        }

        public async Task UpdateAsync<K>(T obj, Expression<Func<T, K>> sel, K value)
        {
            var filter = Builders<T>.Filter.Eq(x => x.Id, obj.Id);
            var update = Builders<T>.Update.Set(sel, value);
            await MongoCollection.UpdateOneAsync(filter, update);
        }

        public async Task AppendAsync<K>(T obj, Expression<Func<T, IEnumerable<K>>> sel, K value)
        {
            var filter = Builders<T>.Filter.Eq(x => x.Id, obj.Id);
            var update = Builders<T>.Update.AddToSet(sel, value);
            await MongoCollection.UpdateOneAsync(filter, update);
        }

        public void Increase<K>(T obj, Expression<Func<T, K>> sel, K value)
        {
            var filter = Builders<T>.Filter.Eq(x => x.Id, obj.Id);
            var update = Builders<T>.Update.Inc(sel, value);
            MongoCollection.UpdateOne(filter, update);
        }

        public async Task IncreaseAsync<K>(T obj, Expression<Func<T, K>> sel, K value)
        {
            var filter = Builders<T>.Filter.Eq(x => x.Id, obj.Id);
            var update = Builders<T>.Update.Inc(sel, value);
            await MongoCollection.UpdateOneAsync(filter, update);
        }

        public void Delete(T obj)
        {
            MongoCollection.DeleteOne(Builders<T>.Filter.Eq(x => x.Id, obj.Id));
        }

        public void Delete(IEnumerable<T> obj)
        {
            MongoCollection.DeleteMany(Builders<T>.Filter.In(x => x.Id, obj.Select(x => x.Id)));
        }

        public async Task DeleteAsync(Expression<Func<T, bool>> p)
        {
            await MongoCollection.DeleteManyAsync(p);
        }

        public async Task DeleteAsync(T obj)
        {
            await MongoCollection.DeleteOneAsync(Builders<T>.Filter.Eq(x => x.Id, obj.Id));
        }

        public async Task DeleteAsync(IEnumerable<T> obj)
        {
            await MongoCollection.DeleteManyAsync(Builders<T>.Filter.In(x => x.Id, obj.Select(x => x.Id)));
        }

        public T GetOrCreate(Expression<Func<T, bool>> p, T obj) => FirstOrDefault(p) ?? Create(obj);

        public async Task<T> GetOrCreateAsync(Expression<Func<T, bool>> p, T obj)
        {
            return (await FirstOrDefaultAsync(p)) ?? await CreateAsync(obj);
        }

        public T FirstOrDefault() => MongoCollection.Find(Builders<T>.Filter.Empty).FirstOrDefault();

        public T FirstOrDefault(Expression<Func<T, bool>> p) => MongoCollection.Find(p).FirstOrDefault();

        public async Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> p) => await (await MongoCollection.FindAsync(p)).FirstOrDefaultAsync();

        public T First(Expression<Func<T, bool>> p) => MongoCollection.Find(p).FirstOrDefault();

        public IEnumerable<T> Where(Expression<Func<T, bool>> p)
        {
            return (MongoCollection.Find(p)).ToEnumerable();
        }

        public async Task<IEnumerable<T>> WhereAsync(Expression<Func<T, bool>> p)
        {
            return (await MongoCollection.FindAsync(p)).ToEnumerable();
        }

        public IEnumerable<T> Search(string search) => MongoCollection.Find(Builders<T>.Filter.Text(search)).ToEnumerable();

        public int Sum(Func<T, int> sum) => MongoCollection.Find(Builders<T>.Filter.Empty).ToEnumerable().Sum(sum);

        public double Sum(Func<T, double> sum) => MongoCollection.Find(Builders<T>.Filter.Empty).ToEnumerable().Sum(sum);

        public decimal Sum(Func<T, decimal> sum) => MongoCollection.Find(Builders<T>.Filter.Empty).ToEnumerable().Sum(sum);

        public float Sum(Func<T, float> sum) => MongoCollection.Find(Builders<T>.Filter.Empty).ToEnumerable().Sum(sum);
    }
}
