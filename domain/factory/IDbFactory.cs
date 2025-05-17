using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using campusLove.domain.ports;

namespace campusLove.domain.factory
{
    public interface IDbFactory
    {
        ICiudadRepository CrearCiudadRepository();
        IUsuarioRepository CrearUsuarioRepository();
        ILoginRepository CrearLoginRepository();
        IDepartamentoRepository CrearDepartamentoRepository();
        
    }
}