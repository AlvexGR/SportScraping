using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using MySql.Data.MySqlClient;
using TQI.Infrastructure.Entity.Database.Helpers;
using TQI.Infrastructure.Entity.Models;

namespace TQI.Infrastructure.Entity.Database.BaseRepository
{
    public abstract class BaseRepository<TEntity> : IBaseRepository<TEntity> where TEntity : BaseModel
    {
        private DbConnectionString _dbConnectionString;

        protected BaseRepository(DbConnectionString dbConnectionString)
        {
            _dbConnectionString = dbConnectionString;
        }

        protected IDbConnection GetConnection()
        {
            return new MySqlConnection(_dbConnectionString.ConnectionString);
        }

        public void Dispose()
        {
            _dbConnectionString = null;
        }

        public async Task<IEnumerable<TEntity>> QueryAsync(string sql, object param = null, int? timeoutSeconds = null)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                var result = await connection.QueryAsync<TEntity>(sql, param, commandTimeout: timeoutSeconds);
                connection.Close();
                return result;
            }
        }

        public async Task<int> InsertAsync(IEnumerable<TEntity> entities, int? timeoutSeconds = null)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                var tableName = DbConverter<TEntity>.ToTableName();
                var columnNames = DbConverter<TEntity>.ToColumnNames();
                var parameterNames = DbConverter<TEntity>.ToParameterNames();
                var sql = QueryGenerator.GenerateInsertQuery(tableName, columnNames, parameterNames);
                int insertResult;
                using (var transaction = connection.BeginTransaction())
                {
                    insertResult = await connection.ExecuteAsync(sql, entities, commandTimeout: timeoutSeconds);
                    transaction.Commit();
                }
                connection.Close();
                return insertResult;
            }
        }

        public async Task<TEntity> InsertThenGet(TEntity entity, int? timeoutSeconds = null)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                var tableName = DbConverter<TEntity>.ToTableName();
                var columnNames = DbConverter<TEntity>.ToColumnNames();
                var parameterNames = DbConverter<TEntity>.ToParameterNames();
                var sql = QueryGenerator.GenerateInsertQuery(tableName, columnNames, parameterNames);
                using (var transaction = connection.BeginTransaction())
                {
                    var insertResult = await connection.ExecuteAsync(sql, entity, commandTimeout: timeoutSeconds);
                    if (insertResult == 1)
                    {
                        const string getLastIdQuery = "SELECT LAST_INSERT_ID();";
                        entity.Id = (await connection.QueryAsync<int>(getLastIdQuery, null, commandTimeout: timeoutSeconds)).First();
                    }
                    else
                    {
                        throw new Exception($"Inserted result must equal to 1: {insertResult}");
                    }
                    transaction.Commit();
                }
                connection.Close();
                return entity;
            }
        }

        public async Task<int> UpdateAsync(IEnumerable<TEntity> entities, string condition = "Id = @Id", int? timeoutSeconds = null)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                var tableName = DbConverter<TEntity>.ToTableName();
                var columnNames = DbConverter<TEntity>.ToColumnNames();
                var parameterNames = DbConverter<TEntity>.ToParameterNames();
                var sql = QueryGenerator.GenerateUpdateQuery(tableName, columnNames, parameterNames, condition);
                int updateResult;
                using (var transaction = connection.BeginTransaction())
                {
                    updateResult = await connection.ExecuteAsync(sql, entities, commandTimeout: timeoutSeconds);
                    transaction.Commit();
                }
                connection.Close();
                return updateResult;
            }
        }

        public async Task<int> DeleteAsync(IEnumerable<TEntity> entities, string condition = "Id = @Id", int? timeoutSeconds = null)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                var tableName = DbConverter<TEntity>.ToTableName();
                var sql = QueryGenerator.GenerateDeleteQuery(tableName, condition);
                int deleteResult;
                using (var transaction = connection.BeginTransaction())
                {
                    deleteResult = await connection.ExecuteAsync(sql, entities, commandTimeout: timeoutSeconds);
                    transaction.Commit();
                }
                connection.Close();
                return deleteResult;
            }
        }
    }
}
