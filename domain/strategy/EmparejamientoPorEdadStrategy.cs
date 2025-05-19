using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using campusLove.application.services;
using campusLove.infraestructure.mysql;
using campusLove.domain.models;

namespace campusLove.domain.strategy
{
    /// <summary>
    /// Implementación concreta de la estrategia de emparejamiento basada en compatibilidad de edad.
    /// Esta estrategia encuentra perfiles de usuarios con edad similar al usuario actual.
    /// </summary>
    /// <remarks>
    /// Algoritmo: Busca usuarios dentro de un rango de edad predefinido (por defecto ±5 años).
    /// Prioriza los perfiles con menor diferencia de edad.
    /// </remarks>
    public class EmparejamientoPorEdadStrategy : IEmparejamientoStrategy
    {
        /// <summary>
        /// Factory para crear conexiones a la base de datos MySQL
        /// </summary>
        private readonly MySqlDbFactory _dbFactory;
        
        /// <summary>
        /// Rango de edad para buscar perfiles (±3 años)
        /// </summary>
        private const int RANGO_EDAD = 3; // Rango de diferencia de edad preferido (± años)
        
        /// <summary>
        /// Nombre descriptivo de la estrategia para mostrar en la UI
        /// </summary>
        public string Nombre => "Compatibilidad por Edad";
        
        /// <summary>
        /// Descripción detallada de la estrategia para mostrar en la UI
        /// </summary>
        public string Descripcion => "Encuentra perfiles dentro de un rango de edad similar";
        
        /// <summary>
        /// Constructor que recibe la factory de conexiones a base de datos
        /// </summary>
        /// <param name="dbFactory">Factory para crear conexiones MySQL</param>
        public EmparejamientoPorEdadStrategy(MySqlDbFactory dbFactory)
        {
            _dbFactory = dbFactory;
        }
        
        /// <summary>
        /// Método que encuentra perfiles de usuarios compatibles según la estrategia de emparejamiento por edad.
        /// </summary>
        /// <param name="usuarioId">ID del usuario actual</param>
        /// <param name="cantidadPerfiles">Cantidad de perfiles a encontrar</param>
        /// <returns>Lista de perfiles de usuarios compatibles</returns>
        public async Task<List<PerfilUsuario>> EncontrarPerfilesCompatibles(int usuarioId, int cantidadPerfiles)
        {
            var perfiles = new List<PerfilUsuario>();
            
            using (var conn = _dbFactory.CreateConnection())
            {
                conn.Open();
                
                // Primero, obtener la edad del usuario actual
                var cmdEdad = new MySqlCommand(
                    "SELECT edad FROM usuarios WHERE id = @usuarioId",
                    (MySqlConnection)conn);
                cmdEdad.Parameters.AddWithValue("@usuarioId", usuarioId);
                
                int edadUsuario = 0;
                using (var reader = await cmdEdad.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        edadUsuario = reader.GetInt32(reader.GetOrdinal("edad"));
                    }
                }
                
                if (edadUsuario == 0)
                {
                    return perfiles;
                }
                
                // Calcular el rango de edad para la búsqueda
                int edadMinima = Math.Max(18, edadUsuario - RANGO_EDAD);
                int edadMaxima = edadUsuario + RANGO_EDAD;
                
                var cmd = new MySqlCommand(
                    @"SELECT 
                        u.id, 
                        u.nombre, 
                        u.edad, 
                        u.carrera, 
                        u.frase, 
                        u.intereses,
                        c.nombre as ciudad,
                        (SELECT COUNT(*) FROM interacciones 
                         WHERE usuario_id = @usuarioId AND objetivo_usuario_id = u.id) as interaccion_exists,
                        ABS(u.edad - @edadUsuario) as diferencia_edad
                    FROM usuarios u
                    JOIN ciudades c ON u.ciudad_id = c.id
                    LEFT JOIN interacciones i ON u.id = i.objetivo_usuario_id AND i.usuario_id = @usuarioId
                    WHERE u.id != @usuarioId
                      AND u.edad BETWEEN @edadMinima AND @edadMaxima
                    GROUP BY u.id
                    ORDER BY diferencia_edad ASC, interaccion_exists ASC, RAND()
                    LIMIT @cantidadPerfiles",
                    (MySqlConnection)conn);
                
                cmd.Parameters.AddWithValue("@usuarioId", usuarioId);
                cmd.Parameters.AddWithValue("@edadUsuario", edadUsuario);
                cmd.Parameters.AddWithValue("@edadMinima", edadMinima);
                cmd.Parameters.AddWithValue("@edadMaxima", edadMaxima);
                cmd.Parameters.AddWithValue("@cantidadPerfiles", cantidadPerfiles);
                
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var perfil = new PerfilUsuario
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("id")),
                            Nombre = reader["nombre"].ToString(),
                            Edad = reader.GetInt32(reader.GetOrdinal("edad")),
                            Carrera = reader["carrera"].ToString(),
                            Frase = reader["frase"].ToString(),
                            Intereses = reader["intereses"].ToString(),
                            Ciudad = reader["ciudad"].ToString(),
                            YaInteractuado = reader.GetInt32(reader.GetOrdinal("interaccion_exists")) > 0
                        };
                        
                        perfiles.Add(perfil);
                    }
                }
            }
            
            return perfiles;
        }
    }
}
