using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using campusLove.domain.entities;

namespace campusLove.domain.ports
{
    public interface IUsuarioRepository
    {
        Task<Usuario> GetByIdAsync(int id);
        Task<IEnumerable<Usuario>> GetAllAsync();
        Task AddAsync(Usuario usuario);
        Task UpdateAsync(Usuario usuario);
        Task DeleteAsync(int id);
    }
}