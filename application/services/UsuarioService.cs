using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using campusLove.domain.entities;
using campusLove.infraestructure.mysql;
using campusLove.domain.strategy;
using campusLove.domain.models;

namespace campusLove.application.services
{
    /// <summary>
    /// Servicio principal que gestiona la lógica de usuarios y perfiles.
    /// Implementa el patrón Strategy para permitir diferentes algoritmos de emparejamiento.
    /// </summary>
    /// <remarks>
    /// Este servicio es el contexto en el patrón Strategy:
    /// - Mantiene una referencia a la estrategia actual
    /// - Permite cambiar dinámicamente entre diferentes estrategias
    /// - Delega el algoritmo de encontrar perfiles compatibles a la estrategia seleccionada
    /// 
    /// Además, gestiona el registro de usuarios, la obtención de perfiles y la
    /// interacción entre usuarios (likes/dislikes) con su límite diario configurable.
    /// </remarks>
    public class UsuarioService
    {
        /// <summary>
        /// Factory para crear conexiones a la base de datos MySQL
        /// </summary>
        private readonly MySqlDbFactory _dbFactory;
        
        /// <summary>
        /// Referencia a la estrategia de emparejamiento actualmente seleccionada
        /// </summary>
        private IEmparejamientoStrategy _estrategiaActual;
        
        /// <summary>
        /// Lista de todas las estrategias de emparejamiento disponibles en el sistema
        /// </summary>
        private List<IEmparejamientoStrategy> _estrategiasDisponibles;

        /// <summary>
        /// Constructor que inicializa el servicio y configura las estrategias disponibles
        /// </summary>
        /// <param name="dbFactory">Factory para crear conexiones a la base de datos</param>
        public UsuarioService(MySqlDbFactory dbFactory)
        {
            _dbFactory = dbFactory;
            // Inicializamos las estrategias disponibles (implementación del patrón Strategy)
            _estrategiasDisponibles = new List<IEmparejamientoStrategy>
            {
                new EmparejamientoPorInteresesStrategy(dbFactory),
                new EmparejamientoPorUbicacionStrategy(dbFactory),
                new EmparejamientoPorEdadStrategy(dbFactory)
            };
            
            // Estrategia por defecto: por intereses
            _estrategiaActual = _estrategiasDisponibles[0];
        }
        
        /// <summary>
        /// Obtiene la lista de estrategias de emparejamiento disponibles para mostrar al usuario
        /// </summary>
        /// <returns>Lista de estrategias disponibles</returns>
        public List<IEmparejamientoStrategy> ObtenerEstrategiasDisponibles()
        {
            return _estrategiasDisponibles;
        }
        
        /// <summary>
        /// Cambia la estrategia de emparejamiento actual por otra proporcionada
        /// </summary>
        /// <param name="nuevaEstrategia">La nueva estrategia a utilizar</param>
        /// <remarks>Si se pasa null, se usa la estrategia por defecto (intereses)</remarks>
        public void CambiarEstrategia(IEmparejamientoStrategy nuevaEstrategia)
        {
            _estrategiaActual = nuevaEstrategia ?? _estrategiasDisponibles[0];
        }
        
        // Método para cambiar la estrategia por índice
        public void CambiarEstrategiaPorIndice(int indice)
        {
            if (indice >= 0 && indice < _estrategiasDisponibles.Count)
            {
                _estrategiaActual = _estrategiasDisponibles[indice];
            }
        }
        
        // Obtener la estrategia actual
        public IEmparejamientoStrategy ObtenerEstrategiaActual()
        {
            return _estrategiaActual;
        }

        public async Task RegistrarUsuario(string nombre, int edad, int genero, string intereses, 
            string carrera, string frase, int ciudadId)
        {
            using (var conn = _dbFactory.CreateConnection())
            {
                conn.Open();
                var cmd = new MySqlCommand(
                    @"INSERT INTO usuarios (nombre, edad, genero, intereses, carrera, frase, 
                    likes_diarios, max_likes_diarios, ciudad_id) 
                    VALUES (@nombre, @edad, @genero, @intereses, @carrera, @frase, 0, 10, @ciudadId)", 
                    (MySqlConnection)conn);

                cmd.Parameters.AddWithValue("@nombre", nombre);
                cmd.Parameters.AddWithValue("@edad", edad);
                cmd.Parameters.AddWithValue("@genero", genero);
                cmd.Parameters.AddWithValue("@intereses", intereses);
                cmd.Parameters.AddWithValue("@carrera", carrera);
                cmd.Parameters.AddWithValue("@frase", frase);
                cmd.Parameters.AddWithValue("@ciudadId", ciudadId);
                
                await cmd.ExecuteNonQueryAsync();
            }
        }

        // Clase PerfilUsuario trasladada a domain/models/PerfilUsuario.cs

        public async Task<List<PerfilUsuario>> ObtenerPerfiles(int currentUserId)
        {
            // Usamos la estrategia actual para encontrar perfiles compatibles
            var perfilesCompatibles = await _estrategiaActual.EncontrarPerfilesCompatibles(currentUserId, 10);
            
            // Si no se encontraron perfiles usando la estrategia, caemos en un método de respaldo
            if (perfilesCompatibles.Count == 0)
            {
                return await ObtenerPerfilesRespaldo(currentUserId);
            }
            
            return perfilesCompatibles;
        }
        
        // Método de respaldo para obtener perfiles cuando la estrategia no encuentra ninguno
        private async Task<List<PerfilUsuario>> ObtenerPerfilesRespaldo(int currentUserId)
        {
            var perfiles = new List<PerfilUsuario>();
            
            using (var conn = _dbFactory.CreateConnection())
            {
                conn.Open();
                
                // Primero, obtener perfiles con los que no se ha interactuado
                var cmd = new MySqlCommand(
                    @"SELECT u.id, u.nombre, u.edad, u.carrera, u.frase, u.intereses, c.nombre as ciudad, 0 as ya_interactuado
                    FROM usuarios u
                    LEFT JOIN ciudades c ON u.ciudad_id = c.id
                    WHERE u.id != @currentUserId
                    AND u.id NOT IN (
                        SELECT objetivo_usuario_id FROM interacciones WHERE usuario_id = @currentUserId
                    )
                    LIMIT 20", 
                    (MySqlConnection)conn);
                
                cmd.Parameters.AddWithValue("@currentUserId", currentUserId);
                
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        perfiles.Add(new PerfilUsuario
                        {
                            Id = Convert.ToInt32(reader["id"]),
                            Nombre = reader["nombre"].ToString(),
                            Edad = Convert.ToInt32(reader["edad"]),
                            Carrera = reader["carrera"].ToString(),
                            Frase = reader["frase"].ToString(),
                            Intereses = reader["intereses"].ToString(),
                            Ciudad = reader["ciudad"].ToString(),
                            YaInteractuado = false
                        });
                    }
                }
                
                // Si no hay suficientes perfiles nuevos, obtener también perfiles con los que ya se ha interactuado
                if (perfiles.Count < 5)
                {
                    cmd = new MySqlCommand(
                        @"SELECT u.id, u.nombre, u.edad, u.carrera, u.frase, u.intereses, c.nombre as ciudad, 1 as ya_interactuado
                        FROM usuarios u
                        LEFT JOIN ciudades c ON u.ciudad_id = c.id
                        WHERE u.id != @currentUserId
                        AND u.id IN (
                            SELECT objetivo_usuario_id FROM interacciones WHERE usuario_id = @currentUserId
                        )
                        LIMIT 20", 
                        (MySqlConnection)conn);
                    
                    cmd.Parameters.AddWithValue("@currentUserId", currentUserId);
                    
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            perfiles.Add(new PerfilUsuario
                            {
                                Id = Convert.ToInt32(reader["id"]),
                                Nombre = reader["nombre"].ToString(),
                                Edad = Convert.ToInt32(reader["edad"]),
                                Carrera = reader["carrera"].ToString(),
                                Frase = reader["frase"].ToString(),
                                Intereses = reader["intereses"].ToString(),
                                Ciudad = reader["ciudad"].ToString(),
                                YaInteractuado = true
                            });
                        }
                    }
                }
                
                // Si aún no hay perfiles, obtener todos los perfiles excepto el usuario actual
                if (perfiles.Count == 0)
                {
                    cmd = new MySqlCommand(
                        @"SELECT u.id, u.nombre, u.edad, u.carrera, u.frase, u.intereses, c.nombre as ciudad
                        FROM usuarios u
                        LEFT JOIN ciudades c ON u.ciudad_id = c.id
                        WHERE u.id != @currentUserId
                        LIMIT 20", 
                        (MySqlConnection)conn);
                    
                    cmd.Parameters.AddWithValue("@currentUserId", currentUserId);
                    
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            perfiles.Add(new PerfilUsuario
                            {
                                Id = Convert.ToInt32(reader["id"]),
                                Nombre = reader["nombre"].ToString(),
                                Edad = Convert.ToInt32(reader["edad"]),
                                Carrera = reader["carrera"].ToString(),
                                Frase = reader["frase"].ToString(),
                                Intereses = reader["intereses"].ToString(),
                                Ciudad = reader["ciudad"].ToString(),
                                YaInteractuado = false
                            });
                        }
                    }
                }
            }
            
            return perfiles;
        }
        
        public async Task MostrarPerfiles(int currentUserId)
        {
            var perfiles = await ObtenerPerfiles(currentUserId);
            
            foreach (var perfil in perfiles)
            {
                Console.WriteLine($"ID: {perfil.Id}, " +
                    $"Nombre: {perfil.Nombre}, " +
                    $"Edad: {perfil.Edad}, " +
                    $"Carrera: {perfil.Carrera}, " +
                    $"Frase: {perfil.Frase}");
            }
        }
    }
} 