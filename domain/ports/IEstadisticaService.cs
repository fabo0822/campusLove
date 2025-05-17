using System.Collections.Generic;
using System.Threading.Tasks;
using campusLove.domain.models;

namespace campusLove.domain.ports
{
    /// <summary>
    /// Puerto primario para el servicio de estadísticas.
    /// Define las operaciones que la aplicación puede realizar con las estadísticas.
    /// </summary>
    public interface IEstadisticaService
    {
        /// <summary>
        /// Obtiene la lista de usuarios ordenada por cantidad de likes recibidos
        /// </summary>
        Task<IEnumerable<EstadisticaUsuario>> ObtenerUsuariosConMasLikes();

        /// <summary>
        /// Obtiene estadísticas generales del sistema
        /// </summary>
        Task<Dictionary<string, int>> ObtenerEstadisticasGenerales();

        /// <summary>
        /// Muestra las estadísticas en la consola
        /// </summary>
        Task MostrarEstadisticas();

        /// <summary>
        /// Actualiza las estadísticas para un usuario específico
        /// </summary>
        /// <param name="usuarioId">ID del usuario a actualizar</param>
        Task ActualizarEstadisticas(int usuarioId);
    }
} 