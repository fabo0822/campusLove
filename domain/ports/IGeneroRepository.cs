using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using campusLove.domain.entities;

namespace campusLove.domain.ports
{
    public interface IGeneroRepository
    {
        Task<Genero> GetByIdAsync(int id);
        Task<IEnumerable<Genero>> GetAllAsync();
        Task AddAsync(Genero genero);
        Task UpdateAsync(Genero genero);
        Task DeleteAsync(int id);
    
    }
}