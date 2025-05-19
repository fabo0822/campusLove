using System.Collections.Generic;
using System.Threading.Tasks;
using campusLove.application.services;
using campusLove.domain.models;

namespace campusLove.domain.strategy
{
    /// <summary>
    /// Interface que define el contrato para todas las estrategias de emparejamiento.
    /// Implementa el patrón Strategy permitiendo intercambiar algoritmos de emparejamiento en tiempo de ejecución.
    /// </summary>
    /// <remarks>
    /// El patrón Strategy nos permite definir una familia de algoritmos, encapsular cada uno,
    /// y hacerlos intercambiables. Este patrón permite que el algoritmo de emparejamiento varíe
    /// independientemente de los clientes que lo utilizan.
    /// </remarks>
    public interface IEmparejamientoStrategy
    {
        /// <summary>
        /// Nombre descriptivo de la estrategia para mostrar en la interfaz de usuario.
        /// </summary>
        string Nombre { get; }
        
        /// <summary>
        /// Descripción detallada de la estrategia para mostrar en la interfaz de usuario.
        /// </summary>
        string Descripcion { get; }
        
        /// <summary>
        /// Método principal que implementa el algoritmo para encontrar perfiles compatibles.
        /// </summary>
        /// <param name="usuarioId">ID del usuario para el que se buscan perfiles compatibles</param>
        /// <param name="cantidadPerfiles">Número máximo de perfiles a devolver</param>
        /// <returns>Lista de perfiles de usuario compatibles</returns>
        Task<List<PerfilUsuario>> EncontrarPerfilesCompatibles(int usuarioId, int cantidadPerfiles);
    }
}
