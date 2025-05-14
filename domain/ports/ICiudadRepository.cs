using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using campusLove.domain.entities;

namespace campusLove.domain.ports
{
    public interface ICiudadRepository
    {
        Task<Ciudad> GetByIdAsync(int id);
        Task<IEnumerable<Ciudad>> GetAllAsync();
        Task AddAsync(Ciudad ciudad);
        Task UpdateAsync(Ciudad ciudad);
        Task DeleteAsync(int id);
    }
}
    
