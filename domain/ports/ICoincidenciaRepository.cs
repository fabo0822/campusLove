using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using campusLove.domain.entities;

namespace campusLove.domain.ports
{
    public interface ICoincidenciaRepository
    {
         Task<Coincidencia> GetByIdAsync(int id);
        Task<IEnumerable<Coincidencia>> GetAllAsync();
        Task AddAsync(Coincidencia coincidencia);
        Task UpdateAsync(Coincidencia coincidencia);
        Task DeleteAsync(int id);
    }
}