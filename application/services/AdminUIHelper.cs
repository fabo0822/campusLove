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
            Console.Clear();
            
            // Crear título con Spectre.Console
            AnsiConsole.Write(
                new FigletText("Añadir Interacción")
                    .Centered()
                    .Color(Color.Green));
            
            AnsiConsole.WriteLine();
            
            // Obtener lista de usuarios emisores
            AnsiConsole.MarkupLine("[bold]Seleccione el usuario emisor:[/]");
            var usuariosEmisores = new Dictionary<string, int>();
            
            using (var conn = _dbFactory.CreateConnection())
            {
                conn.Open();
                var cmd = new MySqlCommand(
                    "SELECT id, nombre FROM usuarios ORDER BY nombre", 
                    (MySqlConnection)conn);
                
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        usuariosEmisores.Add(
                            $"{reader["nombre"]} (ID: {reader["id"]})",
                            Convert.ToInt32(reader["id"])
                        );
                    }
                }
            }
            
            if (usuariosEmisores.Count == 0)
            {
                AnsiConsole.MarkupLine("[red]No hay usuarios registrados para crear interacciones.[/]");
                AnsiConsole.WriteLine("\nPresione cualquier tecla para continuar.");
                Console.ReadKey();
                return;
            }
            
            // Seleccionar usuario emisor
            var usuarioEmisorSeleccionado = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[bold]Usuario que envía la interacción:[/]")
                    .PageSize(10)
                    .HighlightStyle(new Style(foreground: Color.Green))
                    .AddChoices(usuariosEmisores.Keys));
            
            int usuarioEmisorId = usuariosEmisores[usuarioEmisorSeleccionado];
            
            // Obtener lista de usuarios receptores (excluyendo al emisor)
            AnsiConsole.MarkupLine("\n[bold]Seleccione el usuario receptor:[/]");
            var usuariosReceptores = new Dictionary<string, int>();
            
            using (var conn = _dbFactory.CreateConnection())
            {
                conn.Open();
                var cmd = new MySqlCommand(
                    "SELECT id, nombre FROM usuarios WHERE id != @emisorId ORDER BY nombre", 
                    (MySqlConnection)conn);
                cmd.Parameters.AddWithValue("@emisorId", usuarioEmisorId);
                
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        usuariosReceptores.Add(
                            $"{reader["nombre"]} (ID: {reader["id"]})",
                            Convert.ToInt32(reader["id"])
                        );
                    }
                }
            }
            
            if (usuariosReceptores.Count == 0)
            {
                AnsiConsole.MarkupLine("[red]No hay otros usuarios disponibles para recibir la interacción.[/]");
                AnsiConsole.WriteLine("\nPresione cualquier tecla para continuar.");
                Console.ReadKey();
                return;
            }
            
            // Seleccionar usuario receptor
            var usuarioReceptorSeleccionado = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[bold]Usuario que recibe la interacción:[/]")
                    .PageSize(10)
                    .HighlightStyle(new Style(foreground: Color.Green))
                    .AddChoices(usuariosReceptores.Keys));
            
            int usuarioReceptorId = usuariosReceptores[usuarioReceptorSeleccionado];
            
            // Seleccionar tipo de interacción (like/dislike)
            var tipoInteraccion = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[bold]Tipo de interacción:[/]")
                    .PageSize(3)
                    .HighlightStyle(new Style(foreground: Color.Green))
                    .AddChoices(new[] { "Like", "Dislike" }));
            
            bool esLike = tipoInteraccion == "Like";
            
            // Confirmar creación
            var confirmar = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title($"\n[bold]¿Confirmar la creación de esta interacción?[/]")
                    .PageSize(3)
                    .HighlightStyle(new Style(foreground: Color.Green))
                    .AddChoices(new[] { "Cancelar", "Confirmar" }));
            
            if (confirmar == "Confirmar")
            {
                // Mostrar spinner mientras se procesa
                var resultado = false;
                await AnsiConsole.Status()
                    .StartAsync("Creando interacción...", async ctx => 
                    {
                        resultado = await _adminService.AñadirInteraccion(usuarioEmisorId, usuarioReceptorId, esLike);
                    });
                
                if (resultado)
                {
                    AnsiConsole.MarkupLine("\n[green]Interacción creada exitosamente.[/]");
                    
                    // Verificar si esta interacción generó un match
                    bool esMatch = false;
                    using (var conn = _dbFactory.CreateConnection())
                    {
                        conn.Open();
                        var cmd = new MySqlCommand(
                            @"SELECT COUNT(*) FROM interacciones 
                            WHERE usuario_id = @receptorId 
                            AND objetivo_usuario_id = @emisorId 
                            AND le_gusto = 1", 
                            (MySqlConnection)conn);
                        cmd.Parameters.AddWithValue("@emisorId", usuarioEmisorId);
                        cmd.Parameters.AddWithValue("@receptorId", usuarioReceptorId);
                        
                        int count = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                        esMatch = esLike && count > 0;
                    }
                    
                    if (esMatch)
                    {
                        AnsiConsole.MarkupLine("\n[yellow]¡Esta interacción ha generado un [bold]MATCH[/] entre los usuarios![/]");
                    }
                }
                else
                {
                    AnsiConsole.MarkupLine("\n[red]Error al crear la interacción.[/]");
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
        /// Interfaz de usuario para eliminar una interacción
        /// </summary>
        public async Task EliminarInteraccion()
        {
            Console.Clear();
            
            // Crear título con Spectre.Console
            AnsiConsole.Write(
                new FigletText("Eliminar Interacción")
                    .Centered()
                    .Color(Color.Red));
            
            AnsiConsole.WriteLine();
            
            // Mostrar tabla de interacciones recientes
            AnsiConsole.MarkupLine("[bold]Interacciones recientes:[/]");
            var table = new Table();
            table.Border = TableBorder.Rounded;
            
            // Añadir columnas
            table.AddColumn(new TableColumn("[b]ID[/]").Centered());
            table.AddColumn(new TableColumn("[b]Usuario Emisor[/]"));
            table.AddColumn(new TableColumn("[b]Usuario Receptor[/]"));
            table.AddColumn(new TableColumn("[b]Tipo[/]").Centered());
            table.AddColumn(new TableColumn("[b]Fecha[/]").Centered());
            
            // Obtener datos de interacciones
            var interacciones = new Dictionary<string, int>();
            
            using (var conn = _dbFactory.CreateConnection())
            {
                conn.Open();
                var cmd = new MySqlCommand(
                    @"SELECT i.id, 
                    ue.nombre as emisor, 
                    ur.nombre as receptor, 
                    CASE WHEN i.le_gusto = 1 THEN 'Like' ELSE 'Dislike' END as tipo,
                    i.fecha_interaccion as fecha
                    FROM interacciones i 
                    JOIN usuarios ue ON i.usuario_id = ue.id 
                    JOIN usuarios ur ON i.objetivo_usuario_id = ur.id 
                    ORDER BY i.fecha_interaccion DESC LIMIT 15", 
                    (MySqlConnection)conn);
                
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        int id = Convert.ToInt32(reader["id"]);
                        string emisor = reader["emisor"].ToString();
                        string receptor = reader["receptor"].ToString();
                        string tipo = reader["tipo"].ToString();
                        DateTime fecha = Convert.ToDateTime(reader["fecha"]);
                        
                        table.AddRow(
                            id.ToString(),
                            emisor,
                            receptor,
                            tipo == "Like" ? "[green]Like[/]" : "[red]Dislike[/]",
                            fecha.ToString("yyyy-MM-dd HH:mm")
                        );
                        
                        interacciones.Add($"ID: {id} - {emisor} → {receptor} ({tipo})", id);
                    }
                }
            }
            
            AnsiConsole.Write(table);
            AnsiConsole.WriteLine();
            
            if (interacciones.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]No hay interacciones registradas.[/]");
                AnsiConsole.WriteLine("\nPresione cualquier tecla para continuar.");
                Console.ReadKey();
                return;
            }
            
            // Seleccionar interacción a eliminar
            var interaccionSeleccionada = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[bold]Seleccione la interacción que desea eliminar:[/]")
                    .PageSize(10)
                    .HighlightStyle(new Style(foreground: Color.Red))
                    .AddChoices(interacciones.Keys));
            
            int interaccionId = interacciones[interaccionSeleccionada];
            
            // Confirmar eliminación
            var confirmar = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[bold red]¿Está seguro de que desea eliminar esta interacción?[/]")
                    .PageSize(3)
                    .HighlightStyle(new Style(foreground: Color.Red))
                    .AddChoices(new[] { "Cancelar", "Eliminar" }));
            
            if (confirmar == "Eliminar")
            {
                // Mostrar spinner mientras se procesa
                var resultado = false;
                
                await AnsiConsole.Status()
                    .StartAsync("Eliminando interacción...", async ctx => 
                    {
                        resultado = await _adminService.EliminarInteraccion(interaccionId);
                    });
                
                if (resultado)
                {
                    AnsiConsole.MarkupLine("\n[green]Interacción eliminada exitosamente.[/]");
                }
                else
                {
                    AnsiConsole.MarkupLine("\n[red]Error al eliminar la interacción.[/]");
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
        /// Interfaz de usuario para ver más interacciones
        /// </summary>
        public async Task VerMasInteracciones()
        {
            Console.Clear();
            bool salir = false;
            int paginaActual = 0;
            const int elementosPorPagina = 20;
            
            while (!salir)
            {
                Console.Clear();
                
                // Crear título con Spectre.Console
                AnsiConsole.Write(
                    new FigletText("Interacciones")
                        .Centered()
                        .Color(Color.Blue));
                
                AnsiConsole.WriteLine();
                
                // Mostrar tabla de interacciones con paginación
                AnsiConsole.MarkupLine($"[bold]Interacciones (Página {paginaActual + 1})[/]");
                var table = new Table();
                table.Border = TableBorder.Rounded;
                
                // Añadir columnas
                table.AddColumn(new TableColumn("[b]ID[/]").Centered());
                table.AddColumn(new TableColumn("[b]Usuario Emisor[/]"));
                table.AddColumn(new TableColumn("[b]Usuario Receptor[/]"));
                table.AddColumn(new TableColumn("[b]Tipo[/]").Centered());
                table.AddColumn(new TableColumn("[b]Fecha[/]").Centered());
                
                // Obtener datos de interacciones para la página actual
                int totalInteracciones = 0;
                
                using (var conn = _dbFactory.CreateConnection())
                {
                    conn.Open();
                    
                    // Primero contar el total de interacciones para calcular el número total de páginas
                    var cmdCount = new MySqlCommand(
                        "SELECT COUNT(*) FROM interacciones", 
                        (MySqlConnection)conn);
                    
                    totalInteracciones = Convert.ToInt32(await cmdCount.ExecuteScalarAsync());
                    
                    // Obtener las interacciones para la página actual
                    var cmd = new MySqlCommand(
                        @"SELECT i.id, 
                        ue.nombre as emisor, 
                        ur.nombre as receptor, 
                        CASE WHEN i.le_gusto = 1 THEN 'Like' ELSE 'Dislike' END as tipo,
                        i.fecha_interaccion as fecha
                        FROM interacciones i 
                        JOIN usuarios ue ON i.usuario_id = ue.id 
                        JOIN usuarios ur ON i.objetivo_usuario_id = ur.id 
                        ORDER BY i.fecha_interaccion DESC
                        LIMIT @limit OFFSET @offset", 
                        (MySqlConnection)conn);
                    
                    cmd.Parameters.AddWithValue("@limit", elementosPorPagina);
                    cmd.Parameters.AddWithValue("@offset", paginaActual * elementosPorPagina);
                    
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            int id = Convert.ToInt32(reader["id"]);
                            string emisor = reader["emisor"].ToString();
                            string receptor = reader["receptor"].ToString();
                            string tipo = reader["tipo"].ToString();
                            DateTime fecha = Convert.ToDateTime(reader["fecha"]);
                            
                            table.AddRow(
                                id.ToString(),
                                emisor,
                                receptor,
                                tipo == "Like" ? "[green]Like[/]" : "[red]Dislike[/]",
                                fecha.ToString("yyyy-MM-dd HH:mm")
                            );
                        }
                    }
                }
                
                AnsiConsole.Write(table);
                
                // Calcular número total de páginas
                int totalPaginas = (int)Math.Ceiling((double)totalInteracciones / elementosPorPagina);
                
                // Mostrar información de paginación
                AnsiConsole.MarkupLine($"Mostrando {paginaActual * elementosPorPagina + 1}-{Math.Min((paginaActual + 1) * elementosPorPagina, totalInteracciones)} de {totalInteracciones} interacciones. Página {paginaActual + 1} de {totalPaginas}");
                
                // Mostrar opciones de navegación
                var opciones = new List<string>();
                
                if (paginaActual > 0)
                {
                    opciones.Add("Página anterior");
                }
                
                if ((paginaActual + 1) * elementosPorPagina < totalInteracciones)
                {
                    opciones.Add("Página siguiente");
                }
                
                opciones.Add("Filtrar por usuario");
                opciones.Add("Volver al menú anterior");
                
                // Seleccionar opción
                var opcion = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("\n[bold]¿Qué desea hacer?[/]")
                        .PageSize(opciones.Count)
                        .HighlightStyle(new Style(foreground: Color.Blue))
                        .AddChoices(opciones));
                
                switch (opcion)
                {
                    case "Página anterior":
                        paginaActual--;
                        break;
                    
                    case "Página siguiente":
                        paginaActual++;
                        break;
                    
                    case "Filtrar por usuario":
                        await FiltrarInteraccionesPorUsuario();
                        break;
                    
                    case "Volver al menú anterior":
                        salir = true;
                        break;
                }
            }
        }
        
        /// <summary>
        /// Filtrar interacciones por usuario
        /// </summary>
        private async Task FiltrarInteraccionesPorUsuario()
        {
            Console.Clear();
            
            // Crear título con Spectre.Console
            AnsiConsole.Write(
                new FigletText("Filtrar Interacciones")
                    .Centered()
                    .Color(Color.Blue));
            
            AnsiConsole.WriteLine();
            
            // Obtener lista de usuarios
            var usuarios = new Dictionary<string, int>();
            
            using (var conn = _dbFactory.CreateConnection())
            {
                conn.Open();
                var cmd = new MySqlCommand(
                    "SELECT id, nombre FROM usuarios ORDER BY nombre", 
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
            
            // Añadir opción para mostrar todos los usuarios
            usuarios.Add("<Todos los usuarios>", -1);
            
            // Seleccionar usuario
            var usuarioSeleccionado = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[bold]Seleccione un usuario para filtrar:[/]")
                    .PageSize(10)
                    .HighlightStyle(new Style(foreground: Color.Blue))
                    .AddChoices(usuarios.Keys));
            
            int usuarioId = usuarios[usuarioSeleccionado];
            
            // Si se seleccionó "Todos los usuarios", regresar al listado completo
            if (usuarioId == -1)
            {
                return;
            }
            
            // Seleccionar tipo de filtro
            var tipoFiltro = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[bold]Tipo de filtro:[/]")
                    .PageSize(3)
                    .HighlightStyle(new Style(foreground: Color.Blue))
                    .AddChoices(new[] { "Interacciones enviadas", "Interacciones recibidas", "Ambas direcciones" }));
            
            Console.Clear();
            
            // Crear título con Spectre.Console
            string nombreUsuario = usuarioSeleccionado.Split('(')[0].Trim();
            AnsiConsole.Write(
                new FigletText("Interacciones")
                    .Centered()
                    .Color(Color.Blue));
            
            AnsiConsole.WriteLine();
            
            // Mostrar tabla de interacciones filtradas
            string subtitulo = tipoFiltro switch {
                "Interacciones enviadas" => $"[bold]Interacciones enviadas por {nombreUsuario}[/]",
                "Interacciones recibidas" => $"[bold]Interacciones recibidas por {nombreUsuario}[/]",
                _ => $"[bold]Todas las interacciones de {nombreUsuario}[/]"
            };
            
            AnsiConsole.MarkupLine(subtitulo);
            var table = new Table();
            table.Border = TableBorder.Rounded;
            
            // Añadir columnas
            table.AddColumn(new TableColumn("[b]ID[/]").Centered());
            table.AddColumn(new TableColumn("[b]Usuario Emisor[/]"));
            table.AddColumn(new TableColumn("[b]Usuario Receptor[/]"));
            table.AddColumn(new TableColumn("[b]Tipo[/]").Centered());
            table.AddColumn(new TableColumn("[b]Fecha[/]").Centered());
            
            // Construir la condición SQL según el tipo de filtro
            string condicion = tipoFiltro switch {
                "Interacciones enviadas" => "i.usuario_id = @usuarioId",
                "Interacciones recibidas" => "i.objetivo_usuario_id = @usuarioId",
                _ => "(i.usuario_id = @usuarioId OR i.objetivo_usuario_id = @usuarioId)"
            };
            
            // Obtener datos de interacciones filtradas
            using (var conn = _dbFactory.CreateConnection())
            {
                conn.Open();
                var cmd = new MySqlCommand(
                    $@"SELECT i.id, 
                    ue.nombre as emisor, 
                    ur.nombre as receptor, 
                    CASE WHEN i.le_gusto = 1 THEN 'Like' ELSE 'Dislike' END as tipo,
                    i.fecha_interaccion as fecha
                    FROM interacciones i 
                    JOIN usuarios ue ON i.usuario_id = ue.id 
                    JOIN usuarios ur ON i.objetivo_usuario_id = ur.id 
                    WHERE {condicion}
                    ORDER BY i.fecha_interaccion DESC
                    LIMIT 50", 
                    (MySqlConnection)conn);
                
                cmd.Parameters.AddWithValue("@usuarioId", usuarioId);
                
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    bool hayInteracciones = false;
                    
                    while (await reader.ReadAsync())
                    {
                        hayInteracciones = true;
                        int id = Convert.ToInt32(reader["id"]);
                        string emisor = reader["emisor"].ToString();
                        string receptor = reader["receptor"].ToString();
                        string tipo = reader["tipo"].ToString();
                        DateTime fecha = Convert.ToDateTime(reader["fecha"]);
                        
                        table.AddRow(
                            id.ToString(),
                            emisor,
                            receptor,
                            tipo == "Like" ? "[green]Like[/]" : "[red]Dislike[/]",
                            fecha.ToString("yyyy-MM-dd HH:mm")
                        );
                    }
                    
                    if (!hayInteracciones)
                    {
                        AnsiConsole.MarkupLine("[yellow]No se encontraron interacciones con los filtros seleccionados.[/]");
                    }
                    else
                    {
                        AnsiConsole.Write(table);
                    }
                }
            }
            
            AnsiConsole.WriteLine("\nPresione cualquier tecla para continuar.");
            Console.ReadKey();
        }
        
        #endregion

        #region Métodos CRUD para Coincidencias
        
        /// <summary>
        /// Interfaz de usuario para ver más coincidencias
        /// </summary>
        public async Task VerCoincidencias()
        {
            Console.Clear();
            bool salir = false;
            int paginaActual = 0;
            const int elementosPorPagina = 20;
            
            while (!salir)
            {
                Console.Clear();
                
                // Crear título con Spectre.Console
                AnsiConsole.Write(
                    new FigletText("Coincidencias")
                        .Centered()
                        .Color(Color.Fuchsia));
                
                AnsiConsole.WriteLine();
                
                // Mostrar tabla de coincidencias existentes con paginación
                AnsiConsole.MarkupLine($"[bold]Coincidencias (Página {paginaActual + 1})[/]");
                var table = new Table();
                table.Border = TableBorder.Rounded;
                
                // Añadir columnas
                table.AddColumn(new TableColumn("[b]ID[/]").Centered());
                table.AddColumn(new TableColumn("[b]Usuario 1[/]"));
                table.AddColumn(new TableColumn("[b]Usuario 2[/]"));
                table.AddColumn(new TableColumn("[b]Fecha Coincidencia[/]").Centered());
                
                // Obtener el total de coincidencias y las de la página actual
                int totalCoincidencias = 0;
                
                using (var conn = _dbFactory.CreateConnection())
                {
                    conn.Open();
                    
                    // Primero contar el total de coincidencias
                    var cmdCount = new MySqlCommand(
                        "SELECT COUNT(*) FROM coincidencias", 
                        (MySqlConnection)conn);
                    
                    totalCoincidencias = Convert.ToInt32(await cmdCount.ExecuteScalarAsync());
                    
                    // Obtener las coincidencias para la página actual
                    var cmd = new MySqlCommand(
                        @"SELECT c.id, 
                        u1.nombre as usuario1, 
                        u2.nombre as usuario2, 
                        c.fecha_coincidencia as fecha
                        FROM coincidencias c 
                        JOIN usuarios u1 ON c.usuario1_id = u1.id 
                        JOIN usuarios u2 ON c.usuario2_id = u2.id 
                        ORDER BY c.fecha_coincidencia DESC
                        LIMIT @limit OFFSET @offset", 
                        (MySqlConnection)conn);
                    
                    cmd.Parameters.AddWithValue("@limit", elementosPorPagina);
                    cmd.Parameters.AddWithValue("@offset", paginaActual * elementosPorPagina);
                    
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            int id = Convert.ToInt32(reader["id"]);
                            string usuario1 = reader["usuario1"].ToString();
                            string usuario2 = reader["usuario2"].ToString();
                            DateTime fecha = Convert.ToDateTime(reader["fecha"]);
                            
                            table.AddRow(
                                id.ToString(),
                                usuario1,
                                usuario2,
                                fecha.ToString("yyyy-MM-dd HH:mm")
                            );
                        }
                    }
                }
                
                if (totalCoincidencias == 0)
                {
                    AnsiConsole.MarkupLine("[yellow]No hay coincidencias registradas.[/]");
                    AnsiConsole.WriteLine("\nPresione cualquier tecla para continuar.");
                    Console.ReadKey();
                    return;
                }
                
                AnsiConsole.Write(table);
                
                // Calcular número total de páginas
                int totalPaginas = (int)Math.Ceiling((double)totalCoincidencias / elementosPorPagina);
                
                // Mostrar información de paginación
                AnsiConsole.MarkupLine($"Mostrando {paginaActual * elementosPorPagina + 1}-{Math.Min((paginaActual + 1) * elementosPorPagina, totalCoincidencias)} de {totalCoincidencias} coincidencias. Página {paginaActual + 1} de {totalPaginas}");
                
                // Mostrar opciones de navegación
                var opciones = new List<string>();
                
                if (paginaActual > 0)
                {
                    opciones.Add("Página anterior");
                }
                
                if ((paginaActual + 1) * elementosPorPagina < totalCoincidencias)
                {
                    opciones.Add("Página siguiente");
                }
                
                opciones.Add("Buscar por usuario");
                opciones.Add("Ver detalles de una coincidencia");
                opciones.Add("Volver al menú anterior");
                
                // Seleccionar opción
                var opcion = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("\n[bold]¿Qué desea hacer?[/]")
                        .PageSize(Math.Min(opciones.Count, 10))
                        .HighlightStyle(new Style(foreground: Color.Fuchsia))
                        .AddChoices(opciones));
                
                switch (opcion)
                {
                    case "Página anterior":
                        paginaActual--;
                        break;
                    
                    case "Página siguiente":
                        paginaActual++;
                        break;
                    
                    case "Buscar por usuario":
                        await BuscarCoincidenciasPorUsuario();
                        break;
                    
                    case "Ver detalles de una coincidencia":
                        await VerDetallesCoincidencia();
                        break;
                    
                    case "Volver al menú anterior":
                        salir = true;
                        break;
                }
            }
        }
        
        /// <summary>
        /// Buscar coincidencias por usuario
        /// </summary>
        private async Task BuscarCoincidenciasPorUsuario()
        {
            Console.Clear();
            
            // Crear título con Spectre.Console
            AnsiConsole.Write(
                new FigletText("Buscar Coincidencias")
                    .Centered()
                    .Color(Color.Fuchsia));
            
            AnsiConsole.WriteLine();
            
            // Obtener lista de usuarios
            var usuarios = new Dictionary<string, int>();
            
            using (var conn = _dbFactory.CreateConnection())
            {
                conn.Open();
                var cmd = new MySqlCommand(
                    "SELECT id, nombre FROM usuarios ORDER BY nombre", 
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
            
            // Seleccionar usuario
            var usuarioSeleccionado = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[bold]Seleccione un usuario para ver sus coincidencias:[/]")
                    .PageSize(10)
                    .HighlightStyle(new Style(foreground: Color.Fuchsia))
                    .AddChoices(usuarios.Keys));
            
            int usuarioId = usuarios[usuarioSeleccionado];
            string nombreUsuario = usuarioSeleccionado.Split('(')[0].Trim();
            
            Console.Clear();
            
            // Crear título con Spectre.Console
            AnsiConsole.Write(
                new FigletText("Coincidencias")
                    .Centered()
                    .Color(Color.Fuchsia));
            
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine($"[bold]Coincidencias de {nombreUsuario}[/]");
            
            // Tabla para mostrar resultados
            var table = new Table();
            table.Border = TableBorder.Rounded;
            
            // Añadir columnas
            table.AddColumn(new TableColumn("[b]ID[/]").Centered());
            table.AddColumn(new TableColumn("[b]Usuario[/]"));
            table.AddColumn(new TableColumn("[b]Match con[/]"));
            table.AddColumn(new TableColumn("[b]Fecha[/]").Centered());
            
            // Obtener coincidencias del usuario
            int totalCoincidencias = 0;
            
            using (var conn = _dbFactory.CreateConnection())
            {
                conn.Open();
                var cmd = new MySqlCommand(
                    @"SELECT c.id, 
                    u1.nombre as usuario1, u1.id as usuario1_id,
                    u2.nombre as usuario2, u2.id as usuario2_id,
                    c.fecha_coincidencia as fecha
                    FROM coincidencias c 
                    JOIN usuarios u1 ON c.usuario1_id = u1.id 
                    JOIN usuarios u2 ON c.usuario2_id = u2.id 
                    WHERE c.usuario1_id = @usuarioId OR c.usuario2_id = @usuarioId
                    ORDER BY c.fecha_coincidencia DESC", 
                    (MySqlConnection)conn);
                
                cmd.Parameters.AddWithValue("@usuarioId", usuarioId);
                
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        totalCoincidencias++;
                        int id = Convert.ToInt32(reader["id"]);
                        int usuario1Id = Convert.ToInt32(reader["usuario1_id"]);
                        int usuario2Id = Convert.ToInt32(reader["usuario2_id"]);
                        string usuario1 = reader["usuario1"].ToString();
                        string usuario2 = reader["usuario2"].ToString();
                        DateTime fecha = Convert.ToDateTime(reader["fecha"]);
                        
                        // Determinar cuál es el usuario actual y cuál es la pareja
                        string usuarioActual = usuario1Id == usuarioId ? usuario1 : usuario2;
                        string pareja = usuario1Id == usuarioId ? usuario2 : usuario1;
                        
                        table.AddRow(
                            id.ToString(),
                            usuarioActual,
                            pareja,
                            fecha.ToString("yyyy-MM-dd HH:mm")
                        );
                    }
                }
            }
            
            if (totalCoincidencias == 0)
            {
                AnsiConsole.MarkupLine($"[yellow]El usuario {nombreUsuario} no tiene coincidencias registradas.[/]");
            }
            else
            {
                AnsiConsole.Write(table);
                AnsiConsole.MarkupLine($"\nSe encontraron {totalCoincidencias} coincidencias para {nombreUsuario}.");
            }
            
            AnsiConsole.WriteLine("\nPresione cualquier tecla para continuar.");
            Console.ReadKey();
        }
        
        /// <summary>
        /// Ver detalles de una coincidencia específica
        /// </summary>
        private async Task VerDetallesCoincidencia()
        {
            Console.Clear();
            
            // Crear título con Spectre.Console
            AnsiConsole.Write(
                new FigletText("Detalles Coincidencia")
                    .Centered()
                    .Color(Color.Fuchsia));
            
            AnsiConsole.WriteLine();
            
            // Solicitar ID de la coincidencia
            int coincidenciaId;
            while (true)
            {
                try
                {
                    coincidenciaId = AnsiConsole.Ask<int>("[fuchsia]Ingrese el ID de la coincidencia:[/] ");
                    break;
                }
                catch
                {
                    AnsiConsole.MarkupLine("[red]Por favor, ingrese un número válido.[/]");
                }
            }
            
            // Obtener detalles de la coincidencia
            bool coincidenciaEncontrada = false;
            int matchUsuario1Id = 0;
            int matchUsuario2Id = 0;
            
            using (var conn = _dbFactory.CreateConnection())
            {
                conn.Open();
                var cmd = new MySqlCommand(
                    @"SELECT c.id, c.usuario1_id, c.usuario2_id, c.fecha_coincidencia,
                    u1.nombre as usuario1_nombre, u1.edad as usuario1_edad, 
                    u2.nombre as usuario2_nombre, u2.edad as usuario2_edad,
                    g1.descripcion as usuario1_genero, g2.descripcion as usuario2_genero,
                    ci1.nombre as usuario1_ciudad, ci2.nombre as usuario2_ciudad
                    FROM coincidencias c
                    JOIN usuarios u1 ON c.usuario1_id = u1.id
                    JOIN usuarios u2 ON c.usuario2_id = u2.id
                    JOIN generos g1 ON u1.genero = g1.id
                    JOIN generos g2 ON u2.genero = g2.id
                    JOIN ciudades ci1 ON u1.ciudad_id = ci1.id
                    JOIN ciudades ci2 ON u2.ciudad_id = ci2.id
                    WHERE c.id = @coincidenciaId", 
                    (MySqlConnection)conn);
                
                cmd.Parameters.AddWithValue("@coincidenciaId", coincidenciaId);
                
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        coincidenciaEncontrada = true;
                        
                        // Crear panel con detalles de la coincidencia
                        // Obtener datos
                        DateTime fechaCoincidencia = Convert.ToDateTime(reader["fecha_coincidencia"]);
                        
                        // Usuario 1
                        int usuario1Id = Convert.ToInt32(reader["usuario1_id"]);
                        string usuario1Nombre = reader["usuario1_nombre"].ToString();
                        int usuario1Edad = Convert.ToInt32(reader["usuario1_edad"]);
                        string usuario1Genero = reader["usuario1_genero"].ToString();
                        string usuario1Ciudad = reader["usuario1_ciudad"].ToString();
                        
                        // Usuario 2
                        int usuario2Id = Convert.ToInt32(reader["usuario2_id"]);
                        string usuario2Nombre = reader["usuario2_nombre"].ToString();
                        int usuario2Edad = Convert.ToInt32(reader["usuario2_edad"]);
                        string usuario2Genero = reader["usuario2_genero"].ToString();
                        string usuario2Ciudad = reader["usuario2_ciudad"].ToString();
                        
                        // Guardar los IDs para consultar interacciones después
                        matchUsuario1Id = usuario1Id;
                        matchUsuario2Id = usuario2Id;
                        
                        // Construir mensaje de detalles en un panel
                        var detallesTexto = $"[bold fuchsia]Fecha de coincidencia:[/] {fechaCoincidencia.ToString("yyyy-MM-dd HH:mm")}\n\n" +
                            $"[bold fuchsia]Usuario 1:[/]\n" +
                            $"  [bold]ID:[/] {usuario1Id}\n" +
                            $"  [bold]Nombre:[/] {usuario1Nombre}\n" +
                            $"  [bold]Edad:[/] {usuario1Edad} años\n" +
                            $"  [bold]Género:[/] {usuario1Genero}\n" +
                            $"  [bold]Ciudad:[/] {usuario1Ciudad}\n\n" +
                            $"[bold fuchsia]Usuario 2:[/]\n" +
                            $"  [bold]ID:[/] {usuario2Id}\n" +
                            $"  [bold]Nombre:[/] {usuario2Nombre}\n" +
                            $"  [bold]Edad:[/] {usuario2Edad} años\n" +
                            $"  [bold]Género:[/] {usuario2Genero}\n" +
                            $"  [bold]Ciudad:[/] {usuario2Ciudad}\n";
                        
                        var panel = new Panel(new Markup(detallesTexto));
                        panel.Header = new PanelHeader($"[bold fuchsia]Detalles de la Coincidencia ID: {coincidenciaId}[/]");
                        panel.Border = BoxBorder.Rounded;
                        panel.Expand = true;
                        
                        AnsiConsole.Write(panel);
                    }
                }
                
                if (coincidenciaEncontrada)
                {
                    // Obtener interacciones que llevaron al match
                    var interacciones = new Table();
                    interacciones.Border = TableBorder.Rounded;
                    interacciones.Title = new TableTitle("[bold fuchsia]Interacciones que llevaron al match[/]");
                    
                    // Añadir columnas
                    interacciones.AddColumn(new TableColumn("[b]Usuario[/]"));
                    interacciones.AddColumn(new TableColumn("[b]Acción[/]").Centered());
                    interacciones.AddColumn(new TableColumn("[b]Hacia[/]"));
                    interacciones.AddColumn(new TableColumn("[b]Fecha[/]").Centered());
                    
                    // Consultar interacciones
                    var cmdInteracciones = new MySqlCommand(
                        @"SELECT u1.nombre as emisor, u2.nombre as receptor, 
                        i.le_gusto, i.fecha_interaccion
                        FROM interacciones i
                        JOIN usuarios u1 ON i.usuario_id = u1.id
                        JOIN usuarios u2 ON i.objetivo_usuario_id = u2.id
                        WHERE (i.usuario_id = @usuario1Id AND i.objetivo_usuario_id = @usuario2Id) OR
                        (i.usuario_id = @usuario2Id AND i.objetivo_usuario_id = @usuario1Id)
                        ORDER BY i.fecha_interaccion", 
                        (MySqlConnection)conn);
                    
                    cmdInteracciones.Parameters.AddWithValue("@usuario1Id", matchUsuario1Id);
                    cmdInteracciones.Parameters.AddWithValue("@usuario2Id", matchUsuario2Id);
                    
                    using (var readerInteracciones = await cmdInteracciones.ExecuteReaderAsync())
                    {
                        bool hayInteracciones = false;
                        
                        while (await readerInteracciones.ReadAsync())
                        {
                            hayInteracciones = true;
                            
                            string emisor = readerInteracciones["emisor"].ToString();
                            string receptor = readerInteracciones["receptor"].ToString();
                            bool leGusto = Convert.ToBoolean(readerInteracciones["le_gusto"]);
                            DateTime fecha = Convert.ToDateTime(readerInteracciones["fecha_interaccion"]);
                            
                            interacciones.AddRow(
                                emisor,
                                leGusto ? "[green]Like[/]" : "[red]Dislike[/]",
                                receptor,
                                fecha.ToString("yyyy-MM-dd HH:mm")
                            );
                        }
                        
                        if (hayInteracciones)
                        {
                            AnsiConsole.WriteLine();
                            AnsiConsole.Write(interacciones);
                        }
                        else
                        {
                            AnsiConsole.MarkupLine("\n[yellow]No se encontraron interacciones que llevaran a este match.[/]");
                        }
                    }
                }
                else
                {
                    AnsiConsole.MarkupLine($"[red]No se encontró ninguna coincidencia con el ID {coincidenciaId}.[/]");
                }
            }
            
            AnsiConsole.WriteLine("\nPresione cualquier tecla para continuar.");
            Console.ReadKey();
        }
        
        /// <summary>
        /// Interfaz de usuario para eliminar una coincidencia
        /// </summary>
        public async Task EliminarCoincidencia()
        {
            Console.Clear();
            
            // Crear título con Spectre.Console
            AnsiConsole.Write(
                new FigletText("Eliminar Coincidencia")
                    .Centered()
                    .Color(Color.Red));
            
            AnsiConsole.WriteLine();
            
            // Mostrar tabla de coincidencias existentes
            AnsiConsole.MarkupLine("[bold]Coincidencias existentes:[/]");
            var table = new Table();
            table.Border = TableBorder.Rounded;
            
            // Añadir columnas
            table.AddColumn(new TableColumn("[b]ID[/]").Centered());
            table.AddColumn(new TableColumn("[b]Usuario 1[/]"));
            table.AddColumn(new TableColumn("[b]Usuario 2[/]"));
            table.AddColumn(new TableColumn("[b]Fecha Coincidencia[/]").Centered());
            
            // Obtener coincidencias existentes
            var coincidencias = new Dictionary<string, int>();
            
            using (var conn = _dbFactory.CreateConnection())
            {
                conn.Open();
                var cmd = new MySqlCommand(
                    @"SELECT c.id, 
                    u1.nombre as usuario1, 
                    u2.nombre as usuario2, 
                    c.fecha_coincidencia as fecha
                    FROM coincidencias c 
                    JOIN usuarios u1 ON c.usuario1_id = u1.id 
                    JOIN usuarios u2 ON c.usuario2_id = u2.id 
                    ORDER BY c.fecha_coincidencia DESC", 
                    (MySqlConnection)conn);
                
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        int id = Convert.ToInt32(reader["id"]);
                        string usuario1 = reader["usuario1"].ToString();
                        string usuario2 = reader["usuario2"].ToString();
                        DateTime fecha = Convert.ToDateTime(reader["fecha"]);
                        
                        table.AddRow(
                            id.ToString(),
                            usuario1,
                            usuario2,
                            fecha.ToString("yyyy-MM-dd HH:mm")
                        );
                        
                        coincidencias.Add($"ID: {id} - {usuario1} ❤ {usuario2} ({fecha.ToString("yyyy-MM-dd")})", id);
                    }
                }
            }
            
            AnsiConsole.Write(table);
            AnsiConsole.WriteLine();
            
            if (coincidencias.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]No hay coincidencias registradas.[/]");
                AnsiConsole.WriteLine("\nPresione cualquier tecla para continuar.");
                Console.ReadKey();
                return;
            }
            
            // Seleccionar coincidencia a eliminar
            var coincidenciaSeleccionada = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[bold]Seleccione la coincidencia que desea eliminar:[/]")
                    .PageSize(10)
                    .HighlightStyle(new Style(foreground: Color.Red))
                    .AddChoices(coincidencias.Keys));
            
            int coincidenciaId = coincidencias[coincidenciaSeleccionada];
            
            // Confirmar eliminación
            var confirmar = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[bold red]¿Está seguro de que desea eliminar esta coincidencia?[/]")
                    .PageSize(3)
                    .HighlightStyle(new Style(foreground: Color.Red))
                    .AddChoices(new[] { "Cancelar", "Eliminar" }));
            
            if (confirmar == "Eliminar")
            {
                // Mostrar spinner mientras se procesa
                var resultado = false;
                await AnsiConsole.Status()
                    .StartAsync("Eliminando coincidencia...", async ctx => 
                    {
                        resultado = await _adminService.EliminarCoincidencia(coincidenciaId);
                    });
                
                if (resultado)
                {
                    AnsiConsole.MarkupLine("\n[green]Coincidencia eliminada exitosamente.[/]");
                }
                else
                {
                    AnsiConsole.MarkupLine("\n[red]Error al eliminar la coincidencia.[/]");
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

        #region Métodos CRUD para Ciudades
        
        /// <summary>
        /// Interfaz de usuario para añadir una ciudad
        /// </summary>
        public async Task AñadirCiudad()
        {
            Console.Clear();
            
            // Crear título con Spectre.Console
            AnsiConsole.Write(
                new FigletText("Añadir Ciudad")
                    .Centered()
                    .Color(Color.Green));
            
            AnsiConsole.WriteLine();
            
            // Obtener lista de departamentos
            var departamentos = new Dictionary<string, int>();
            
            using (var conn = _dbFactory.CreateConnection())
            {
                conn.Open();
                var cmd = new MySqlCommand(
                    @"SELECT d.id, d.nombre, p.nombre as pais 
                    FROM departamentos d
                    JOIN paises p ON d.pais_id = p.id
                    ORDER BY p.nombre, d.nombre", 
                    (MySqlConnection)conn);
                
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        int id = Convert.ToInt32(reader["id"]);
                        string nombre = reader["nombre"].ToString();
                        string pais = reader["pais"].ToString();
                        
                        departamentos.Add($"{nombre} - {pais}", id);
                    }
                }
            }
            
            if (departamentos.Count == 0)
            {
                AnsiConsole.MarkupLine("[red]No hay departamentos registrados. Debe agregar al menos un departamento antes de crear una ciudad.[/]");
                AnsiConsole.WriteLine("\nPresione cualquier tecla para continuar.");
                Console.ReadKey();
                return;
            }
            
            // Solicitar nombre de la ciudad
            string nombreCiudad = AnsiConsole.Ask<string>("[green]Nombre de la ciudad:[/] ");
            
            // Validar nombre
            if (string.IsNullOrWhiteSpace(nombreCiudad))
            {
                AnsiConsole.MarkupLine("[red]El nombre de la ciudad no puede estar vacío.[/]");
                AnsiConsole.WriteLine("\nPresione cualquier tecla para continuar.");
                Console.ReadKey();
                return;
            }
            
            // Seleccionar departamento
            var departamentoSeleccionado = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[bold]Seleccione el departamento:[/]")
                    .PageSize(10)
                    .HighlightStyle(new Style(foreground: Color.Green))
                    .AddChoices(departamentos.Keys));
            
            int departamentoId = departamentos[departamentoSeleccionado];
            
            // Confirmar creación
            var confirmar = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title($"\n[bold]¿Confirmar la creación de la ciudad [green]{nombreCiudad}[/]?[/]")
                    .PageSize(3)
                    .HighlightStyle(new Style(foreground: Color.Green))
                    .AddChoices(new[] { "Cancelar", "Confirmar" }));
            
            if (confirmar == "Confirmar")
            {
                // Mostrar spinner mientras se procesa
                var resultado = false;
                await AnsiConsole.Status()
                    .StartAsync("Creando ciudad...", async ctx => 
                    {
                        resultado = await _adminService.AñadirCiudad(nombreCiudad, departamentoId);
                    });
                
                if (resultado)
                {
                    AnsiConsole.MarkupLine("\n[green]Ciudad creada exitosamente.[/]");
                }
                else
                {
                    AnsiConsole.MarkupLine("\n[red]Error al crear la ciudad.[/]");
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
        /// Interfaz de usuario para editar una ciudad
        /// </summary>
        public async Task EditarCiudad()
        {
            Console.Clear();
            
            // Crear título con Spectre.Console
            AnsiConsole.Write(
                new FigletText("Editar Ciudad")
                    .Centered()
                    .Color(Color.Yellow));
            
            AnsiConsole.WriteLine();
            
            // Mostrar tabla de ciudades existentes
            AnsiConsole.MarkupLine("[bold]Ciudades existentes:[/]");
            var table = new Table();
            table.Border = TableBorder.Rounded;
            
            // Añadir columnas
            table.AddColumn(new TableColumn("[b]ID[/]").Centered());
            table.AddColumn(new TableColumn("[b]Ciudad[/]"));
            table.AddColumn(new TableColumn("[b]Departamento[/]"));
            table.AddColumn(new TableColumn("[b]País[/]"));
            
            // Obtener ciudades existentes
            var ciudades = new Dictionary<string, int>();
            
            using (var conn = _dbFactory.CreateConnection())
            {
                conn.Open();
                var cmd = new MySqlCommand(
                    @"SELECT c.id, c.nombre, d.nombre as departamento, p.nombre as pais
                    FROM ciudades c
                    JOIN departamentos d ON c.departamento_id = d.id
                    JOIN paises p ON d.pais_id = p.id
                    ORDER BY p.nombre, d.nombre, c.nombre", 
                    (MySqlConnection)conn);
                
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        int id = Convert.ToInt32(reader["id"]);
                        string nombre = reader["nombre"].ToString();
                        string departamento = reader["departamento"].ToString();
                        string pais = reader["pais"].ToString();
                        
                        table.AddRow(
                            id.ToString(),
                            nombre,
                            departamento,
                            pais
                        );
                        
                        ciudades.Add($"{nombre} (ID: {id})", id);
                    }
                }
            }
            
            AnsiConsole.Write(table);
            AnsiConsole.WriteLine();
            
            if (ciudades.Count == 0)
            {
                AnsiConsole.MarkupLine("[red]No hay ciudades registradas para editar.[/]");
                AnsiConsole.WriteLine("\nPresione cualquier tecla para continuar.");
                Console.ReadKey();
                return;
            }
            
            // Seleccionar ciudad a editar
            var ciudadSeleccionada = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[bold]Seleccione la ciudad que desea editar:[/]")
                    .PageSize(10)
                    .HighlightStyle(new Style(foreground: Color.Yellow))
                    .AddChoices(ciudades.Keys));
            
            int ciudadId = ciudades[ciudadSeleccionada];
            
            // Obtener detalles actuales de la ciudad seleccionada
            string nombreActual = "";
            int departamentoIdActual = 0;
            
            using (var conn = _dbFactory.CreateConnection())
            {
                conn.Open();
                var cmd = new MySqlCommand(
                    "SELECT nombre, departamento_id FROM ciudades WHERE id = @ciudadId", 
                    (MySqlConnection)conn);
                cmd.Parameters.AddWithValue("@ciudadId", ciudadId);
                
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        nombreActual = reader["nombre"].ToString();
                        departamentoIdActual = Convert.ToInt32(reader["departamento_id"]);
                    }
                }
            }
            
            // Solicitar nuevo nombre (o mantener el actual)
            var nuevoNombre = AnsiConsole.Prompt(
                new TextPrompt<string>($"[yellow]Nuevo nombre ([grey]actual: {nombreActual}[/]):[/] ")
                    .AllowEmpty()
                    .DefaultValue(nombreActual));
            
            // Obtener lista de departamentos
            var departamentos = new Dictionary<string, int>();
            string departamentoActualNombre = "";
            
            using (var conn = _dbFactory.CreateConnection())
            {
                conn.Open();
                
                // Primero obtener el nombre del departamento actual
                var cmdDepartamentoActual = new MySqlCommand(
                    @"SELECT d.nombre, p.nombre as pais
                    FROM departamentos d
                    JOIN paises p ON d.pais_id = p.id
                    WHERE d.id = @departamentoId", 
                    (MySqlConnection)conn);
                cmdDepartamentoActual.Parameters.AddWithValue("@departamentoId", departamentoIdActual);
                
                using (var reader = await cmdDepartamentoActual.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        string nombre = reader["nombre"].ToString();
                        string pais = reader["pais"].ToString();
                        departamentoActualNombre = $"{nombre} - {pais}";
                    }
                }
                
                // Obtener lista completa de departamentos
                var cmdDepartamentos = new MySqlCommand(
                    @"SELECT d.id, d.nombre, p.nombre as pais
                    FROM departamentos d
                    JOIN paises p ON d.pais_id = p.id
                    ORDER BY p.nombre, d.nombre", 
                    (MySqlConnection)conn);
                
                using (var reader = await cmdDepartamentos.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        int id = Convert.ToInt32(reader["id"]);
                        string nombre = reader["nombre"].ToString();
                        string pais = reader["pais"].ToString();
                        
                        departamentos.Add($"{nombre} - {pais}", id);
                    }
                }
            }
            
            // Mostrar departamento actual y permitir cambiarlo
            AnsiConsole.MarkupLine($"[yellow]Departamento actual: [grey]{departamentoActualNombre}[/][/]");
            
            // Preguntar si desea cambiar el departamento
            var cambiarDepartamento = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[bold]¿Desea cambiar el departamento?[/]")
                    .PageSize(3)
                    .HighlightStyle(new Style(foreground: Color.Yellow))
                    .AddChoices(new[] { "No", "Sí" }));
            
            int nuevoDepartamentoId = departamentoIdActual;
            
            if (cambiarDepartamento == "Sí")
            {
                // Seleccionar nuevo departamento
                var departamentoSeleccionado = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[bold]Seleccione el nuevo departamento:[/]")
                        .PageSize(10)
                        .HighlightStyle(new Style(foreground: Color.Yellow))
                        .AddChoices(departamentos.Keys));
                
                nuevoDepartamentoId = departamentos[departamentoSeleccionado];
            }
            
            // Confirmar edición
            var confirmar = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title($"\n[bold]¿Confirmar los cambios?[/]")
                    .PageSize(3)
                    .HighlightStyle(new Style(foreground: Color.Yellow))
                    .AddChoices(new[] { "Cancelar", "Confirmar" }));
            
            if (confirmar == "Confirmar")
            {
                // Mostrar spinner mientras se procesa
                var resultado = false;
                await AnsiConsole.Status()
                    .StartAsync("Actualizando ciudad...", async ctx => 
                    {
                        resultado = await _adminService.EditarCiudad(ciudadId, nuevoNombre, nuevoDepartamentoId);
                    });
                
                if (resultado)
                {
                    AnsiConsole.MarkupLine("\n[green]Ciudad actualizada exitosamente.[/]");
                }
                else
                {
                    AnsiConsole.MarkupLine("\n[red]Error al actualizar la ciudad.[/]");
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
        /// Interfaz de usuario para eliminar una ciudad
        /// </summary>
        public async Task EliminarCiudad()
        {
            Console.Clear();
            
            // Crear título con Spectre.Console
            AnsiConsole.Write(
                new FigletText("Eliminar Ciudad")
                    .Centered()
                    .Color(Color.Red));
            
            AnsiConsole.WriteLine();
            
            // Mostrar tabla de ciudades existentes
            AnsiConsole.MarkupLine("[bold]Ciudades existentes:[/]");
            var table = new Table();
            table.Border = TableBorder.Rounded;
            
            // Añadir columnas
            table.AddColumn(new TableColumn("[b]ID[/]").Centered());
            table.AddColumn(new TableColumn("[b]Ciudad[/]"));
            table.AddColumn(new TableColumn("[b]Departamento[/]"));
            table.AddColumn(new TableColumn("[b]País[/]"));
            table.AddColumn(new TableColumn("[b]Usuarios[/]").Centered());
            
            // Obtener ciudades existentes
            var ciudades = new Dictionary<string, int>();
            
            using (var conn = _dbFactory.CreateConnection())
            {
                conn.Open();
                var cmd = new MySqlCommand(
                    @"SELECT c.id, c.nombre, d.nombre as departamento, p.nombre as pais,
                    (SELECT COUNT(*) FROM usuarios u WHERE u.ciudad_id = c.id) as usuarios
                    FROM ciudades c
                    JOIN departamentos d ON c.departamento_id = d.id
                    JOIN paises p ON d.pais_id = p.id
                    ORDER BY p.nombre, d.nombre, c.nombre", 
                    (MySqlConnection)conn);
                
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        int id = Convert.ToInt32(reader["id"]);
                        string nombre = reader["nombre"].ToString();
                        string departamento = reader["departamento"].ToString();
                        string pais = reader["pais"].ToString();
                        int usuarios = Convert.ToInt32(reader["usuarios"]);
                        
                        table.AddRow(
                            id.ToString(),
                            nombre,
                            departamento,
                            pais,
                            usuarios > 0 ? $"[red]{usuarios}[/]" : $"[green]{usuarios}[/]"
                        );
                        
                        // Solo podemos eliminar ciudades sin usuarios asociados
                        if (usuarios == 0)
                        {
                            ciudades.Add($"{nombre} (ID: {id}) - {departamento}, {pais}", id);
                        }
                    }
                }
            }
            
            AnsiConsole.Write(table);
            AnsiConsole.WriteLine();
            
            if (ciudades.Count == 0)
            {
                AnsiConsole.MarkupLine("[red]No hay ciudades disponibles para eliminar. Las ciudades con usuarios asociados no se pueden eliminar.[/]");
                AnsiConsole.WriteLine("\nPresione cualquier tecla para continuar.");
                Console.ReadKey();
                return;
            }
            
            // Seleccionar ciudad a eliminar
            var ciudadSeleccionada = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[bold]Seleccione la ciudad que desea eliminar:[/]")
                    .PageSize(10)
                    .HighlightStyle(new Style(foreground: Color.Red))
                    .AddChoices(ciudades.Keys));
            
            int ciudadId = ciudades[ciudadSeleccionada];
            
            // Confirmar eliminación
            var confirmar = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[bold red]¿Está seguro de que desea eliminar esta ciudad?[/]")
                    .PageSize(3)
                    .HighlightStyle(new Style(foreground: Color.Red))
                    .AddChoices(new[] { "Cancelar", "Eliminar" }));
            
            if (confirmar == "Eliminar")
            {
                // Mostrar spinner mientras se procesa
                var resultado = false;
                await AnsiConsole.Status()
                    .StartAsync("Eliminando ciudad...", async ctx => 
                    {
                        resultado = await _adminService.EliminarCiudad(ciudadId);
                    });
                
                if (resultado)
                {
                    AnsiConsole.MarkupLine("\n[green]Ciudad eliminada exitosamente.[/]");
                }
                else
                {
                    AnsiConsole.MarkupLine("\n[red]Error al eliminar la ciudad. Es posible que tenga usuarios asociados o que haya sido referenciada en otros registros.[/]");
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

        #region Métodos CRUD para Departamentos
        
        /// <summary>
        /// Interfaz de usuario para añadir un departamento
        /// </summary>
        public async Task AñadirDepartamento()
        {
            Console.Clear();
            
            // Crear título con Spectre.Console
            AnsiConsole.Write(
                new FigletText("Añadir Departamento")
                    .Centered()
                    .Color(Color.Green));
            
            AnsiConsole.WriteLine();
            
            // Obtener lista de países
            var paises = new Dictionary<string, int>();
            
            using (var conn = _dbFactory.CreateConnection())
            {
                conn.Open();
                var cmd = new MySqlCommand(
                    "SELECT id, nombre FROM paises ORDER BY nombre", 
                    (MySqlConnection)conn);
                
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        int id = Convert.ToInt32(reader["id"]);
                        string nombre = reader["nombre"].ToString();
                        
                        paises.Add(nombre, id);
                    }
                }
            }
            
            if (paises.Count == 0)
            {
                AnsiConsole.MarkupLine("[red]No hay países registrados. Debe agregar al menos un país antes de crear un departamento.[/]");
                AnsiConsole.WriteLine("\nPresione cualquier tecla para continuar.");
                Console.ReadKey();
                return;
            }
            
            // Solicitar nombre del departamento
            string nombreDepartamento = AnsiConsole.Ask<string>("[green]Nombre del departamento:[/] ");
            
            // Validar nombre
            if (string.IsNullOrWhiteSpace(nombreDepartamento))
            {
                AnsiConsole.MarkupLine("[red]El nombre del departamento no puede estar vacío.[/]");
                AnsiConsole.WriteLine("\nPresione cualquier tecla para continuar.");
                Console.ReadKey();
                return;
            }
            
            // Seleccionar país
            var paisSeleccionado = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[bold]Seleccione el país:[/]")
                    .PageSize(10)
                    .HighlightStyle(new Style(foreground: Color.Green))
                    .AddChoices(paises.Keys));
            
            int paisId = paises[paisSeleccionado];
            
            // Confirmar creación
            var confirmar = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title($"\n[bold]¿Confirmar la creación del departamento [green]{nombreDepartamento}[/] para el país [green]{paisSeleccionado}[/]?[/]")
                    .PageSize(3)
                    .HighlightStyle(new Style(foreground: Color.Green))
                    .AddChoices(new[] { "Cancelar", "Confirmar" }));
            
            if (confirmar == "Confirmar")
            {
                // Mostrar spinner mientras se procesa
                var resultado = false;
                await AnsiConsole.Status()
                    .StartAsync("Creando departamento...", async ctx => 
                    {
                        resultado = await _adminService.AñadirDepartamento(nombreDepartamento, paisId);
                    });
                
                if (resultado)
                {
                    AnsiConsole.MarkupLine("\n[green]Departamento creado exitosamente.[/]");
                }
                else
                {
                    AnsiConsole.MarkupLine("\n[red]Error al crear el departamento.[/]");
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
        /// Interfaz de usuario para editar un departamento
        /// </summary>
        public async Task EditarDepartamento()
        {
            Console.Clear();
            
            // Crear título con Spectre.Console
            AnsiConsole.Write(
                new FigletText("Editar Departamento")
                    .Centered()
                    .Color(Color.Yellow));
            
            AnsiConsole.WriteLine();
            
            // Mostrar tabla de departamentos existentes
            AnsiConsole.MarkupLine("[bold]Departamentos existentes:[/]");
            var table = new Table();
            table.Border = TableBorder.Rounded;
            
            // Añadir columnas
            table.AddColumn(new TableColumn("[b]ID[/]").Centered());
            table.AddColumn(new TableColumn("[b]Departamento[/]"));
            table.AddColumn(new TableColumn("[b]País[/]"));
            table.AddColumn(new TableColumn("[b]Ciudades[/]").Centered());
            
            // Obtener departamentos existentes
            var departamentos = new Dictionary<string, int>();
            
            using (var conn = _dbFactory.CreateConnection())
            {
                conn.Open();
                var cmd = new MySqlCommand(
                    @"SELECT d.id, d.nombre, p.nombre as pais,
                    (SELECT COUNT(*) FROM ciudades c WHERE c.departamento_id = d.id) as ciudades
                    FROM departamentos d
                    JOIN paises p ON d.pais_id = p.id
                    ORDER BY p.nombre, d.nombre", 
                    (MySqlConnection)conn);
                
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        int id = Convert.ToInt32(reader["id"]);
                        string nombre = reader["nombre"].ToString();
                        string pais = reader["pais"].ToString();
                        int ciudades = Convert.ToInt32(reader["ciudades"]);
                        
                        table.AddRow(
                            id.ToString(),
                            nombre,
                            pais,
                            ciudades.ToString()
                        );
                        
                        departamentos.Add($"{nombre} (ID: {id}) - {pais}", id);
                    }
                }
            }
            
            AnsiConsole.Write(table);
            AnsiConsole.WriteLine();
            
            if (departamentos.Count == 0)
            {
                AnsiConsole.MarkupLine("[red]No hay departamentos registrados para editar.[/]");
                AnsiConsole.WriteLine("\nPresione cualquier tecla para continuar.");
                Console.ReadKey();
                return;
            }
            
            // Seleccionar departamento a editar
            var departamentoSeleccionado = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[bold]Seleccione el departamento que desea editar:[/]")
                    .PageSize(10)
                    .HighlightStyle(new Style(foreground: Color.Yellow))
                    .AddChoices(departamentos.Keys));
            
            int departamentoId = departamentos[departamentoSeleccionado];
            
            // Obtener detalles actuales del departamento seleccionado
            string nombreActual = "";
            int paisIdActual = 0;
            
            using (var conn = _dbFactory.CreateConnection())
            {
                conn.Open();
                var cmd = new MySqlCommand(
                    "SELECT nombre, pais_id FROM departamentos WHERE id = @departamentoId", 
                    (MySqlConnection)conn);
                cmd.Parameters.AddWithValue("@departamentoId", departamentoId);
                
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        nombreActual = reader["nombre"].ToString();
                        paisIdActual = Convert.ToInt32(reader["pais_id"]);
                    }
                }
            }
            
            // Solicitar nuevo nombre (o mantener el actual)
            var nuevoNombre = AnsiConsole.Prompt(
                new TextPrompt<string>($"[yellow]Nuevo nombre ([grey]actual: {nombreActual}[/]):[/] ")
                    .AllowEmpty()
                    .DefaultValue(nombreActual));
            
            // Obtener lista de países
            var paises = new Dictionary<string, int>();
            string paisActualNombre = "";
            
            using (var conn = _dbFactory.CreateConnection())
            {
                conn.Open();
                
                // Primero obtener el nombre del país actual
                var cmdPaisActual = new MySqlCommand(
                    "SELECT nombre FROM paises WHERE id = @paisId", 
                    (MySqlConnection)conn);
                cmdPaisActual.Parameters.AddWithValue("@paisId", paisIdActual);
                
                using (var reader = await cmdPaisActual.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        paisActualNombre = reader["nombre"].ToString();
                    }
                }
                
                // Obtener lista completa de países
                var cmdPaises = new MySqlCommand(
                    "SELECT id, nombre FROM paises ORDER BY nombre", 
                    (MySqlConnection)conn);
                
                using (var reader = await cmdPaises.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        int id = Convert.ToInt32(reader["id"]);
                        string nombre = reader["nombre"].ToString();
                        
                        paises.Add(nombre, id);
                    }
                }
            }
            
            // Mostrar país actual y permitir cambiarlo
            AnsiConsole.MarkupLine($"[yellow]País actual: [grey]{paisActualNombre}[/][/]");
            
            // Preguntar si desea cambiar el país
            var cambiarPais = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[bold]¿Desea cambiar el país?[/]")
                    .PageSize(3)
                    .HighlightStyle(new Style(foreground: Color.Yellow))
                    .AddChoices(new[] { "No", "Sí" }));
            
            int nuevoPaisId = paisIdActual;
            
            if (cambiarPais == "Sí")
            {
                // Seleccionar nuevo país
                var paisSeleccionado = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[bold]Seleccione el nuevo país:[/]")
                        .PageSize(10)
                        .HighlightStyle(new Style(foreground: Color.Yellow))
                        .AddChoices(paises.Keys));
                
                nuevoPaisId = paises[paisSeleccionado];
            }
            
            // Confirmar edición
            var confirmar = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title($"\n[bold]¿Confirmar los cambios?[/]")
                    .PageSize(3)
                    .HighlightStyle(new Style(foreground: Color.Yellow))
                    .AddChoices(new[] { "Cancelar", "Confirmar" }));
            
            if (confirmar == "Confirmar")
            {
                // Mostrar spinner mientras se procesa
                var resultado = false;
                await AnsiConsole.Status()
                    .StartAsync("Actualizando departamento...", async ctx => 
                    {
                        resultado = await _adminService.EditarDepartamento(departamentoId, nuevoNombre, nuevoPaisId);
                    });
                
                if (resultado)
                {
                    AnsiConsole.MarkupLine("\n[green]Departamento actualizado exitosamente.[/]");
                }
                else
                {
                    AnsiConsole.MarkupLine("\n[red]Error al actualizar el departamento.[/]");
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
        /// Interfaz de usuario para eliminar un departamento
        /// </summary>
        public async Task EliminarDepartamento()
        {
            Console.Clear();
            
            // Crear título con Spectre.Console
            AnsiConsole.Write(
                new FigletText("Eliminar Departamento")
                    .Centered()
                    .Color(Color.Red));
            
            AnsiConsole.WriteLine();
            
            // Mostrar tabla de departamentos existentes
            AnsiConsole.MarkupLine("[bold]Departamentos existentes:[/]");
            var table = new Table();
            table.Border = TableBorder.Rounded;
            
            // Añadir columnas
            table.AddColumn(new TableColumn("[b]ID[/]").Centered());
            table.AddColumn(new TableColumn("[b]Departamento[/]"));
            table.AddColumn(new TableColumn("[b]País[/]"));
            table.AddColumn(new TableColumn("[b]Ciudades[/]").Centered());
            
            // Obtener departamentos existentes
            var departamentos = new Dictionary<string, int>();
            
            using (var conn = _dbFactory.CreateConnection())
            {
                conn.Open();
                var cmd = new MySqlCommand(
                    @"SELECT d.id, d.nombre, p.nombre as pais,
                    (SELECT COUNT(*) FROM ciudades c WHERE c.departamento_id = d.id) as ciudades
                    FROM departamentos d
                    JOIN paises p ON d.pais_id = p.id
                    ORDER BY p.nombre, d.nombre", 
                    (MySqlConnection)conn);
                
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        int id = Convert.ToInt32(reader["id"]);
                        string nombre = reader["nombre"].ToString();
                        string pais = reader["pais"].ToString();
                        int ciudades = Convert.ToInt32(reader["ciudades"]);
                        
                        table.AddRow(
                            id.ToString(),
                            nombre,
                            pais,
                            ciudades > 0 ? $"[red]{ciudades}[/]" : $"[green]{ciudades}[/]"
                        );
                        
                        // Solo podemos eliminar departamentos sin ciudades asociadas
                        if (ciudades == 0)
                        {
                            departamentos.Add($"{nombre} (ID: {id}) - {pais}", id);
                        }
                    }
                }
            }
            
            AnsiConsole.Write(table);
            AnsiConsole.WriteLine();
            
            if (departamentos.Count == 0)
            {
                AnsiConsole.MarkupLine("[red]No hay departamentos disponibles para eliminar. Los departamentos con ciudades asociadas no se pueden eliminar.[/]");
                AnsiConsole.WriteLine("\nPresione cualquier tecla para continuar.");
                Console.ReadKey();
                return;
            }
            
            // Seleccionar departamento a eliminar
            var departamentoSeleccionado = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[bold]Seleccione el departamento que desea eliminar:[/]")
                    .PageSize(10)
                    .HighlightStyle(new Style(foreground: Color.Red))
                    .AddChoices(departamentos.Keys));
            
            int departamentoId = departamentos[departamentoSeleccionado];
            
            // Confirmar eliminación
            var confirmar = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[bold red]¿Está seguro de que desea eliminar este departamento?[/]")
                    .PageSize(3)
                    .HighlightStyle(new Style(foreground: Color.Red))
                    .AddChoices(new[] { "Cancelar", "Eliminar" }));
            
            if (confirmar == "Eliminar")
            {
                // Mostrar spinner mientras se procesa
                var resultado = false;
                await AnsiConsole.Status()
                    .StartAsync("Eliminando departamento...", async ctx => 
                    {
                        resultado = await _adminService.EliminarDepartamento(departamentoId);
                    });
                
                if (resultado)
                {
                    AnsiConsole.MarkupLine("\n[green]Departamento eliminado exitosamente.[/]");
                }
                else
                {
                    AnsiConsole.MarkupLine("\n[red]Error al eliminar el departamento. Es posible que tenga ciudades asociadas no mostradas o que haya sido referenciado en otros registros.[/]");
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
