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
    /// Implementación concreta de la estrategia de emparejamiento basada en ubicación geográfica.
    /// Esta estrategia encuentra perfiles de usuarios que están cerca geográficamente.
    /// </summary>
    /// <remarks>
    /// Algoritmo: Busca usuarios en la misma ciudad primero, luego en el mismo departamento.
    /// Prioriza los perfiles más cercanos (misma ciudad sobre mismo departamento).
    /// </remarks>
    public class EmparejamientoPorUbicacionStrategy : IEmparejamientoStrategy
    {
        /// <summary>
        /// Factory para crear conexiones a la base de datos MySQL
        /// </summary>
        private readonly MySqlDbFactory _dbFactory;
        
        /// <summary>
        /// Nombre descriptivo de la estrategia para mostrar en la UI
        /// </summary>
        public string Nombre => "Cercanía Geográfica";
        
        /// <summary>
        /// Descripción detallada de la estrategia para mostrar en la UI
        /// </summary>
        public string Descripcion => "Encuentra perfiles en tu misma ciudad o departamento";
        
        /// <summary>
        /// Constructor que recibe la factory de conexiones a base de datos
        /// </summary>
        /// <param name="dbFactory">Factory para crear conexiones MySQL</param>
        public EmparejamientoPorUbicacionStrategy(MySqlDbFactory dbFactory)
        {
            _dbFactory = dbFactory;
        }
        
        /// <summary>
        /// Implementa el algoritmo para encontrar perfiles compatibles basados en ubicación geográfica.
        /// </summary>
        /// <param name="usuarioId">ID del usuario para el que se buscan perfiles compatibles</param>
        /// <param name="cantidadPerfiles">Número máximo de perfiles a devolver</param>
        /// <returns>Lista de perfiles de usuario compatibles según cercanía geográfica</returns>
        /// <remarks>
        /// Primero busca usuarios en la misma ciudad, luego en el mismo departamento.
        /// Si no encuentra suficientes perfiles, devuelve los disponibles sin filtrar por ubicación.
        /// </remarks>
        public async Task<List<PerfilUsuario>> EncontrarPerfilesCompatibles(int usuarioId, int cantidadPerfiles)
        {
            var perfiles = new List<PerfilUsuario>();
            
            using (var conn = _dbFactory.CreateConnection())
            {
                conn.Open();
                
                // Primero, obtener la ciudad y departamento del usuario actual
                var cmdUbicacion = new MySqlCommand(
                    @"SELECT c.id as ciudad_id, c.departamento_id 
                      FROM usuarios u
                      JOIN ciudades c ON u.ciudad_id = c.id
                      WHERE u.id = @usuarioId",
                    (MySqlConnection)conn);
                cmdUbicacion.Parameters.AddWithValue("@usuarioId", usuarioId);
                
                int ciudadId = 0;
                int departamentoId = 0;
                
                using (var reader = await cmdUbicacion.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        ciudadId = reader.GetInt32(reader.GetOrdinal("ciudad_id"));
                        departamentoId = reader.GetInt32(reader.GetOrdinal("departamento_id"));
                    }
                }
                
                if (ciudadId == 0 || departamentoId == 0)
                {
                    return perfiles;
                }
                
                // Buscar primero en la misma ciudad, luego en el mismo departamento
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
                        CASE 
                            WHEN c.id = @ciudadId THEN 1  -- Misma ciudad
                            WHEN c.departamento_id = @departamentoId THEN 2  -- Mismo departamento
                            ELSE 3  -- Otro departamento
                        END as prioridad
                    FROM usuarios u
                    JOIN ciudades c ON u.ciudad_id = c.id
                    LEFT JOIN interacciones i ON u.id = i.objetivo_usuario_id AND i.usuario_id = @usuarioId
                    WHERE u.id != @usuarioId
                    GROUP BY u.id
                    ORDER BY prioridad ASC, interaccion_exists ASC, RAND()
                    LIMIT @cantidadPerfiles",
                    (MySqlConnection)conn);
                
                cmd.Parameters.AddWithValue("@usuarioId", usuarioId);
                cmd.Parameters.AddWithValue("@ciudadId", ciudadId);
                cmd.Parameters.AddWithValue("@departamentoId", departamentoId);
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
