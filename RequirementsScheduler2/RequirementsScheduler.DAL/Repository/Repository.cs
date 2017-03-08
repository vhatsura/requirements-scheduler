using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using LinqToDB;
using LinqToDB.Mapping;

namespace RequirementsScheduler.DAL.Repository
{
    public abstract class Repository<TEntity, TKey> : IRepository<TEntity, TKey>
        where TEntity : class, IRepositoryModel<TKey>
    {
        private Database Db { get; }

        protected Repository(Database db)
        {
            Db = db;
        }

        public IEnumerable<TEntity> Get()
        {
            using (var db = Db.Open())
            {
                return db.GetTable<TEntity>().ToList();
            }
        }

        public IEnumerable<TEntity> Get(Expression<Func<TEntity, bool>> filter)
        {
            using (var db = Db.Open())
            {
                return db.GetTable<TEntity>()
                    .Where(filter)
                    .ToList();
            }
        }

        public TEntity Get(TKey id)
        {
            using (var db = Db.Open())
            {
                var pkName =
                    typeof(TEntity)
                        .GetProperties()
                        .First(prop => prop.GetCustomAttributes<PrimaryKeyAttribute>(false).Any());

                var expression = SimpleComparison(pkName.Name, id);

                return db.GetTable<TEntity>()
                    .Where(expression)
                    .FirstOrDefault();
            }
        }

        public TEntity Add(TEntity entity)
        {
            using (var db = Db.Open())
            {
                var key = db.InsertWithIdentity(entity);

                return Get((TKey) key);
            }
        }

        public bool Delete(TKey id)
        {
            using (var db = Db.Open())
            {
                var count = db.GetTable<TEntity>()
                    .Delete(entity => Equals(entity.Id, id));

                return count > 0;
            }
        }

        public TEntity Update(TKey id, TEntity entity)
        {
            using (var db = Db.Open())
            {
                db.GetTable<TEntity>()
                    .Update(e => Equals(e.Id, id), e => entity);

                return Get(id);
            }
        }

        public Func<TEntity, bool> SimpleComparison(string property, TKey value)
        {
            var type = typeof(TEntity);
            var pe = Expression.Parameter(type, "p");
            var propertyReference = Expression.Property(pe, property);
            var constantReference = Expression.Constant(value);

            return Expression.Lambda<Func<TEntity, bool>>
                (Expression.Equal(propertyReference, constantReference), pe).Compile();
        }
    }
}
