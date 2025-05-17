using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using campusLove.domain.entities;

namespace campusLove.domain.ports
{
    public interface ILoginRepository
    {
        Task<Login> GetByIdAsync(int id);
        Task<IEnumerable<Login>> GetAllAsync();
        Task AddAsync(Login login);
        Task UpdateAsync(Login login);
        Task DeleteAsync(int id);
    
    }
}