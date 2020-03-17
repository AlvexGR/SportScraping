using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TQI.Infrastructure.Entity.Models;

namespace TQI.Infrastructure.Entity.Database.BaseRepository
{
    public interface IBaseRepository<TEntity> : IDisposable where TEntity : BaseModel
    {
        /// <summary>
        /// Query by a sql statement in async
        /// </summary>
        /// <param name="sql">Sql statement</param>
        /// <param name="param">Parameter</param>
        /// <param name="timeoutSeconds">Timeout in seconds</param>
        /// <returns>List of elements</returns>
        Task<IEnumerable<TEntity>> QueryAsync(string sql, object param = null, int? timeoutSeconds = null);

        /// <summary>
        /// Insert list of entities
        /// </summary>
        /// <param name="entities">Entities to insert</param>
        /// <param name="timeoutSeconds">Timeout in seconds</param>
        /// <returns>Number to total inserted records</returns>
        Task<int> InsertAsync(IEnumerable<TEntity> entities, int? timeoutSeconds = null);

        /// <summary>
        /// Insert entity to have key generated then get the id back and return with entity
        /// </summary>
        /// <param name="entity">Entity to insert</param>
        /// <param name="timeoutSeconds">Timeout in seconds</param>
        /// <returns>The inserted entity with id</returns>
        Task<TEntity> InsertThenGet(TEntity entity, int? timeoutSeconds = null);

        /// <summary>
        /// Update list of entities
        /// </summary>
        /// <param name="entities">Entities to update</param>
        /// <param name="condition">Update condition separate with comma. Ex: Id = @Id, Name = @Name,...</param>
        /// <param name="timeoutSeconds">Timeout in seconds</param>
        /// <returns>Number to total updated records</returns>
        Task<int> UpdateAsync(IEnumerable<TEntity> entities, string condition = "Id = @Id", int? timeoutSeconds = null);

        /// <summary>
        /// Delete list of entities
        /// </summary>
        /// <param name="entities">Entities to delete</param>
        /// <param name="condition">Update condition separate with comma. Ex: Id = @Id, Name = @Name,...</param>
        /// <param name="timeoutSeconds">Timeout in seconds</param>
        /// <returns>Number to total deleted records</returns>
        Task<int> DeleteAsync(IEnumerable<TEntity> entities, string condition = "Id = @Id", int? timeoutSeconds = null);
    }
}
