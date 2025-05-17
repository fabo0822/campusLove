using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using campusLove.domain.ports;
using campusLove.domain.entities;

namespace campusLove.infraestructure.repositories
{
    public class LoginRepository : IGenericRepository<Login>
    {
        public List<Login> ObtenerTodos()
        {
            throw new NotImplementedException();
        }

        public void Crear(Login entity)
        {
            throw new NotImplementedException();
        }

        public void Actualizar(Login entity)
        {
            throw new NotImplementedException();
        }

        public void Eliminar(int id)
        {
            throw new NotImplementedException();
        }

        public Task<Usuario> ValidateCredentialsAsync(string email, string password)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ChangePasswordAsync(int usuarioId, string oldPassword, string newPassword)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ResetPasswordAsync(string email)
        {
            throw new NotImplementedException();
        }
    }
} 