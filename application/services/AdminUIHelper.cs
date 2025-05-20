using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Spectre.Console;
using campusLove.domain.entities;
using campusLove.infraestructure.mysql;
using MySql.Data.MySqlClient;

namespace campusLove.application.services
{
    /// <summary>
    /// Clase auxiliar que contiene métodos de interfaz de usuario para el panel de administración
    /// </summary>
    public class AdminUIHelper
    {
        private readonly AdminService _adminService;
        private readonly MySqlDbFactory _dbFactory;

        public AdminUIHelper(AdminService adminService, MySqlDbFactory dbFactory)
        {
            _adminService = adminService;
            _dbFactory = dbFactory;
        }
        
        /// <summary>
        /// Muestra un mensaje estándar de "En construcción" para funcionalidades que están en desarrollo
        /// </summary>
        /// <param name="titulo">Título de la funcionalidad en construcción</param>
        /// <returns>Task</returns>
        private async Task MostrarMensajeEnConstruccion(string titulo = "")
        {
            Console.Clear();
            
            var panel = new Panel("[bold yellow]Esta funcionalidad está en desarrollo[/]");
            panel.Border = BoxBorder.Rounded;
            panel.Padding = new Padding(2, 1, 2, 1);
            panel.BorderColor(Color.Yellow);
            
            AnsiConsole.Write(
                new FigletText("En Construcción")
                    .Centered()
                    .Color(Color.Yellow));
            
            AnsiConsole.WriteLine();
            AnsiConsole.Write(panel);
            AnsiConsole.WriteLine();
            
            if (!string.IsNullOrEmpty(titulo))
            {
                AnsiConsole.MarkupLine($"[bold]Funcionalidad:[/] {titulo}");
                AnsiConsole.WriteLine();
            }
            
            AnsiConsole.MarkupLine("Esta funcionalidad estará disponible en una próxima actualización.");
            AnsiConsole.WriteLine("\nPresione cualquier tecla para volver al menú anterior.");
            Console.ReadKey();
        }

        #region Métodos CRUD para Usuarios

        /// <summary>
        /// Interfaz de usuario para añadir un nuevo usuario
        /// </summary>
        public async Task AñadirUsuario()
        {
            Console.Clear();
            
            // Crear título con Spectre.Console
            AnsiConsole.Write(
                new FigletText("Añadir Usuario")
                    .Centered()
                    .Color(Color.Green));
            
            AnsiConsole.WriteLine();
            
            // Solicitar datos del nuevo usuario
            string nombre = AnsiConsole.Prompt(
                new TextPrompt<string>("[bold]Nombre:[/] ")
                    .PromptStyle("green")
                    .ValidationErrorMessage("[red]El nombre debe tener al menos 3 caracteres[/]")
                    .Validate(name => 
                    {
                        return !string.IsNullOrWhiteSpace(name) && name.Length >= 3 
                            ? ValidationResult.Success() 
                            : ValidationResult.Error();
                    }));
            
            int edad = AnsiConsole.Prompt(
                new TextPrompt<int>("[bold]Edad:[/] ")
                    .PromptStyle("green")
                    .ValidationErrorMessage("[red]La edad debe ser un número entre 18 y 99[/]")
                    .Validate(age => 
                    {
                        return age >= 18 && age <= 99 
                            ? ValidationResult.Success() 
                            : ValidationResult.Error();
                    }));
            
            // Obtener géneros disponibles
            var generos = new Dictionary<string, int>();
            using (var conn = _dbFactory.CreateConnection())
            {
                conn.Open();
                var cmd = new MySqlCommand("SELECT id, descripcion FROM generos", (MySqlConnection)conn);
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        generos.Add(
                            reader["descripcion"].ToString(),
                            Convert.ToInt32(reader["id"])
                        );
                    }
                }
            }
            
            var generoSeleccionado = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[bold]Género:[/]")
                    .PageSize(5)
                    .HighlightStyle(new Style(foreground: Color.Green))
                    .AddChoices(generos.Keys));
            
            int generoId = generos[generoSeleccionado];
            
            string intereses = AnsiConsole.Prompt(
                new TextPrompt<string>("[bold]Intereses:[/] ")
                    .PromptStyle("green")
                    .AllowEmpty());
            
            string carrera = AnsiConsole.Prompt(
                new TextPrompt<string>("[bold]Carrera:[/] ")
                    .PromptStyle("green")
                    .AllowEmpty());
            
            string frase = AnsiConsole.Prompt(
                new TextPrompt<string>("[bold]Frase:[/] ")
                    .PromptStyle("green")
                    .AllowEmpty());
            
            // Obtener ciudades disponibles
            var ciudades = new Dictionary<string, int>();
            using (var conn = _dbFactory.CreateConnection())
            {
                conn.Open();
                var cmd = new MySqlCommand("SELECT id, nombre FROM ciudades", (MySqlConnection)conn);
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        ciudades.Add(
                            reader["nombre"].ToString(),
                            Convert.ToInt32(reader["id"])
                        );
                    }
                }
            }
            
            var ciudadSeleccionada = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[bold]Ciudad:[/]")
                    .PageSize(10)
                    .HighlightStyle(new Style(foreground: Color.Green))
                    .AddChoices(ciudades.Keys));
            
            int ciudadId = ciudades[ciudadSeleccionada];
            
            int maxLikesDiarios = AnsiConsole.Prompt(
                new TextPrompt<int>("[bold]Máximo de likes diarios:[/] ")
                    .PromptStyle("green")
                    .DefaultValue(10)
                    .ValidationErrorMessage("[red]Debe ser un número mayor a 0[/]")
                    .Validate(likes => 
                    {
                        return likes > 0 
                            ? ValidationResult.Success() 
                            : ValidationResult.Error();
                    }));
            
            string correo = AnsiConsole.Prompt(
                new TextPrompt<string>("[bold]Correo:[/] ")
                    .PromptStyle("green")
                    .ValidationErrorMessage("[red]El correo debe contener '@'[/]")
                    .Validate(email => 
                    {
                        return !string.IsNullOrWhiteSpace(email) && email.Contains('@') 
                            ? ValidationResult.Success() 
                            : ValidationResult.Error();
                    }));
            
            string contrasena = AnsiConsole.Prompt(
                new TextPrompt<string>("[bold]Contraseña:[/] ")
                    .PromptStyle("green")
                    .Secret()
                    .ValidationErrorMessage("[red]La contraseña debe tener al menos 5 caracteres[/]")
                    .Validate(pass => 
                    {
                        return !string.IsNullOrWhiteSpace(pass) && pass.Length >= 5 
                            ? ValidationResult.Success() 
                            : ValidationResult.Error();
                    }));
            
            bool esAdmin = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[bold]¿Es administrador?[/]")
                    .PageSize(3)
                    .HighlightStyle(new Style(foreground: Color.Green))
                    .AddChoices(new[] { "No", "Sí" })) == "Sí";
            
            // Confirmar creación
            var crear = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[bold]¿Desea crear este usuario?[/]")
                    .PageSize(3)
                    .HighlightStyle(new Style(foreground: Color.Green))
                    .AddChoices(new[] { "Cancelar", "Crear" }));
            
            if (crear == "Crear")
            {
                // Mostrar spinner mientras se procesa
                var resultado = false;
                await AnsiConsole.Status()
                    .StartAsync("Creando usuario...", async ctx => 
                    {
                        resultado = await _adminService.AñadirUsuario(
                            nombre, edad, generoId, intereses, carrera, frase, 
                            ciudadId, maxLikesDiarios, correo, contrasena, esAdmin);
                    });
                
                if (resultado)
                {
                    AnsiConsole.MarkupLine("\n[green]Usuario creado exitosamente.[/]");
                }
                else
                {
                    AnsiConsole.MarkupLine("\n[red]Error al crear el usuario. Puede que el correo ya esté en uso.[/]");
                }
            }
            else
            {
                AnsiConsole.MarkupLine("\n[yellow]Operación cancelada.[/]");
            }
            
            AnsiConsole.WriteLine("\nPresione cualquier tecla para continuar.");
            Console.ReadKey();
        }

        /// <summary>
        /// Interfaz de usuario para editar un usuario existente
        /// </summary>
        public async Task EditarUsuario()
        {
            Console.Clear();
            
            // Crear título con Spectre.Console
            AnsiConsole.Write(
                new FigletText("Editar Usuario")
                    .Centered()
                    .Color(Color.Green));
            
            AnsiConsole.WriteLine();
            
            // Primero obtener la lista de usuarios
            var usuarios = new Dictionary<string, int>();
            
            using (var conn = _dbFactory.CreateConnection())
            {
                conn.Open();
                var cmd = new MySqlCommand(
                    "SELECT u.id, u.nombre FROM usuarios u ORDER BY u.nombre", 
                    (MySqlConnection)conn);
                
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        usuarios.Add(
                            $"{reader["nombre"]} (ID: {reader["id"]})",
                            Convert.ToInt32(reader["id"])
                        );
                    }
                }
            }
            
            if (usuarios.Count == 0)
            {
                AnsiConsole.MarkupLine("[red]No hay usuarios registrados.[/]");
                AnsiConsole.WriteLine("\nPresione cualquier tecla para continuar.");
                Console.ReadKey();
                return;
            }
            
            // Seleccionar usuario a editar
            var usuarioSeleccionado = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[bold]Seleccione el usuario a editar:[/]")
                    .PageSize(10)
                    .HighlightStyle(new Style(foreground: Color.Green))
                    .AddChoices(usuarios.Keys));
            
            int usuarioId = usuarios[usuarioSeleccionado];
            
            // Obtener datos actuales del usuario
            Usuario usuario = null;
            string correoActual = "";
            bool esAdminActual = false;
            
            using (var conn = _dbFactory.CreateConnection())
            {
                conn.Open();
                
                // Obtener datos del usuario
                var cmdUsuario = new MySqlCommand(
                    "SELECT * FROM usuarios WHERE id = @usuarioId", 
                    (MySqlConnection)conn);
                cmdUsuario.Parameters.AddWithValue("@usuarioId", usuarioId);
                
                using (var reader = await cmdUsuario.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        usuario = new Usuario
                        {
                            Id = usuarioId,
                            Nombre = reader["nombre"].ToString(),
                            Edad = Convert.ToInt32(reader["edad"]),
                            GeneroId = Convert.ToInt32(reader["genero"]),
                            Intereses = reader["intereses"].ToString(),
                            Carrera = reader["carrera"].ToString(),
                            Frase = reader["frase"].ToString(),
                            LikesDiarios = Convert.ToInt32(reader["likes_diarios"]),
                            MaxLikesDiarios = Convert.ToInt32(reader["max_likes_diarios"]),
                            CiudadId = Convert.ToInt32(reader["ciudad_id"])
                        };
                    }
                }
                
                // Obtener datos de login
                var cmdLogin = new MySqlCommand(
                    "SELECT correo, es_admin FROM login WHERE usuario_id = @usuarioId", 
                    (MySqlConnection)conn);
                cmdLogin.Parameters.AddWithValue("@usuarioId", usuarioId);
                
                using (var reader = await cmdLogin.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        correoActual = reader["correo"].ToString();
                        esAdminActual = Convert.ToBoolean(reader["es_admin"]);
                    }
                }
            }
            
            if (usuario == null)
            {
                AnsiConsole.MarkupLine("[red]No se encontró el usuario seleccionado.[/]");
                AnsiConsole.WriteLine("\nPresione cualquier tecla para continuar.");
                Console.ReadKey();
                return;
            }
            
            // Mostrar formulario con datos actuales y permitir editar
            AnsiConsole.MarkupLine($"\n[bold]Editando usuario:[/] {usuario.Nombre} (ID: {usuario.Id})");
            AnsiConsole.WriteLine("\nDeje en blanco para mantener el valor actual.");
            AnsiConsole.WriteLine();
            
            // Solicitar nuevo nombre (opcional)
            string nuevoNombre = AnsiConsole.Prompt(
                new TextPrompt<string>($"[bold]Nombre:[/] [dim]({usuario.Nombre})[/] ")
                    .PromptStyle("green")
                    .AllowEmpty());
            
            // Si no se ingresa un nuevo valor, mantener el actual
            if (string.IsNullOrWhiteSpace(nuevoNombre))
            {
                nuevoNombre = usuario.Nombre;
            }
            
            // Solicitar nueva edad (opcional)
            string edadInput = AnsiConsole.Prompt(
                new TextPrompt<string>($"[bold]Edad:[/] [dim]({usuario.Edad})[/] ")
                    .PromptStyle("green")
                    .AllowEmpty());
            
            int nuevaEdad = usuario.Edad;
            if (!string.IsNullOrWhiteSpace(edadInput) && int.TryParse(edadInput, out int edad))
            {
                if (edad >= 18 && edad <= 99)
                {
                    nuevaEdad = edad;
                }
                else
                {
                    AnsiConsole.MarkupLine("[yellow]Edad no válida. Se mantendrá el valor actual.[/]");
                }
            }
            
            // Obtener géneros
            var generos = new Dictionary<string, int>();
            string generoActual = "";
            
            using (var conn = _dbFactory.CreateConnection())
            {
                conn.Open();
                var cmd = new MySqlCommand("SELECT id, descripcion FROM generos", (MySqlConnection)conn);
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        int id = Convert.ToInt32(reader["id"]);
                        string descripcion = reader["descripcion"].ToString();
                        generos.Add(descripcion, id);
                        
                        if (id == usuario.GeneroId)
                        {
                            generoActual = descripcion;
                        }
                    }
                }
            }
            
            // Seleccionar nuevo género
            var opciones = new List<string> { $"<Mantener actual: {generoActual}>" };
            opciones.AddRange(generos.Keys);
            
            var generoSeleccionado = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title($"[bold]Género:[/]")
                    .PageSize(5)
                    .HighlightStyle(new Style(foreground: Color.Green))
                    .AddChoices(opciones));
            
            int nuevoGeneroId = usuario.GeneroId;
            if (generoSeleccionado != $"<Mantener actual: {generoActual}>")
            {
                nuevoGeneroId = generos[generoSeleccionado];
            }
            
            // Solicitar nuevos intereses
            string nuevosIntereses = AnsiConsole.Prompt(
                new TextPrompt<string>($"[bold]Intereses:[/] [dim]({usuario.Intereses})[/] ")
                    .PromptStyle("green")
                    .AllowEmpty());
            
            if (string.IsNullOrWhiteSpace(nuevosIntereses))
            {
                nuevosIntereses = usuario.Intereses;
            }
            
            // Solicitar nueva carrera
            string nuevaCarrera = AnsiConsole.Prompt(
                new TextPrompt<string>($"[bold]Carrera:[/] [dim]({usuario.Carrera})[/] ")
                    .PromptStyle("green")
                    .AllowEmpty());
            
            if (string.IsNullOrWhiteSpace(nuevaCarrera))
            {
                nuevaCarrera = usuario.Carrera;
            }
            
            // Solicitar nueva frase
            string nuevaFrase = AnsiConsole.Prompt(
                new TextPrompt<string>($"[bold]Frase:[/] [dim]({usuario.Frase})[/] ")
                    .PromptStyle("green")
                    .AllowEmpty());
            
            if (string.IsNullOrWhiteSpace(nuevaFrase))
            {
                nuevaFrase = usuario.Frase;
            }
            
            // Obtener ciudades
            var ciudades = new Dictionary<string, int>();
            string ciudadActual = "";
            
            using (var conn = _dbFactory.CreateConnection())
            {
                conn.Open();
                var cmd = new MySqlCommand("SELECT id, nombre FROM ciudades", (MySqlConnection)conn);
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        int id = Convert.ToInt32(reader["id"]);
                        string nombre = reader["nombre"].ToString();
                        ciudades.Add(nombre, id);
                        
                        if (id == usuario.CiudadId)
                        {
                            ciudadActual = nombre;
                        }
                    }
                }
            }
            
            // Seleccionar nueva ciudad
            opciones = new List<string> { $"<Mantener actual: {ciudadActual}>" };
            opciones.AddRange(ciudades.Keys);
            
            var ciudadSeleccionada = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title($"[bold]Ciudad:[/]")
                    .PageSize(10)
                    .HighlightStyle(new Style(foreground: Color.Green))
                    .AddChoices(opciones));
            
            int nuevaCiudadId = usuario.CiudadId;
            if (ciudadSeleccionada != $"<Mantener actual: {ciudadActual}>")
            {
                nuevaCiudadId = ciudades[ciudadSeleccionada];
            }
            
            // Solicitar nuevo máximo de likes diarios
            string likesInput = AnsiConsole.Prompt(
                new TextPrompt<string>($"[bold]Máximo de likes diarios:[/] [dim]({usuario.MaxLikesDiarios})[/] ")
                    .PromptStyle("green")
                    .AllowEmpty());
            
            int nuevoMaxLikes = usuario.MaxLikesDiarios;
            if (!string.IsNullOrWhiteSpace(likesInput) && int.TryParse(likesInput, out int likes))
            {
                if (likes > 0)
                {
                    nuevoMaxLikes = likes;
                }
                else
                {
                    AnsiConsole.MarkupLine("[yellow]Valor no válido. Se mantendrá el valor actual.[/]");
                }
            }
            
            // Solicitar nuevo correo (opcional)
            string nuevoCorreo = AnsiConsole.Prompt(
                new TextPrompt<string>($"[bold]Correo:[/] [dim]({correoActual})[/] ")
                    .PromptStyle("green")
                    .AllowEmpty());
            
            if (string.IsNullOrWhiteSpace(nuevoCorreo))
            {
                nuevoCorreo = correoActual;
            }
            else if (!nuevoCorreo.Contains('@'))
            {
                AnsiConsole.MarkupLine("[yellow]Correo no válido. Se mantendrá el valor actual.[/]");
                nuevoCorreo = correoActual;
            }
            
            // Solicitar nueva contraseña (opcional)
            string nuevaContrasena = AnsiConsole.Prompt(
                new TextPrompt<string>("[bold]Nueva contraseña:[/] [dim](dejar en blanco para no cambiar)[/] ")
                    .PromptStyle("green")
                    .Secret()
                    .AllowEmpty());
            
            // Cambiar estado de administrador
            bool nuevoEsAdmin = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title($"[bold]¿Es administrador?[/] [dim]({(esAdminActual ? "Sí" : "No")})[/]")
                    .PageSize(3)
                    .HighlightStyle(new Style(foreground: Color.Green))
                    .AddChoices(new[] { "No", "Sí" })) == "Sí";
            
            // Confirmar edición
            var confirmar = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[bold]¿Desea guardar los cambios?[/]")
                    .PageSize(3)
                    .HighlightStyle(new Style(foreground: Color.Green))
                    .AddChoices(new[] { "Cancelar", "Guardar" }));
            
            if (confirmar == "Guardar")
            {
                // Mostrar spinner mientras se procesa
                var resultado = false;
                await AnsiConsole.Status()
                    .StartAsync("Actualizando usuario...", async ctx => 
                    {
                        // Actualizar usuario
                        var userResult = await _adminService.EditarUsuario(
                            usuarioId, nuevoNombre, nuevaEdad, nuevoGeneroId, 
                            nuevosIntereses, nuevaCarrera, nuevaFrase, 
                            nuevaCiudadId, nuevoMaxLikes);
                        
                        // Actualizar login si se cambió el correo o la contraseña
                        var loginResult = true;
                        if (nuevoCorreo != correoActual || !string.IsNullOrEmpty(nuevaContrasena))
                        {
                            // Implementar método para actualizar login en AdminService
                            using (var conn = _dbFactory.CreateConnection())
                            {
                                conn.Open();
                                using (var cmd = new MySqlCommand())
                                {
                                    cmd.Connection = (MySqlConnection)conn;
                                    
                                    if (!string.IsNullOrEmpty(nuevaContrasena))
                                    {
                                        cmd.CommandText = "UPDATE login SET correo = @correo, contrasena = @contrasena WHERE usuario_id = @usuarioId";
                                        cmd.Parameters.AddWithValue("@correo", nuevoCorreo);
                                        cmd.Parameters.AddWithValue("@contrasena", nuevaContrasena);
                                    }
                                    else
                                    {
                                        cmd.CommandText = "UPDATE login SET correo = @correo WHERE usuario_id = @usuarioId";
                                        cmd.Parameters.AddWithValue("@correo", nuevoCorreo);
                                    }
                                    
                                    cmd.Parameters.AddWithValue("@usuarioId", usuarioId);
                                    
                                    try
                                    {
                                        await cmd.ExecuteNonQueryAsync();
                                    }
                                    catch
                                    {
                                        loginResult = false;
                                    }
                                }
                            }
                        }
                        
                        // Actualizar estado de administrador si cambió
                        var adminResult = true;
                        if (nuevoEsAdmin != esAdminActual)
                        {
                            adminResult = await _adminService.CambiarEstadoAdmin(usuarioId, nuevoEsAdmin);
                        }
                        
                        resultado = userResult && loginResult && adminResult;
                    });
                
                if (resultado)
                {
                    AnsiConsole.MarkupLine("\n[green]Usuario actualizado exitosamente.[/]");
                }
                else
                {
                    AnsiConsole.MarkupLine("\n[red]Error al actualizar el usuario. Es posible que el correo ya esté en uso.[/]");
                }
            }
            else
            {
                AnsiConsole.MarkupLine("\n[yellow]Operación cancelada.[/]");
            }
            
            AnsiConsole.WriteLine("\nPresione cualquier tecla para continuar.");
            Console.ReadKey();
        }

        /// <summary>
        /// Interfaz de usuario para eliminar un usuario
        /// </summary>
        public async Task EliminarUsuario()
        {
            Console.Clear();
            
            // Crear título con Spectre.Console
            AnsiConsole.Write(
                new FigletText("Eliminar Usuario")
                    .Centered()
                    .Color(Color.Red));
            
            AnsiConsole.WriteLine();
            
            // Obtener lista de usuarios
            var usuarios = new Dictionary<string, int>();
            
            using (var conn = _dbFactory.CreateConnection())
            {
                conn.Open();
                var cmd = new MySqlCommand(
                    "SELECT u.id, u.nombre FROM usuarios u ORDER BY u.nombre", 
                    (MySqlConnection)conn);
                
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        usuarios.Add(
                            $"{reader["nombre"]} (ID: {reader["id"]})",
                            Convert.ToInt32(reader["id"])
                        );
                    }
                }
            }
            
            if (usuarios.Count == 0)
            {
                AnsiConsole.MarkupLine("[red]No hay usuarios registrados.[/]");
                AnsiConsole.WriteLine("\nPresione cualquier tecla para continuar.");
                Console.ReadKey();
                return;
            }
            
            // Seleccionar usuario a eliminar
            var usuarioSeleccionado = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[bold]Seleccione el usuario a eliminar:[/]")
                    .PageSize(10)
                    .HighlightStyle(new Style(foreground: Color.Red))
                    .AddChoices(usuarios.Keys));
            
            int usuarioId = usuarios[usuarioSeleccionado];
            
            // Mostrar advertencia y confirmar eliminación
            AnsiConsole.MarkupLine("\n[bold red]ADVERTENCIA:[/] Esta acción eliminará permanentemente el usuario seleccionado.");
            AnsiConsole.MarkupLine("Se eliminarán también todas sus interacciones, coincidencias y estadísticas asociadas.");
            
            var confirmar = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[bold]¿Está seguro de que desea eliminar este usuario?[/]")
                    .PageSize(3)
                    .HighlightStyle(new Style(foreground: Color.Red))
                    .AddChoices(new[] { "Cancelar", "Eliminar" }));
            
            if (confirmar == "Eliminar")
            {
                // Requiere doble confirmación para mayor seguridad
                var confirmacionFinal = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[bold red]¡Esta acción no se puede deshacer![/] ¿Confirmar eliminación?")
                        .PageSize(3)
                        .HighlightStyle(new Style(foreground: Color.Red))
                        .AddChoices(new[] { "Cancelar", "Confirmar eliminación" }));
                
                if (confirmacionFinal == "Confirmar eliminación")
                {
                    // Mostrar spinner mientras se procesa
                    var resultado = false;
                    await AnsiConsole.Status()
                        .StartAsync("Eliminando usuario...", async ctx => 
                        {
                            resultado = await _adminService.EliminarUsuario(usuarioId);
                        });
                    
                    if (resultado)
                    {
                        AnsiConsole.MarkupLine("\n[green]Usuario eliminado exitosamente.[/]");
                    }
                    else
                    {
                        AnsiConsole.MarkupLine("\n[red]Error al eliminar el usuario. Es posible que el usuario tenga registros relacionados que no pueden ser eliminados.[/]");
                    }
                }
                else
                {
                    AnsiConsole.MarkupLine("\n[yellow]Operación cancelada.[/]");
                }
            }
            else
            {
                AnsiConsole.MarkupLine("\n[yellow]Operación cancelada.[/]");
            }
            
            AnsiConsole.WriteLine("\nPresione cualquier tecla para continuar.");
            Console.ReadKey();
        }

        /// <summary>
        /// Interfaz de usuario para ver detalles de un usuario
        /// </summary>
        public async Task VerDetallesUsuario()
        {
            Console.Clear();
            
            // Crear título con Spectre.Console
            AnsiConsole.Write(
                new FigletText("Detalles de Usuario")
                    .Centered()
                    .Color(Color.Blue));
            
            AnsiConsole.WriteLine();
            
            // Obtener lista de usuarios
            var usuarios = new Dictionary<string, int>();
            
            using (var conn = _dbFactory.CreateConnection())
            {
                conn.Open();
                var cmd = new MySqlCommand(
                    "SELECT u.id, u.nombre FROM usuarios u ORDER BY u.nombre", 
                    (MySqlConnection)conn);
                
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        usuarios.Add(
                            $"{reader["nombre"]} (ID: {reader["id"]})",
                            Convert.ToInt32(reader["id"])
                        );
                    }
                }
            }
            
            if (usuarios.Count == 0)
            {
                AnsiConsole.MarkupLine("[red]No hay usuarios registrados.[/]");
                AnsiConsole.WriteLine("\nPresione cualquier tecla para continuar.");
                Console.ReadKey();
                return;
            }
            
            // Seleccionar usuario para ver detalles
            var usuarioSeleccionado = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[bold]Seleccione un usuario para ver sus detalles:[/]")
                    .PageSize(10)
                    .HighlightStyle(new Style(foreground: Color.Blue))
                    .AddChoices(usuarios.Keys));
            
            int usuarioId = usuarios[usuarioSeleccionado];
            
            // Obtener detalles completos del usuario seleccionado
            await AnsiConsole.Status()
                .StartAsync("Cargando detalles del usuario...", async ctx => 
                {
                    var usuario = await _adminService.ObtenerDetallesUsuario(usuarioId);
                    
                    if (usuario != null)
                    {
                        ctx.Status("Obteniendo información adicional...");
                        
                        // Obtener información del género
                        string generoNombre = "No especificado";
                        using (var conn = _dbFactory.CreateConnection())
                        {
                            conn.Open();
                            var cmd = new MySqlCommand(
                                "SELECT descripcion FROM generos WHERE id = @generoId", 
                                (MySqlConnection)conn);
                            cmd.Parameters.AddWithValue("@generoId", usuario.GeneroId);
                            
                            var result = await cmd.ExecuteScalarAsync();
                            if (result != null && result != DBNull.Value)
                            {
                                generoNombre = result.ToString();
                            }
                        }
                        
                        // Obtener información de la ciudad
                        string ciudadNombre = "No especificada";
                        using (var conn = _dbFactory.CreateConnection())
                        {
                            conn.Open();
                            var cmd = new MySqlCommand(
                                "SELECT nombre FROM ciudades WHERE id = @ciudadId", 
                                (MySqlConnection)conn);
                            cmd.Parameters.AddWithValue("@ciudadId", usuario.CiudadId);
                            
                            var result = await cmd.ExecuteScalarAsync();
                            if (result != null && result != DBNull.Value)
                            {
                                ciudadNombre = result.ToString();
                            }
                        }
                        
                        // Obtener información de coincidencias
                        int coincidencias = 0;
                        using (var conn = _dbFactory.CreateConnection())
                        {
                            conn.Open();
                            var cmd = new MySqlCommand(
                                "SELECT COUNT(*) FROM coincidencias WHERE usuario1_id = @usuarioId OR usuario2_id = @usuarioId", 
                                (MySqlConnection)conn);
                            cmd.Parameters.AddWithValue("@usuarioId", usuarioId);
                            
                            coincidencias = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                        }
                        
                        // Obtener información de interacciones dadas
                        int interaccionesDadas = 0;
                        int likesDados = 0;
                        using (var conn = _dbFactory.CreateConnection())
                        {
                            conn.Open();
                            var cmd = new MySqlCommand(
                                "SELECT COUNT(*) FROM interacciones WHERE usuario_id = @usuarioId", 
                                (MySqlConnection)conn);
                            cmd.Parameters.AddWithValue("@usuarioId", usuarioId);
                            
                            interaccionesDadas = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                            
                            // Likes dados
                            cmd = new MySqlCommand(
                                "SELECT COUNT(*) FROM interacciones WHERE usuario_id = @usuarioId AND le_gusto = 1", 
                                (MySqlConnection)conn);
                            cmd.Parameters.AddWithValue("@usuarioId", usuarioId);
                            
                            likesDados = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                        }
                        
                        // Obtener información de interacciones recibidas
                        int interaccionesRecibidas = 0;
                        int likesRecibidos = 0;
                        using (var conn = _dbFactory.CreateConnection())
                        {
                            conn.Open();
                            var cmd = new MySqlCommand(
                                "SELECT COUNT(*) FROM interacciones WHERE objetivo_usuario_id = @usuarioId", 
                                (MySqlConnection)conn);
                            cmd.Parameters.AddWithValue("@usuarioId", usuarioId);
                            
                            interaccionesRecibidas = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                            
                            // Likes recibidos
                            cmd = new MySqlCommand(
                                "SELECT COUNT(*) FROM interacciones WHERE objetivo_usuario_id = @usuarioId AND le_gusto = 1", 
                                (MySqlConnection)conn);
                            cmd.Parameters.AddWithValue("@usuarioId", usuarioId);
                            
                            likesRecibidos = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                        }
                        
                        // La información de login ya está cargada con el usuario
                        string correo = usuario.Login?.Correo ?? "No disponible";
                        bool esAdmin = usuario.Login?.EsAdmin ?? false;
                        
                        ctx.Status("Mostrando resultados...");
                        Console.Clear();
                        
                        // Mostrar título nuevamente después de limpiar pantalla
                        AnsiConsole.Write(
                            new FigletText("Detalles de Usuario")
                                .Centered()
                                .Color(Color.Blue));
                        
                        AnsiConsole.WriteLine();
                        
                        // Panel con información de identidad
                        var panelIdentidad = new Panel($"[bold]ID:[/] {usuario.Id} | [bold]Nombre:[/] {usuario.Nombre} | [bold]Edad:[/] {usuario.Edad}");
                        panelIdentidad.Border = BoxBorder.Rounded;
                        panelIdentidad.BorderColor(Color.Blue);
                        panelIdentidad.Header = new PanelHeader("Información Personal");
                        panelIdentidad.Expand();
                        AnsiConsole.Write(panelIdentidad);
                        
                        // Panel con frase e intereses
                        var panelPerfil = new Panel($"[bold]Frase:[/] \"{usuario.Frase}\"\n\n[bold]Carrera:[/] {usuario.Carrera}\n\n[bold]Intereses:[/] {usuario.Intereses}");
                        panelPerfil.Border = BoxBorder.Rounded;
                        panelPerfil.BorderColor(Color.Green);
                        panelPerfil.Header = new PanelHeader("Perfil");
                        panelPerfil.Expand();
                        AnsiConsole.Write(panelPerfil);
                        
                        // Grid para mostrar datos adicionales
                        var grid = new Grid();
                        grid.AddColumn(new GridColumn());
                        grid.AddColumn(new GridColumn());
                        
                        // Primer panel (Localización y Demografía)
                        var panelDemografia = new Panel(
                            $"[bold]Género:[/] {generoNombre}\n" +
                            $"[bold]Ciudad:[/] {ciudadNombre}\n");
                        panelDemografia.Border = BoxBorder.Rounded;
                        panelDemografia.BorderColor(Color.Yellow);
                        panelDemografia.Header = new PanelHeader("Demografía");
                        
                        // Segundo panel (Cuenta y Preferencias)
                        var panelCuenta = new Panel(
                            $"[bold]Correo:[/] {correo}\n" +
                            $"[bold]Administrador:[/] {(esAdmin ? "[green]Sí[/]" : "[red]No[/]")}\n" +
                            $"[bold]Límite diario de likes:[/] {usuario.MaxLikesDiarios}\n" +
                            $"[bold]Likes restantes hoy:[/] {usuario.MaxLikesDiarios - usuario.LikesDiarios}");
                        panelCuenta.Border = BoxBorder.Rounded;
                        panelCuenta.BorderColor(Color.Purple);
                        panelCuenta.Header = new PanelHeader("Cuenta");
                        
                        grid.AddRow(panelDemografia, panelCuenta);
                        AnsiConsole.Write(grid);
                        
                        // Panel de estadísticas
                        var panelEstadisticas = new Panel(
                            $"[bold]Likes dados:[/] {likesDados} de {interaccionesDadas} interacciones ({(interaccionesDadas > 0 ? (likesDados * 100.0 / interaccionesDadas).ToString("0.0") : "0")}%)\n" +
                            $"[bold]Likes recibidos:[/] {likesRecibidos} de {interaccionesRecibidas} interacciones ({(interaccionesRecibidas > 0 ? (likesRecibidos * 100.0 / interaccionesRecibidas).ToString("0.0") : "0")}%)\n" +
                            $"[bold]Coincidencias totales:[/] {coincidencias}\n" +
                            $"[bold]Tasa de éxito:[/] {(likesDados > 0 ? (coincidencias * 100.0 / likesDados).ToString("0.0") : "0")}%");
                        panelEstadisticas.Border = BoxBorder.Rounded;
                        panelEstadisticas.BorderColor(Color.Red);
                        panelEstadisticas.Header = new PanelHeader("Estadísticas");
                        panelEstadisticas.Expand();
                        AnsiConsole.Write(panelEstadisticas);
                    }
                    else
                    {
                        AnsiConsole.MarkupLine("[red]No se pudo encontrar la información del usuario.[/]");
                    }
                });
            
            AnsiConsole.WriteLine("\nPresione cualquier tecla para continuar.");
            Console.ReadKey();
        }
        
        #endregion

        #region Métodos CRUD para Interacciones
        
        /// <summary>
        /// Interfaz de usuario para añadir una interacción
        /// </summary>
        public async Task AñadirInteraccion()
        {
            await MostrarMensajeEnConstruccion("Añadir Interacción");
        }
        
        /// <summary>
        /// Interfaz de usuario para eliminar una interacción
        /// </summary>
        public async Task EliminarInteraccion()
        {
            await MostrarMensajeEnConstruccion("Eliminar Interacción");
        }
        
        /// <summary>
        /// Interfaz de usuario para ver más interacciones
        /// </summary>
        public async Task VerMasInteracciones()
        {
            await MostrarMensajeEnConstruccion("Ver Más Interacciones");
        }
        
        #endregion

        #region Métodos CRUD para Coincidencias
        
        /// <summary>
        /// Interfaz de usuario para eliminar una coincidencia
        /// </summary>
        public async Task EliminarCoincidencia()
        {
            await MostrarMensajeEnConstruccion("Eliminar Coincidencia");
        }
        
        #endregion

        #region Métodos CRUD para Ciudades
        
        /// <summary>
        /// Interfaz de usuario para añadir una ciudad
        /// </summary>
        public async Task AñadirCiudad()
        {
            await MostrarMensajeEnConstruccion("Añadir Ciudad");
        }
        
        /// <summary>
        /// Interfaz de usuario para editar una ciudad
        /// </summary>
        public async Task EditarCiudad()
        {
            await MostrarMensajeEnConstruccion("Editar Ciudad");
        }
        
        /// <summary>
        /// Interfaz de usuario para eliminar una ciudad
        /// </summary>
        public async Task EliminarCiudad()
        {
            await MostrarMensajeEnConstruccion("Eliminar Ciudad");
        }
        
        #endregion

        #region Métodos CRUD para Departamentos
        
        /// <summary>
        /// Interfaz de usuario para añadir un departamento
        /// </summary>
        public async Task AñadirDepartamento()
        {
            await MostrarMensajeEnConstruccion("Añadir Departamento");
        }
        
        /// <summary>
        /// Interfaz de usuario para editar un departamento
        /// </summary>
        public async Task EditarDepartamento()
        {
            await MostrarMensajeEnConstruccion("Editar Departamento");
        }
        
        /// <summary>
        /// Interfaz de usuario para eliminar un departamento
        /// </summary>
        public async Task EliminarDepartamento()
        {
            await MostrarMensajeEnConstruccion("Eliminar Departamento");
        }
        
        #endregion

        #region Métodos CRUD para Géneros
        
        /// <summary>
        /// Interfaz de usuario para añadir un género
        /// </summary>
        public async Task AñadirGenero()
        {
            Console.Clear();
            
            // Crear título con Spectre.Console
            AnsiConsole.Write(
                new FigletText("Añadir Género")
                    .Centered()
                    .Color(Color.Green));
            
            AnsiConsole.WriteLine();
            
            // Mostrar géneros existentes
            AnsiConsole.MarkupLine("[bold]Géneros existentes:[/]");
            var table = new Table();
            table.Border = TableBorder.Rounded;
            
            // Añadir columnas
            table.AddColumn(new TableColumn("[b]ID[/]").Centered());
            table.AddColumn(new TableColumn("[b]Descripción[/]").Centered());
            
            // Obtener datos de géneros
            using (var conn = _dbFactory.CreateConnection())
            {
                conn.Open();
                var cmd = new MySqlCommand(
                    "SELECT id, descripcion FROM generos ORDER BY id", 
                    (MySqlConnection)conn);
                
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        table.AddRow(
                            reader["id"].ToString(),
                            reader["descripcion"].ToString()
                        );
                    }
                }
            }
            
            AnsiConsole.Write(table);
            AnsiConsole.WriteLine();
            
            // Solicitar descripción del nuevo género
            string descripcion = AnsiConsole.Prompt(
                new TextPrompt<string>("[bold]Descripción del nuevo género:[/] ")
                    .PromptStyle("green")
                    .ValidationErrorMessage("[red]La descripción debe tener al menos 3 caracteres[/]")
                    .Validate(desc => 
                    {
                        return !string.IsNullOrWhiteSpace(desc) && desc.Length >= 3 
                            ? ValidationResult.Success() 
                            : ValidationResult.Error();
                    }));
            
            // Confirmar creación
            var confirmar = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[bold]¿Desea crear este género?[/]")
                    .PageSize(3)
                    .HighlightStyle(new Style(foreground: Color.Green))
                    .AddChoices(new[] { "Cancelar", "Crear" }));
            
            if (confirmar == "Crear")
            {
                // Mostrar spinner mientras se procesa
                var resultado = false;
                await AnsiConsole.Status()
                    .StartAsync("Creando género...", async ctx => 
                    {
                        resultado = await _adminService.AñadirGenero(descripcion);
                    });
                
                if (resultado)
                {
                    AnsiConsole.MarkupLine("\n[green]Género creado exitosamente.[/]");
                }
                else
                {
                    AnsiConsole.MarkupLine("\n[red]Error al crear el género. Es posible que ya exista uno similar.[/]");
                }
            }
            else
            {
                AnsiConsole.MarkupLine("\n[yellow]Operación cancelada.[/]");
            }
            
            AnsiConsole.WriteLine("\nPresione cualquier tecla para continuar.");
            Console.ReadKey();
        }
        
        /// <summary>
        /// Interfaz de usuario para editar un género
        /// </summary>
        public async Task EditarGenero()
        {
            Console.Clear();
            
            // Crear título con Spectre.Console
            AnsiConsole.Write(
                new FigletText("Editar Género")
                    .Centered()
                    .Color(Color.Yellow));
            
            AnsiConsole.WriteLine();
            
            // Obtener los géneros disponibles
            var generos = new Dictionary<string, (int id, string descripcion)>();
            
            using (var conn = _dbFactory.CreateConnection())
            {
                conn.Open();
                var cmd = new MySqlCommand(
                    "SELECT id, descripcion FROM generos ORDER BY id", 
                    (MySqlConnection)conn);
                
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        int id = Convert.ToInt32(reader["id"]);
                        string descripcion = reader["descripcion"].ToString();
                        generos.Add(
                            $"{descripcion} (ID: {id})",
                            (id, descripcion)
                        );
                    }
                }
            }
            
            if (generos.Count == 0)
            {
                AnsiConsole.MarkupLine("[red]No hay géneros registrados.[/]");
                AnsiConsole.WriteLine("\nPresione cualquier tecla para continuar.");
                Console.ReadKey();
                return;
            }
            
            // Mostrar géneros actuales
            AnsiConsole.MarkupLine("[bold]Géneros existentes:[/]");
            var table = new Table();
            table.Border = TableBorder.Rounded;
            
            // Añadir columnas
            table.AddColumn(new TableColumn("[b]ID[/]").Centered());
            table.AddColumn(new TableColumn("[b]Descripción[/]").Centered());
            
            // Agregar filas a la tabla
            foreach (var genero in generos.Values)
            {
                table.AddRow(
                    genero.id.ToString(),
                    genero.descripcion
                );
            }
            
            AnsiConsole.Write(table);
            AnsiConsole.WriteLine();
            
            // Seleccionar género a editar
            var generoSeleccionado = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[bold]Seleccione el género a editar:[/]")
                    .PageSize(10)
                    .HighlightStyle(new Style(foreground: Color.Yellow))
                    .AddChoices(generos.Keys));
            
            var (generoId, descripcionActual) = generos[generoSeleccionado];
            
            // Solicitar nueva descripción
            string nuevaDescripcion = AnsiConsole.Prompt(
                new TextPrompt<string>($"[bold]Nueva descripción:[/] [dim]({descripcionActual})[/] ")
                    .PromptStyle("green")
                    .ValidationErrorMessage("[red]La descripción debe tener al menos 3 caracteres[/]")
                    .Validate(desc => 
                    {
                        return !string.IsNullOrWhiteSpace(desc) && desc.Length >= 3 
                            ? ValidationResult.Success() 
                            : ValidationResult.Error();
                    }));
            
            // Confirmar edición
            var confirmar = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[bold]¿Desea guardar los cambios?[/]")
                    .PageSize(3)
                    .HighlightStyle(new Style(foreground: Color.Yellow))
                    .AddChoices(new[] { "Cancelar", "Guardar" }));
            
            if (confirmar == "Guardar")
            {
                // Mostrar spinner mientras se procesa
                var resultado = false;
                await AnsiConsole.Status()
                    .StartAsync("Actualizando género...", async ctx => 
                    {
                        resultado = await _adminService.EditarGenero(generoId, nuevaDescripcion);
                    });
                
                if (resultado)
                {
                    AnsiConsole.MarkupLine("\n[green]Género actualizado exitosamente.[/]");
                }
                else
                {
                    AnsiConsole.MarkupLine("\n[red]Error al actualizar el género. Es posible que no exista o que ya haya otro género con la misma descripción.[/]");
                }
            }
            else
            {
                AnsiConsole.MarkupLine("\n[yellow]Operación cancelada.[/]");
            }
            
            AnsiConsole.WriteLine("\nPresione cualquier tecla para continuar.");
            Console.ReadKey();
        }
        
        /// <summary>
        /// Interfaz de usuario para eliminar un género
        /// </summary>
        public async Task EliminarGenero()
        {
            Console.Clear();
            
            // Crear título con Spectre.Console
            AnsiConsole.Write(
                new FigletText("Eliminar Género")
                    .Centered()
                    .Color(Color.Red));
            
            AnsiConsole.WriteLine();
            
            // Obtener géneros disponibles
            var generos = new Dictionary<string, int>();
            
            using (var conn = _dbFactory.CreateConnection())
            {
                conn.Open();
                var cmd = new MySqlCommand(
                    "SELECT id, descripcion FROM generos ORDER BY id", 
                    (MySqlConnection)conn);
                
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        generos.Add(
                            $"{reader["descripcion"]} (ID: {reader["id"]})",
                            Convert.ToInt32(reader["id"])
                        );
                    }
                }
            }
            
            if (generos.Count == 0)
            {
                AnsiConsole.MarkupLine("[red]No hay géneros registrados.[/]");
                AnsiConsole.WriteLine("\nPresione cualquier tecla para continuar.");
                Console.ReadKey();
                return;
            }
            
            // Mostrar géneros y verificar si hay usuarios asociados a ellos
            AnsiConsole.MarkupLine("[bold]Géneros existentes y su uso:[/]");
            var table = new Table();
            table.Border = TableBorder.Rounded;
            
            // Añadir columnas
            table.AddColumn(new TableColumn("[b]ID[/]").Centered());
            table.AddColumn(new TableColumn("[b]Descripción[/]"));
            table.AddColumn(new TableColumn("[b]Usuarios asociados[/]").Centered());
            
            // Diccionario para guardar relación entre género y usuarios
            var usuariosAsociados = new Dictionary<int, int>();
            
            using (var conn = _dbFactory.CreateConnection())
            {
                conn.Open();
                
                // Contar usuarios por género
                foreach (var genero in generos)
                {
                    int id = genero.Value;
                    
                    var cmdCount = new MySqlCommand(
                        "SELECT COUNT(*) FROM usuarios WHERE genero = @generoId", 
                        (MySqlConnection)conn);
                    cmdCount.Parameters.AddWithValue("@generoId", id);
                    
                    int count = Convert.ToInt32(await cmdCount.ExecuteScalarAsync());
                    usuariosAsociados[id] = count;
                }
                
                // Mostrar datos en la tabla
                var cmdGeneros = new MySqlCommand(
                    "SELECT id, descripcion FROM generos ORDER BY id", 
                    (MySqlConnection)conn);
                
                using (var reader = await cmdGeneros.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        int id = Convert.ToInt32(reader["id"]);
                        string descripcion = reader["descripcion"].ToString();
                        int count = usuariosAsociados.ContainsKey(id) ? usuariosAsociados[id] : 0;
                        
                        string countText = count == 0 
                            ? "[green]0[/]" 
                            : $"[red]{count}[/]";
                        
                        table.AddRow(
                            id.ToString(),
                            descripcion,
                            countText
                        );
                    }
                }
            }
            
            AnsiConsole.Write(table);
            AnsiConsole.WriteLine();
            
            // Seleccionar género a eliminar
            var generoSeleccionado = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[bold]Seleccione el género a eliminar:[/]")
                    .PageSize(10)
                    .HighlightStyle(new Style(foreground: Color.Red))
                    .AddChoices(generos.Keys));
            
            int generoId = generos[generoSeleccionado];
            
            // Verificar si hay usuarios asociados al género
            if (usuariosAsociados.ContainsKey(generoId) && usuariosAsociados[generoId] > 0)
            {
                AnsiConsole.MarkupLine($"\n[red]No se puede eliminar este género porque tiene {usuariosAsociados[generoId]} usuarios asociados.[/]");
                AnsiConsole.MarkupLine("Primero debe reasignar estos usuarios a un género diferente.");
                AnsiConsole.WriteLine("\nPresione cualquier tecla para continuar.");
                Console.ReadKey();
                return;
            }
            
            // Mostrar advertencia y confirmar eliminación
            var confirmar = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[bold]¿Está seguro de que desea eliminar este género?[/]")
                    .PageSize(3)
                    .HighlightStyle(new Style(foreground: Color.Red))
                    .AddChoices(new[] { "Cancelar", "Eliminar" }));
            
            if (confirmar == "Eliminar")
            {
                // Mostrar spinner mientras se procesa
                var resultado = false;
                await AnsiConsole.Status()
                    .StartAsync("Eliminando género...", async ctx => 
                    {
                        resultado = await _adminService.EliminarGenero(generoId);
                    });
                
                if (resultado)
                {
                    AnsiConsole.MarkupLine("\n[green]Género eliminado exitosamente.[/]");
                }
                else
                {
                    AnsiConsole.MarkupLine("\n[red]Error al eliminar el género. Es posible que tenga registros relacionados o que no exista.[/]");
                }
            }
            else
            {
                AnsiConsole.MarkupLine("\n[yellow]Operación cancelada.[/]");
            }
            
            AnsiConsole.WriteLine("\nPresione cualquier tecla para continuar.");
            Console.ReadKey();
        }
        
        #endregion
    }
}
