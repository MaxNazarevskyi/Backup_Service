﻿using Backup_Service.Data.DataModels;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Backup_Service.Data.Repository
{
    public interface IRepository<TEntity> where TEntity : class, IEntity
    {
        public Task AddAsync(TEntity entity);
        public Task DeleteAsync(int id);
        public Task Update(TEntity newData);
        public Task<TEntity?> GetByIdAsync(int id);
        public Task<List<TEntity>> GetAllAsync();
        public Task<List<TEntity>> SearchAsync(Expression<Func<TEntity, bool>> predicate);
        public Task<TEntity?> SingleOrDefaultAsync(Expression<Func<TEntity, bool>> predicate);
        public TEntity? GetById(int id);
        public List<TEntity> GetAll();
    }
}
