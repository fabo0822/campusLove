using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using campusLove.domain.entities;

namespace campusLove.domain.ports
{
    public interface IDepartamentoRepository
    {
        Task<Departamento> GetByIdAsync(int id);
        Task<IEnumerable<Departamento>> GetAllAsync();
        Task AddAsync(Departamento departamento);
        Task UpdateAsync(Departamento departamento);
        Task DeleteAsync(int id);
    
    }
}