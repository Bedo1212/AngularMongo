using DAL.Repositories.Interfaces;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Repositories
{
    public class MongoRepository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        private readonly IMongoDatabase _database;
        private readonly IMongoCollection<TEntity> _collection;
        private string _colllectionName;

        public MongoRepository(IMongoDatabase database, string collectionName)
        {
            _database = database;
            _collection = _database.GetCollection<TEntity>(collectionName);
            _colllectionName = collectionName;
        }

        public virtual void Add(TEntity entity)
        {
            _collection.InsertOne(entity);
        }

        public virtual void AddRange(IEnumerable<TEntity> entities)
        {
            _collection.InsertMany(entities);
        }

        public virtual void Update(TEntity entity)
        {
            // Update logic for MongoDB (you may need to implement this)
            // Example: _collection.ReplaceOne(Builders<TEntity>.Filter.Eq("_id", entity.Id), entity);
        }

        public virtual void UpdateRange(IEnumerable<TEntity> entities)
        {
            // Implement update logic for multiple entities (if needed)
            foreach (var entity in entities)
            {
                Update(entity);
            }
        }

        public virtual void Remove(TEntity entity)
        {
            // Remove logic for MongoDB (you may need to implement this)
            // Example: _collection.DeleteOne(Builders<TEntity>.Filter.Eq("_id", entity.Id));
        }

        public virtual void RemoveRange(IEnumerable<TEntity> entities)
        {
            // Implement remove logic for multiple entities (if needed)
            foreach (var entity in entities)
            {
                Remove(entity);
            }
        }

        public virtual int Count()
        {
            return int.Parse( _collection.EstimatedDocumentCount().ToString());
        }

        public virtual IEnumerable<TEntity> Find(Expression<Func<TEntity, bool>> predicate)
        {
            return _collection.Find(predicate).ToEnumerable();
        }

        public virtual TEntity GetSingleOrDefault(Expression<Func<TEntity, bool>> predicate)
        {
            return _collection.Find(predicate).SingleOrDefault();
        }

        public virtual TEntity Get(int id)
        {
            // You may need to adapt this if your MongoDB documents have a different identifier
            return _collection.Find(Builders<TEntity>.Filter.Eq("_id", id.ToString())).SingleOrDefault();
        }

        public virtual IEnumerable<TEntity> GetAll()
        {
            return _collection.Find(_ => true).ToEnumerable();
        }

       
    }
}
