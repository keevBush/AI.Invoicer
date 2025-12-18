using AI.Invoicer.Domain.Model.Structure;
using AI.Invoicer.Infrastructure.Interface;
using System.Linq.Expressions;

namespace AI.Invoicer.Infrastructure.Persistance
{
    public class InvoiceRepository : IRepository<Invoice>
    {
        public Task AddAsync(params Invoice[] entity)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(string id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Invoice>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Invoice>> GetAsync(Expression<Func<Invoice, bool>> where)
        {
            throw new NotImplementedException();
        }

        public Task<Invoice?> GetByIdAsync(string id)
        {
            throw new NotImplementedException();
        }

        public Task UpdateAsync(Invoice entity)
        {
            throw new NotImplementedException();
        }
    }
}
