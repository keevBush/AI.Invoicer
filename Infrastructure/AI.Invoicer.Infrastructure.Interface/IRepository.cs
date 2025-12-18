using AI.Invoicer.Domain.Model.Structure;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace AI.Invoicer.Infrastructure.Interface
{
    public interface IRepository<T> where T : BaseEntity
    {
        Task<T?> GetByIdAsync(string id);
        Task<IEnumerable<T>> GetAllAsync();
        Task<IEnumerable<T>> GetAsync(Expression<Func<T, bool>> where);
        Task AddAsync(params T[] entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(string id);
    }
}
