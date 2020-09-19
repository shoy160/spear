using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Spear.Core.Domain.Repositories
{
    public abstract partial class BaseRepository<TEntity, TKey>
    {
        public virtual async Task<TKey> InsertAsync(TEntity entity)
        {
            return await Task.FromResult(Insert(entity));
        }

        public virtual async Task<int> InsertAsync(IEnumerable<TEntity> entities)
        {
            return await Task.FromResult(Insert(entities));
        }

        public virtual async Task<int> DeleteAsync(TEntity entity)
        {
            return await Task.FromResult(Delete(entity));
        }

        public virtual async Task<int> DeleteAsync(TKey key)
        {
            return await Task.FromResult(Delete(key));
        }

        public virtual async Task<int> DeleteAsync(Expression<Func<TEntity, bool>> expression)
        {
            return await Task.FromResult(Delete(expression));
        }

        public virtual async Task<int> UpdateAsync(TEntity entity)
        {
            return await Task.FromResult(Update(entity));
        }

        public virtual async Task<int> UpdateAsync(TEntity entity, params string[] parms)
        {
            return await Task.FromResult(Update(entity, parms));
        }

        public virtual async Task<int> UpdateAsync(TEntity entity, Expression<Func<TEntity, bool>> expression, params string[] parms)
        {
            return await Task.FromResult(Update(entity, expression, parms));
        }

        public virtual async Task<int> UpdateAsync(TEntity entity, IQueryable<TEntity> entities, params string[] parms)
        {
            return await Task.FromResult(Update(entity, entities, parms));
        }

        public virtual async Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> expression)
        {
            return await Task.FromResult(Exists(expression));
        }

        public virtual async Task<TEntity> LoadAsync(TKey key)
        {
            return await Task.FromResult(Load(key));
        }

        public virtual async Task<IQueryable<TEntity>> ListAsync(IEnumerable<TKey> keys)
        {
            return await Task.FromResult(List(keys));
        }

        public virtual async Task<TEntity> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> expression)
        {
            return await Task.FromResult(FirstOrDefault(expression));
        }

        public virtual async Task<TEntity> SingleOrDefaultAsync(Expression<Func<TEntity, bool>> expression)
        {
            return await Task.FromResult(SingleOrDefault(expression));
        }

        public virtual async Task<IQueryable<TEntity>> WhereAsync(Expression<Func<TEntity, bool>> expression)
        {
            return await Task.FromResult(Where(expression));
        }

        public virtual async Task<DResults<TEntity>> PageListAsync(IOrderedQueryable<TEntity> ordered, DPage page)
        {
            return await Task.FromResult(PageList(ordered, page));
        }

        public virtual async Task<int> CountAsync()
        {
            return await Task.FromResult(Count());
        }

        public virtual async Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await Task.FromResult(Count(predicate));
        }

        public virtual async Task<long> LongCountAsync()
        {
            return await Task.FromResult(LongCount());
        }

        public virtual async Task<long> LongCountAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await Task.FromResult(LongCount(predicate));
        }
    }
}
