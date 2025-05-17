using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using campusLove.domain.ports;
using campusLove.domain.entities;

namespace campusLove.infraestructure.repositories
{
    public class CiudadRepository : IGenericRepository<Ciudad>
    {
        public List<Ciudad> ObtenerTodos()
        {
            throw new NotImplementedException();
        }

        public void Crear(Ciudad entity)
        {
            throw new NotImplementedException();
        }

        public void Actualizar(Ciudad entity)
        {
            throw new NotImplementedException();
        }

        public void Eliminar(int id)
        {
            throw new NotImplementedException();
        }
    }
}