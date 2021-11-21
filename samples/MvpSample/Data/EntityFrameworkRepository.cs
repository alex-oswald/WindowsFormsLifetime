using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace MvpSample.Data
{
    public class EntityFrameworkRepository<TEntity, TDataContext> : IRepository<TEntity>
        where TEntity : class, IEntity
        where TDataContext : DbContext
    {
        protected readonly TDataContext _context;
        internal DbSet<TEntity> _dbSet;

        public EntityFrameworkRepository(TDataContext dataContext)
        {
            _context = dataContext;
            _dbSet = _context.Set<TEntity>();
        }

        public virtual async Task<bool> DeleteAsync(TEntity entityToDelete)
        {
            if (_context.Entry(entityToDelete).State == EntityState.Detached)
            {
                _dbSet.Attach(entityToDelete);
            }
            _dbSet.Remove(entityToDelete);
            return await _context.SaveChangesAsync() >= 1;
        }

        public virtual async Task<bool> DeleteAsync(Guid id)
        {
            TEntity entityToDelete = await _dbSet.FindAsync(id);
            return await DeleteAsync(entityToDelete);
        }

        public virtual async Task<IList<TEntity>> GetAllAsync(
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            string includeProperties = "")
        {
            try
            {
                // Get the dbSet from the Entity passed in                
                IQueryable<TEntity> query = _dbSet;

                // Apply the filter
                if (filter != null)
                {
                    query = query.Where(filter);
                }

                // Include the specified properties
                foreach (var includeProperty in includeProperties.Split
                    (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProperty);
                }

                // Sort
                if (orderBy != null)
                {
                    return orderBy(query).AsNoTracking().ToList();
                }
                else
                {
                    return query.AsNoTracking().ToList();
                }
            }
            catch (Exception ex)
            {
                // msg = ex.Message;
                return null;
            }
        }

        public virtual async Task<TEntity> GetAsync(Guid id)
        {
            return await _dbSet.FindAsync(id);
        }

        public virtual async Task<TEntity> InsertAsync(TEntity entity)
        {
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public virtual async Task<TEntity> UpdateAsync(TEntity entityToUpdate)
        {
            var dbSet = _context.Set<TEntity>();
            dbSet.Attach(entityToUpdate);
            _context.Entry(entityToUpdate).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return entityToUpdate;
        }
    }
}