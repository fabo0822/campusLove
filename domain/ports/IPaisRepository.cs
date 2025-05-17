using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using campusLove.domain.entities;

namespace campusLove.domain.ports
{
    public interface IPaisRepository
    {
        Task<Pais> GetByIdAsync(int id);
        Task<IEnumerable<Pais>> GetAllAsync();
        Task AddAsync(Pais pais);
        Task UpdateAsync(Pais pais);
        Task DeleteAsync(int id);
    }
}