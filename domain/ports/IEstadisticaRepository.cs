using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using campusLove.domain.entities;

namespace campusLove.domain.ports
{
    public interface IEstadisticaRepository
    {
         Task<Estadistica> GetByIdAsync(int id);
        Task<IEnumerable<Estadistica>> GetAllAsync();
        Task AddAsync(Estadistica estadistica);
        Task UpdateAsync(Estadistica estadistica);
        Task DeleteAsync(int id);
    
    }
}