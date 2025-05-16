using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using campusLove.domain.ports;
using campusLove.domain.entities;

namespace campusLove.infraestructure.repositories
{
    public class UsuarioRepository : IGenericRepository<Usuario>
    {
        public List<Usuario> ObtenerTodos()
        {
            throw new NotImplementedException();
        }

        public void Crear(Usuario entity)
        {
            throw new NotImplementedException();
        }

        public void Actualizar(Usuario entity)
        {
            throw new NotImplementedException();
        }

        public void Eliminar(int id)
        {
            throw new NotImplementedException();
        }
    }
} 