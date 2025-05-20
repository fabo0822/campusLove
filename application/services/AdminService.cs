using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using Spectre.Console;
using campusLove.infraestructure.mysql;
using campusLove.domain.entities;

namespace campusLove.application.services
{
    public class AdminService
    {
        private readonly MySqlDbFactory _dbFactory;

        public AdminService(MySqlDbFactory dbFactory)
        {
            _dbFactory = dbFactory;
        }

        #region Métodos CRUD para Usuarios

        public async Task<bool> AñadirUsuario(string nombre, int edad, int generoId, string intereses, 
            string carrera, string frase, int ciudadId, int maxLikesDiarios, string correo, string contrasena, bool esAdmin = false)
        {
            using (var conn = _dbFactory.CreateConnection())
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // 1. Primero crear el usuario
                        var cmdUsuario = new MySqlCommand(
                            @"INSERT INTO usuarios 
                            (nombre, edad, genero, intereses, carrera, frase, ciudad_id, likes_diarios, max_likes_diarios) 
                            VALUES 
                            (@nombre, @edad, @generoId, @intereses, @carrera, @frase, @ciudadId, 0, @maxLikesDiarios);
                            SELECT LAST_INSERT_ID();", 
                            (MySqlConnection)conn);

                        cmdUsuario.Parameters.AddWithValue("@nombre", nombre);
                        cmdUsuario.Parameters.AddWithValue("@edad", edad);
                        cmdUsuario.Parameters.AddWithValue("@generoId", generoId);
                        cmdUsuario.Parameters.AddWithValue("@intereses", intereses);
                        cmdUsuario.Parameters.AddWithValue("@carrera", carrera);
                        cmdUsuario.Parameters.AddWithValue("@frase", frase);
                        cmdUsuario.Parameters.AddWithValue("@ciudadId", ciudadId);
                        cmdUsuario.Parameters.AddWithValue("@maxLikesDiarios", maxLikesDiarios);

                        // Obtener el ID del usuario recién creado
                        int usuarioId = Convert.ToInt32(await cmdUsuario.ExecuteScalarAsync());

                        // 2. Luego crear el login
                        var cmdLogin = new MySqlCommand(
                            @"INSERT INTO login
                            (usuario_id, correo, contrasena, es_admin)
                            VALUES
                            (@usuarioId, @correo, @contrasena, @esAdmin)",
                            (MySqlConnection)conn);

                        cmdLogin.Parameters.AddWithValue("@usuarioId", usuarioId);
                        cmdLogin.Parameters.AddWithValue("@correo", correo);
                        cmdLogin.Parameters.AddWithValue("@contrasena", contrasena);
                        cmdLogin.Parameters.AddWithValue("@esAdmin", esAdmin);

                        await cmdLogin.ExecuteNonQueryAsync();

                        // 3. Crear registro de estadísticas
                        var cmdEstadisticas = new MySqlCommand(
                            @"INSERT INTO estadisticas
                            (usuario_id, likes_recibidos, coincidencias_totales)
                            VALUES
                            (@usuarioId, 0, 0)",
                            (MySqlConnection)conn);

                        cmdEstadisticas.Parameters.AddWithValue("@usuarioId", usuarioId);
                        await cmdEstadisticas.ExecuteNonQueryAsync();

                        // Confirmar la transacción
                        transaction.Commit();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        // Revertir la transacción si algo falla
                        transaction.Rollback();
                        Console.WriteLine($"Error al añadir usuario: {ex.Message}");
                        return false;
                    }
                }
            }
        }

        public async Task<bool> EditarUsuario(int usuarioId, string nombre, int edad, int generoId, 
            string intereses, string carrera, string frase, int ciudadId, int maxLikesDiarios)
        {
            using (var conn = _dbFactory.CreateConnection())
            {
                conn.Open();
                try
                {
                    var cmd = new MySqlCommand(
                        @"UPDATE usuarios SET
                        nombre = @nombre,
                        edad = @edad,
                        genero = @generoId,
                        intereses = @intereses,
                        carrera = @carrera,
                        frase = @frase,
                        ciudad_id = @ciudadId,
                        max_likes_diarios = @maxLikesDiarios
                        WHERE id = @usuarioId", 
                        (MySqlConnection)conn);

                    cmd.Parameters.AddWithValue("@nombre", nombre);
                    cmd.Parameters.AddWithValue("@edad", edad);
                    cmd.Parameters.AddWithValue("@generoId", generoId);
                    cmd.Parameters.AddWithValue("@intereses", intereses);
                    cmd.Parameters.AddWithValue("@carrera", carrera);
                    cmd.Parameters.AddWithValue("@frase", frase);
                    cmd.Parameters.AddWithValue("@ciudadId", ciudadId);
                    cmd.Parameters.AddWithValue("@maxLikesDiarios", maxLikesDiarios);
                    cmd.Parameters.AddWithValue("@usuarioId", usuarioId);

                    int filasAfectadas = await cmd.ExecuteNonQueryAsync();
                    return filasAfectadas > 0;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al editar usuario: {ex.Message}");
                    return false;
                }
            }
        }

        public async Task<bool> EliminarUsuario(int usuarioId)
        {
            using (var conn = _dbFactory.CreateConnection())
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // 1. Eliminar estadísticas
                        var cmdEstadisticas = new MySqlCommand(
                            "DELETE FROM estadisticas WHERE usuario_id = @usuarioId", 
                            (MySqlConnection)conn);
                        cmdEstadisticas.Parameters.AddWithValue("@usuarioId", usuarioId);
                        await cmdEstadisticas.ExecuteNonQueryAsync();

                        // 2. Eliminar coincidencias
                        var cmdCoincidencias = new MySqlCommand(
                            "DELETE FROM coincidencias WHERE usuario1_id = @usuarioId OR usuario2_id = @usuarioId", 
                            (MySqlConnection)conn);
                        cmdCoincidencias.Parameters.AddWithValue("@usuarioId", usuarioId);
                        await cmdCoincidencias.ExecuteNonQueryAsync();

                        // 3. Eliminar interacciones
                        var cmdInteracciones = new MySqlCommand(
                            "DELETE FROM interacciones WHERE usuario_id = @usuarioId OR objetivo_usuario_id = @usuarioId", 
                            (MySqlConnection)conn);
                        cmdInteracciones.Parameters.AddWithValue("@usuarioId", usuarioId);
                        await cmdInteracciones.ExecuteNonQueryAsync();

                        // 4. Eliminar login
                        var cmdLogin = new MySqlCommand(
                            "DELETE FROM login WHERE usuario_id = @usuarioId", 
                            (MySqlConnection)conn);
                        cmdLogin.Parameters.AddWithValue("@usuarioId", usuarioId);
                        await cmdLogin.ExecuteNonQueryAsync();

                        // 5. Eliminar usuario
                        var cmdUsuario = new MySqlCommand(
                            "DELETE FROM usuarios WHERE id = @usuarioId", 
                            (MySqlConnection)conn);
                        cmdUsuario.Parameters.AddWithValue("@usuarioId", usuarioId);
                        await cmdUsuario.ExecuteNonQueryAsync();

                        // Confirmar la transacción
                        transaction.Commit();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        // Revertir la transacción si algo falla
                        transaction.Rollback();
                        Console.WriteLine($"Error al eliminar usuario: {ex.Message}");
                        return false;
                    }
                }
            }
        }

        public async Task<Usuario> ObtenerDetallesUsuario(int usuarioId)
        {
            using (var conn = _dbFactory.CreateConnection())
            {
                conn.Open();
                try
                {
                    var cmd = new MySqlCommand(
                        @"SELECT u.*, l.correo, l.es_admin
                        FROM usuarios u
                        JOIN login l ON u.id = l.usuario_id
                        WHERE u.id = @usuarioId", 
                        (MySqlConnection)conn);

                    cmd.Parameters.AddWithValue("@usuarioId", usuarioId);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new Usuario
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("id")),
                                Nombre = reader.GetString(reader.GetOrdinal("nombre")),
                                Edad = reader.GetInt32(reader.GetOrdinal("edad")),
                                GeneroId = reader.GetInt32(reader.GetOrdinal("genero")),
                                Intereses = reader.GetString(reader.GetOrdinal("intereses")),
                                Carrera = reader.GetString(reader.GetOrdinal("carrera")),
                                Frase = reader.GetString(reader.GetOrdinal("frase")),
                                CiudadId = reader.GetInt32(reader.GetOrdinal("ciudad_id")),
                                LikesDiarios = reader.GetInt32(reader.GetOrdinal("likes_diarios")),
                                MaxLikesDiarios = reader.GetInt32(reader.GetOrdinal("max_likes_diarios")),
                                Login = new Login
                                {
                                    Correo = reader.GetString(reader.GetOrdinal("correo")),
                                    EsAdmin = reader.GetBoolean(reader.GetOrdinal("es_admin"))
                                }
                            };
                        }
                        return null;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al obtener detalles del usuario: {ex.Message}");
                    return null;
                }
            }
        }

        #endregion

        #region Métodos CRUD para Interacciones

        public async Task<bool> AñadirInteraccion(int usuarioEmisorId, int usuarioReceptorId, bool esLike)
        {
            using (var conn = _dbFactory.CreateConnection())
            {
                conn.Open();
                try
                {
                    var cmd = new MySqlCommand(
                        @"INSERT INTO interacciones
                        (usuario_id, objetivo_usuario_id, le_gusto, fecha_interaccion)
                        VALUES
                        (@emisorId, @receptorId, @esLike, NOW())", 
                        (MySqlConnection)conn);

                    cmd.Parameters.AddWithValue("@emisorId", usuarioEmisorId);
                    cmd.Parameters.AddWithValue("@receptorId", usuarioReceptorId);
                    cmd.Parameters.AddWithValue("@esLike", esLike);

                    await cmd.ExecuteNonQueryAsync();
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al añadir interacción: {ex.Message}");
                    return false;
                }
            }
        }

        public async Task<bool> EliminarInteraccion(int interaccionId)
        {
            using (var conn = _dbFactory.CreateConnection())
            {
                conn.Open();
                try
                {
                    var cmd = new MySqlCommand(
                        "DELETE FROM interacciones WHERE id = @interaccionId", 
                        (MySqlConnection)conn);

                    cmd.Parameters.AddWithValue("@interaccionId", interaccionId);

                    int filasAfectadas = await cmd.ExecuteNonQueryAsync();
                    return filasAfectadas > 0;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al eliminar interacción: {ex.Message}");
                    return false;
                }
            }
        }

        #endregion

        #region Métodos CRUD para Coincidencias

        public async Task<bool> EliminarCoincidencia(int coincidenciaId)
        {
            using (var conn = _dbFactory.CreateConnection())
            {
                conn.Open();
                try
                {
                    var cmd = new MySqlCommand(
                        "DELETE FROM coincidencias WHERE id = @coincidenciaId", 
                        (MySqlConnection)conn);

                    cmd.Parameters.AddWithValue("@coincidenciaId", coincidenciaId);

                    int filasAfectadas = await cmd.ExecuteNonQueryAsync();
                    return filasAfectadas > 0;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al eliminar coincidencia: {ex.Message}");
                    return false;
                }
            }
        }

        #endregion

        #region Métodos CRUD para Ciudades

        public async Task<bool> AñadirCiudad(string nombre, int departamentoId)
        {
            using (var conn = _dbFactory.CreateConnection())
            {
                conn.Open();
                try
                {
                    var cmd = new MySqlCommand(
                        @"INSERT INTO ciudades
                        (id, nombre, departamento_id)
                        VALUES
                        ((SELECT IFNULL(MAX(id), 0) + 1 FROM ciudades c), @nombre, @departamentoId)", 
                        (MySqlConnection)conn);

                    cmd.Parameters.AddWithValue("@nombre", nombre);
                    cmd.Parameters.AddWithValue("@departamentoId", departamentoId);

                    await cmd.ExecuteNonQueryAsync();
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al añadir ciudad: {ex.Message}");
                    return false;
                }
            }
        }

        public async Task<bool> EditarCiudad(int ciudadId, string nombre, int departamentoId)
        {
            using (var conn = _dbFactory.CreateConnection())
            {
                conn.Open();
                try
                {
                    var cmd = new MySqlCommand(
                        @"UPDATE ciudades SET
                        nombre = @nombre,
                        departamento_id = @departamentoId
                        WHERE id = @ciudadId", 
                        (MySqlConnection)conn);

                    cmd.Parameters.AddWithValue("@nombre", nombre);
                    cmd.Parameters.AddWithValue("@departamentoId", departamentoId);
                    cmd.Parameters.AddWithValue("@ciudadId", ciudadId);

                    int filasAfectadas = await cmd.ExecuteNonQueryAsync();
                    return filasAfectadas > 0;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al editar ciudad: {ex.Message}");
                    return false;
                }
            }
        }

        public async Task<bool> EliminarCiudad(int ciudadId)
        {
            using (var conn = _dbFactory.CreateConnection())
            {
                conn.Open();
                try
                {
                    // Primero verificar que no haya usuarios asociados a esta ciudad
                    var cmdVerificar = new MySqlCommand(
                        "SELECT COUNT(*) FROM usuarios WHERE ciudad_id = @ciudadId", 
                        (MySqlConnection)conn);
                    cmdVerificar.Parameters.AddWithValue("@ciudadId", ciudadId);
                    
                    int usuariosAsociados = Convert.ToInt32(await cmdVerificar.ExecuteScalarAsync());
                    if (usuariosAsociados > 0)
                    {
                        return false; // No se puede eliminar si hay usuarios asociados
                    }

                    // Eliminar la ciudad
                    var cmd = new MySqlCommand(
                        "DELETE FROM ciudades WHERE id = @ciudadId", 
                        (MySqlConnection)conn);
                    cmd.Parameters.AddWithValue("@ciudadId", ciudadId);

                    int filasAfectadas = await cmd.ExecuteNonQueryAsync();
                    return filasAfectadas > 0;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al eliminar ciudad: {ex.Message}");
                    return false;
                }
            }
        }

        #endregion

        #region Métodos CRUD para Departamentos

        public async Task<bool> AñadirDepartamento(string nombre, int paisId)
        {
            using (var conn = _dbFactory.CreateConnection())
            {
                conn.Open();
                try
                {
                    var cmd = new MySqlCommand(
                        @"INSERT INTO departamentos
                        (id, nombre, pais_id)
                        VALUES
                        ((SELECT IFNULL(MAX(id), 0) + 1 FROM departamentos d), @nombre, @paisId)", 
                        (MySqlConnection)conn);

                    cmd.Parameters.AddWithValue("@nombre", nombre);
                    cmd.Parameters.AddWithValue("@paisId", paisId);

                    await cmd.ExecuteNonQueryAsync();
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al añadir departamento: {ex.Message}");
                    return false;
                }
            }
        }

        public async Task<bool> EditarDepartamento(int departamentoId, string nombre, int paisId)
        {
            using (var conn = _dbFactory.CreateConnection())
            {
                conn.Open();
                try
                {
                    var cmd = new MySqlCommand(
                        @"UPDATE departamentos SET
                        nombre = @nombre,
                        pais_id = @paisId
                        WHERE id = @departamentoId", 
                        (MySqlConnection)conn);

                    cmd.Parameters.AddWithValue("@nombre", nombre);
                    cmd.Parameters.AddWithValue("@paisId", paisId);
                    cmd.Parameters.AddWithValue("@departamentoId", departamentoId);

                    int filasAfectadas = await cmd.ExecuteNonQueryAsync();
                    return filasAfectadas > 0;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al editar departamento: {ex.Message}");
                    return false;
                }
            }
        }

        public async Task<bool> EliminarDepartamento(int departamentoId)
        {
            using (var conn = _dbFactory.CreateConnection())
            {
                conn.Open();
                try
                {
                    // Primero verificar que no haya ciudades asociadas a este departamento
                    var cmdVerificar = new MySqlCommand(
                        "SELECT COUNT(*) FROM ciudades WHERE departamento_id = @departamentoId", 
                        (MySqlConnection)conn);
                    cmdVerificar.Parameters.AddWithValue("@departamentoId", departamentoId);
                    
                    int ciudadesAsociadas = Convert.ToInt32(await cmdVerificar.ExecuteScalarAsync());
                    if (ciudadesAsociadas > 0)
                    {
                        return false; // No se puede eliminar si hay ciudades asociadas
                    }

                    // Eliminar el departamento
                    var cmd = new MySqlCommand(
                        "DELETE FROM departamentos WHERE id = @departamentoId", 
                        (MySqlConnection)conn);
                    cmd.Parameters.AddWithValue("@departamentoId", departamentoId);

                    int filasAfectadas = await cmd.ExecuteNonQueryAsync();
                    return filasAfectadas > 0;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al eliminar departamento: {ex.Message}");
                    return false;
                }
            }
        }

        #endregion

        #region Métodos CRUD para Géneros

        public async Task<bool> AñadirGenero(string nombre)
        {
            using (var conn = _dbFactory.CreateConnection())
            {
                conn.Open();
                try
                {
                    var cmd = new MySqlCommand(
                        @"INSERT INTO generos
                        (id, descripcion)
                        VALUES
                        ((SELECT IFNULL(MAX(id), 0) + 1 FROM generos g), @descripcion)", 
                        (MySqlConnection)conn);

                    cmd.Parameters.AddWithValue("@descripcion", nombre);

                    await cmd.ExecuteNonQueryAsync();
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al añadir género: {ex.Message}");
                    return false;
                }
            }
        }

        public async Task<bool> EditarGenero(int generoId, string nombre)
        {
            using (var conn = _dbFactory.CreateConnection())
            {
                conn.Open();
                try
                {
                    var cmd = new MySqlCommand(
                        @"UPDATE generos SET
                        descripcion = @descripcion
                        WHERE id = @generoId", 
                        (MySqlConnection)conn);

                    cmd.Parameters.AddWithValue("@descripcion", nombre);
                    cmd.Parameters.AddWithValue("@generoId", generoId);

                    int filasAfectadas = await cmd.ExecuteNonQueryAsync();
                    return filasAfectadas > 0;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al editar género: {ex.Message}");
                    return false;
                }
            }
        }

        public async Task<bool> EliminarGenero(int generoId)
        {
            using (var conn = _dbFactory.CreateConnection())
            {
                conn.Open();
                try
                {
                    // Primero verificar que no haya usuarios asociados a este género
                    var cmdVerificar = new MySqlCommand(
                        "SELECT COUNT(*) FROM usuarios WHERE genero = @generoId", 
                        (MySqlConnection)conn);
                    cmdVerificar.Parameters.AddWithValue("@generoId", generoId);
                    
                    int usuariosAsociados = Convert.ToInt32(await cmdVerificar.ExecuteScalarAsync());
                    if (usuariosAsociados > 0)
                    {
                        return false; // No se puede eliminar si hay usuarios asociados
                    }

                    // Eliminar el género
                    var cmd = new MySqlCommand(
                        "DELETE FROM generos WHERE id = @generoId", 
                        (MySqlConnection)conn);
                    cmd.Parameters.AddWithValue("@generoId", generoId);

                    int filasAfectadas = await cmd.ExecuteNonQueryAsync();
                    return filasAfectadas > 0;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al eliminar género: {ex.Message}");
                    return false;
                }
            }
        }

        #endregion

        // Método para promover/degradar a un usuario como administrador
        public async Task<bool> CambiarEstadoAdmin(int usuarioId, bool esAdmin)
        {
            using (var conn = _dbFactory.CreateConnection())
            {
                conn.Open();
                try
                {
                    var cmd = new MySqlCommand(
                        @"UPDATE login SET
                        es_admin = @esAdmin
                        WHERE usuario_id = @usuarioId", 
                        (MySqlConnection)conn);

                    cmd.Parameters.AddWithValue("@esAdmin", esAdmin);
                    cmd.Parameters.AddWithValue("@usuarioId", usuarioId);

                    int filasAfectadas = await cmd.ExecuteNonQueryAsync();
                    return filasAfectadas > 0;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al cambiar estado de administrador: {ex.Message}");
                    return false;
                }
            }
        }
    }
}
