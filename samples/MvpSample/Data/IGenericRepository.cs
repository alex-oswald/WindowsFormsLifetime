using System.Linq.Expressions;

namespace MvpSample.Data
{
    public interface IEntity
    {
        Guid Id { get; set; }
    }

    // Source: https://github.com/oakcool/OakIdeas.GenericRepository/blob/b85acff9dcd8043a1b2f6ba1353cc2b8cbea92fa/src/OakIdeas.GenericRepository/IGenericRepository.cs
    public interface IRepository<TEntity>
        where TEntity : IEntity
    {
        /// <summary>
        /// Deletes an entity.
        /// </summary>
        /// <param name="entityToDelete">The entity to delete.</param>
        /// <returns>True if the deletion succeeded.</returns>
        Task<bool> DeleteAsync(TEntity entityToDelete);

        /// <summary>
        /// Delete an entity by its <see cref="id"/>.
        /// </summary>
        /// <param name="id">The id of the entity to delete.</param>
        /// <returns>True if the deletion succeeded.</returns>
        Task<bool> DeleteAsync(Guid id);

        Task<IList<TEntity>> GetAllAsync(
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            string includeProperties = "");

        /// <summary>
        /// Gets an entity by its <see cref="id"/>.
        /// </summary>
        /// <param name="id">The id of the entity to get.</param>
        /// <returns>The entity with the specified <see cref="id"/>.</returns>
        Task<TEntity> GetAsync(Guid id);

        /// <summary>
        /// Inserts a new entity.
        /// </summary>
        /// <param name="entity">The entity to insert.</param>
        /// <returns>The inserted entity.</returns>
        Task<TEntity> InsertAsync(TEntity entity);

        /// <summary>
        /// Updates an entity.
        /// </summary>
        /// <param name="entityToUpdate">The entity to update.</param>
        /// <returns>The updated entity.</returns>
        Task<TEntity> UpdateAsync(TEntity entityToUpdate);
    }
}