using System;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using campusLove.domain.entities;
using campusLove.infraestructure.mysql;

namespace campusLove.application.services
{
    public class UsuarioService
    {
        private readonly MySqlDbFactory _dbFactory;

        public UsuarioService(MySqlDbFactory dbFactory)
        {
            _dbFactory = dbFactory;
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

        public class PerfilUsuario
        {
            public int Id { get; set; }
            public string Nombre { get; set; }
            public int Edad { get; set; }
            public string Carrera { get; set; }
            public string Frase { get; set; }
            public string Intereses { get; set; }
            public string Ciudad { get; set; }
            public bool YaInteractuado { get; set; }
        }

        public async Task<List<PerfilUsuario>> ObtenerPerfiles(int currentUserId)
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