using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using campusLove.domain.entities;

namespace campusLove.domain.ports
{
    public interface IInteraccionRepository
    {
         Task<Interaccion> GetByIdAsync(int id);
        Task<IEnumerable<Interaccion>> GetAllAsync();
        Task AddAsync(Interaccion interaccion);
        Task UpdateAsync(Interaccion interaccion);
        Task DeleteAsync(int id);
    
    }
}