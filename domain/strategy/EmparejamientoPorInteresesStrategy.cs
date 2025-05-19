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
    /// Implementación concreta de la estrategia de emparejamiento basada en intereses comunes.
    /// Esta estrategia encuentra perfiles de usuarios que comparten intereses similares.
    /// </summary>
    /// <remarks>
    /// Algoritmo: Busca palabras clave compartidas en el campo 'intereses' de los usuarios.
    /// Prioriza los perfiles con mayor cantidad de intereses en común.
    /// </remarks>
    public class EmparejamientoPorInteresesStrategy : IEmparejamientoStrategy
    {
        /// <summary>
        /// Factory para crear conexiones a la base de datos MySQL
        /// </summary>
        private readonly MySqlDbFactory _dbFactory;
        
        /// <summary>
        /// Nombre descriptivo de la estrategia para mostrar en la UI
        /// </summary>
        public string Nombre => "Intereses Comunes";
        
        /// <summary>
        /// Descripción detallada de la estrategia para mostrar en la UI
        /// </summary>
        public string Descripcion => "Encuentra perfiles que comparten tus intereses y pasiones";
        
        /// <summary>
        /// Constructor que recibe la factory de conexiones a base de datos
        /// </summary>
        /// <param name="dbFactory">Factory para crear conexiones MySQL</param>
        public EmparejamientoPorInteresesStrategy(MySqlDbFactory dbFactory)
        {
            _dbFactory = dbFactory;
        }
        
        /// <summary>
        /// Implementa el algoritmo para encontrar perfiles compatibles basados en intereses comunes.
        /// </summary>
        /// <param name="usuarioId">ID del usuario para el que se buscan perfiles compatibles</param>
        /// <param name="cantidadPerfiles">Número máximo de perfiles a devolver</param>
        /// <returns>Lista de perfiles de usuario compatibles según intereses comunes</returns>
        public async Task<List<PerfilUsuario>> EncontrarPerfilesCompatibles(int usuarioId, int cantidadPerfiles)
        {
            var perfiles = new List<PerfilUsuario>();
            
            using (var conn = _dbFactory.CreateConnection())
            {
                conn.Open();
                
                // Primero, obtener los intereses del usuario actual
                var cmdIntereses = new MySqlCommand(
                    @"SELECT intereses FROM usuarios WHERE id = @usuarioId",
                    (MySqlConnection)conn);
                cmdIntereses.Parameters.AddWithValue("@usuarioId", usuarioId);
                
                string interesesUsuario = string.Empty;
                using (var reader = await cmdIntereses.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        interesesUsuario = reader["intereses"].ToString();
                    }
                }
                
                if (string.IsNullOrEmpty(interesesUsuario))
                {
                    return perfiles;
                }
                
                // Dividir los intereses en palabras clave
                var palabrasClave = interesesUsuario.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                
                // Construir la consulta SQL con condiciones LIKE para cada palabra clave
                var condiciones = new List<string>();
                foreach (var palabra in palabrasClave)
                {
                    if (!string.IsNullOrWhiteSpace(palabra))
                    {
                        condiciones.Add($"u.intereses LIKE @palabra{condiciones.Count}");
                    }
                }
                
                string condicionSQL = string.Join(" OR ", condiciones);
                
                var cmd = new MySqlCommand(
                    $@"SELECT 
                        u.id, 
                        u.nombre, 
                        u.edad, 
                        u.carrera, 
                        u.frase, 
                        u.intereses,
                        c.nombre as ciudad,
                        (SELECT COUNT(*) FROM interacciones 
                         WHERE usuario_id = @usuarioId AND objetivo_usuario_id = u.id) as interaccion_exists
                    FROM usuarios u
                    JOIN ciudades c ON u.ciudad_id = c.id
                    LEFT JOIN interacciones i ON u.id = i.objetivo_usuario_id AND i.usuario_id = @usuarioId
                    WHERE u.id != @usuarioId AND ({condicionSQL})
                    GROUP BY u.id
                    ORDER BY interaccion_exists ASC, (
                        SELECT COUNT(*) FROM interacciones 
                        WHERE objetivo_usuario_id = u.id AND le_gusto = 1
                    ) DESC
                    LIMIT @cantidadPerfiles",
                    (MySqlConnection)conn);
                
                cmd.Parameters.AddWithValue("@usuarioId", usuarioId);
                cmd.Parameters.AddWithValue("@cantidadPerfiles", cantidadPerfiles);
                
                // Agregar parámetros para las condiciones LIKE
                for (int i = 0; i < condiciones.Count; i++)
                {
                    cmd.Parameters.AddWithValue($"@palabra{i}", $"%{palabrasClave[i]}%");
                }
                
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
