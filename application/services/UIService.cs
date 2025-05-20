using System;
using System.Threading.Tasks;
using System.Linq;
using MySql.Data.MySqlClient;
using campusLove.infraestructure.mysql;
using campusLove.domain.strategy;
using Spectre.Console;
using System.Collections.Generic;

namespace campusLove.application.services
{
    public class UIService
    {
        private readonly MySqlDbFactory _dbFactory;
        private readonly UsuarioService _usuarioService;
        private readonly InteraccionService _interaccionService;
        private readonly EstadisticaService _estadisticaService;
        private readonly AuthService _authService;
        private readonly AdminService _adminService;
        private readonly AdminUIHelper _adminUIHelper;
        private int _currentUserId;
        private bool _isAdmin;

        public UIService(
            MySqlDbFactory dbFactory,
            UsuarioService usuarioService,
            InteraccionService interaccionService,
            EstadisticaService estadisticaService,
            AuthService authService,
            AdminService adminService)
        {
            _dbFactory = dbFactory;
            _usuarioService = usuarioService;
            _interaccionService = interaccionService;
            _estadisticaService = estadisticaService;
            _authService = authService;
            _adminService = adminService;
            _adminUIHelper = new AdminUIHelper(adminService, dbFactory);
            _currentUserId = -1;
            _isAdmin = false;
        }

        public int GetCurrentUserId() => _currentUserId;

        public async Task MostrarMenuLogin()
        {
            Console.Clear();
            
            // Crear título con Spectre.Console
            AnsiConsole.Write(
                new FigletText("Campus Love")
                    .Centered()
                    .Color(Color.HotPink));
            
            AnsiConsole.WriteLine();
            
            // Crear un panel con el menú
            var panel = new Panel("[bold]Bienvenido a Campus Love[/]");
            panel.Border = BoxBorder.Double;
            panel.Padding = new Padding(2, 1, 2, 1);
            panel.BorderColor(Color.HotPink);
            AnsiConsole.Write(panel);
            
            // Crear una selección con Spectre.Console
            var opcion = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[bold]¿Qué deseas hacer?[/]")
                    .PageSize(10)
                    .HighlightStyle(new Style(foreground: Color.HotPink))
                    .AddChoices(new[]
                    {
                        "1. Iniciar Sesión",
                        "2. Registrarse",
                        "3. Salir"
                    }));
            
            // Extraer el número de la opción
            string seleccion = opcion.Split('.')[0].Trim();
            
            switch (seleccion)
            {
                case "1":
                    await Login();
                    break;
                case "2":
                    await RegistrarUsuario();
                    break;
                case "3":
                    Environment.Exit(0);
                    break;
                default:
                    AnsiConsole.MarkupLine("[red]Opción no válida. Presione cualquier tecla para continuar.[/]");
                    Console.ReadKey();
                    break;
            }
        }

        public async Task MostrarMenuPrincipal()
        {
            Console.Clear();
            
            // Crear título con Spectre.Console
            AnsiConsole.Write(
                new FigletText("Menu Principal")
                    .Centered()
                    .Color(Color.Fuchsia));
            
            AnsiConsole.WriteLine();
            
            // Crear un panel con información del usuario
            var usuarioPanel = new Panel($"[bold]Usuario ID: {_currentUserId}[/]");
            usuarioPanel.Border = BoxBorder.Rounded;
            usuarioPanel.Padding = new Padding(2, 0, 2, 0);
            usuarioPanel.BorderColor(Color.Aqua);
            AnsiConsole.Write(usuarioPanel);
            
            AnsiConsole.WriteLine();
            
            // Lista base de opciones
            var opciones = new List<string>
            {
                "1. Ver perfiles",
                "2. Ver coincidencias",
                "3. Ver estadísticas",
                "4. Elegir estrategia de emparejamiento",
                "5. Cerrar sesión"
            };
            
            // Añadir opción de administrador si el usuario tiene permisos
            if (_isAdmin)
            {
                opciones.Add("6. Menú Administrador");
            }
            
            // Crear una selección con Spectre.Console
            var opcion = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[bold]¿Qué deseas hacer?[/]")
                    .PageSize(10)
                    .HighlightStyle(new Style(foreground: Color.Green))
                    .AddChoices(opciones));
            
            // Extraer el número de la opción
            string seleccion = opcion.Split('.')[0].Trim();
            
            switch (seleccion)
            {
                case "1":
                    await VerPerfiles();
                    break;
                case "2":
                    await VerCoincidencias();
                    break;
                case "3":
                    await VerEstadisticas();
                    break;
                case "4":
                    await SeleccionarEstrategiaEmparejamiento();
                    break;
                case "5":
                    _currentUserId = -1;
                    _isAdmin = false;
                    break;
                case "6":
                    if (_isAdmin)
                    {
                        await MostrarMenuAdministrador();
                    }
                    else
                    {
                        // Esta opción solo debería aparecer para administradores
                        AnsiConsole.MarkupLine("[red]Opción no disponible. Presione cualquier tecla para continuar.[/]");
                        Console.ReadKey();
                    }
                    break;
                default:
                    AnsiConsole.MarkupLine("[red]Opción no válida. Presione cualquier tecla para continuar.[/]");
                    Console.ReadKey();
                    break;
            }
        }

        public async Task Login()
        {
            bool intentarDeNuevo = true;
            while (intentarDeNuevo)
            {
                Console.Clear();
                
                // Crear título con Spectre.Console
                AnsiConsole.Write(
                    new FigletText("Login")
                        .Centered()
                        .Color(Color.Aqua));
                
                AnsiConsole.WriteLine();
                
                // Crear un panel para el formulario
                var panel = new Panel("[bold]Ingresa tus credenciales[/]");
                panel.Border = BoxBorder.Rounded;
                panel.Padding = new Padding(2, 1, 2, 1);
                panel.BorderColor(Color.Aqua);
                AnsiConsole.Write(panel);
                
                AnsiConsole.WriteLine();
                
                // Solicitar correo
                string correo = AnsiConsole.Prompt(
                    new TextPrompt<string>("[bold]Correo:[/] ")
                        .PromptStyle("green")
                        .ValidationErrorMessage("[red]El correo no puede estar vacío[/]")
                        .Validate(email => 
                        {
                            return !string.IsNullOrWhiteSpace(email) 
                                ? ValidationResult.Success() 
                                : ValidationResult.Error();
                        }));
                
                // Si el usuario presiona ESC, volver al menú principal
                if (correo.ToLower() == "esc")
                    return;
                
                // Solicitar contraseña
                string contrasena = AnsiConsole.Prompt(
                    new TextPrompt<string>("[bold]Contraseña:[/] ")
                        .PromptStyle("green")
                        .Secret()
                        .ValidationErrorMessage("[red]La contraseña no puede estar vacía[/]")
                        .Validate(pass => 
                        {
                            return !string.IsNullOrWhiteSpace(pass) 
                                ? ValidationResult.Success() 
                                : ValidationResult.Error();
                        }));
                
                // Si el usuario presiona ESC, volver al menú principal
                if (contrasena.ToLower() == "esc")
                    return;
                
                try
                {
                    // Mostrar un spinner mientras se autentica
                    var (success, userId, isAdmin) = await AnsiConsole.Status()
                        .StartAsync("Autenticando...", async ctx => 
                        {
                            ctx.Spinner(Spinner.Known.Dots);
                            ctx.SpinnerStyle(Style.Parse("green"));
                            
                            // Realizar la autenticación
                            return await _authService.Login(correo, contrasena);
                        });
                    
                    if (success)
                    {
                        _currentUserId = userId;
                        _isAdmin = isAdmin;
                        AnsiConsole.MarkupLine("\n[green]¡Inicio de sesión exitoso![/]");
                        AnsiConsole.MarkupLine("\nPresione cualquier tecla para continuar.");
                        Console.ReadKey();
                        intentarDeNuevo = false;
                    }
                    else
                    {
                        AnsiConsole.MarkupLine("\n[red]Credenciales incorrectas.[/]");
                        
                        // Preguntar si desea intentar de nuevo
                        var opcion = AnsiConsole.Prompt(
                            new SelectionPrompt<string>()
                                .Title("¿Qué deseas hacer?")
                                .PageSize(3)
                                .AddChoices(new[] { "Intentar de nuevo", "Volver al menú principal" }));
                        
                        if (opcion == "Volver al menú principal")
                            return;
                    }
                }
                catch (Exception ex)
                {
                    AnsiConsole.MarkupLine($"\n[red]Error: {ex.Message}[/]");
                    
                    // Preguntar si desea intentar de nuevo
                    var opcion = AnsiConsole.Prompt(
                        new SelectionPrompt<string>()
                            .Title("¿Qué deseas hacer?")
                            .PageSize(3)
                            .AddChoices(new[] { "Intentar de nuevo", "Volver al menú principal" }));
                    
                    if (opcion == "Volver al menú principal")
                        return;
                }
            }
        }

        private async Task RegistrarUsuario()
        {
            bool intentarDeNuevo = true;
            while (intentarDeNuevo)
            {
                try
                {
                    Console.Clear();
                    
                    // Crear título con Spectre.Console
                    AnsiConsole.Write(
                        new FigletText("Registro")
                            .Centered()
                            .Color(Color.HotPink));
                    
                    AnsiConsole.WriteLine();
                    
                    // Crear un panel para el formulario
                    var panel = new Panel("[bold]Completa tu perfil[/]");
                    panel.Border = BoxBorder.Rounded;
                    panel.Padding = new Padding(2, 1, 2, 1);
                    panel.BorderColor(Color.HotPink);
                    AnsiConsole.Write(panel);
                    
                    AnsiConsole.WriteLine();
                    
                    // Solicitar nombre
                    string nombre = AnsiConsole.Prompt(
                        new TextPrompt<string>("[bold]Nombre[/] (mínimo 3 caracteres): ")
                            .PromptStyle("green")
                            .ValidationErrorMessage("[red]El nombre debe tener al menos 3 caracteres[/]")
                            .Validate(name => 
                            {
                                return !string.IsNullOrWhiteSpace(name) && name.Length >= 3 
                                    ? ValidationResult.Success() 
                                    : ValidationResult.Error();
                            }));
                    
                    // Solicitar edad
                    int edad = AnsiConsole.Prompt(
                        new TextPrompt<int>("[bold]Edad[/] (entre 18 y 99): ")
                            .PromptStyle("green")
                            .ValidationErrorMessage("[red]La edad debe ser un número entre 18 y 99[/]")
                            .Validate(age => 
                            {
                                return age >= 18 && age <= 99 
                                    ? ValidationResult.Success() 
                                    : ValidationResult.Error();
                            }));
                    
                    // Solicitar género
                    var generoOpciones = new Dictionary<string, int>
                    {
                        { "Masculino", 1 },
                        { "Femenino", 2 }
                    };
                    
                    string generoSeleccionado = AnsiConsole.Prompt(
                        new SelectionPrompt<string>()
                            .Title("[bold]Selecciona tu género:[/]")
                            .PageSize(5)
                            .HighlightStyle(new Style(foreground: Color.HotPink))
                            .AddChoices(generoOpciones.Keys));
                    
                    int genero = generoOpciones[generoSeleccionado];


                    // Solicitar intereses
                    string intereses = AnsiConsole.Prompt(
                        new TextPrompt<string>("[bold]Intereses[/] (mínimo 5 caracteres): ")
                            .PromptStyle("green")
                            .ValidationErrorMessage("[red]Los intereses deben tener al menos 5 caracteres[/]")
                            .Validate(interest => 
                            {
                                return !string.IsNullOrWhiteSpace(interest) && interest.Length >= 5 
                                    ? ValidationResult.Success() 
                                    : ValidationResult.Error();
                            }));
                    
                    // Solicitar carrera
                    string carrera = AnsiConsole.Prompt(
                        new TextPrompt<string>("[bold]Carrera[/] (mínimo 3 caracteres): ")
                            .PromptStyle("green")
                            .ValidationErrorMessage("[red]La carrera debe tener al menos 3 caracteres[/]")
                            .Validate(career => 
                            {
                                return !string.IsNullOrWhiteSpace(career) && career.Length >= 3 
                                    ? ValidationResult.Success() 
                                    : ValidationResult.Error();
                            }));
                    
                    // Solicitar frase
                    string frase = AnsiConsole.Prompt(
                        new TextPrompt<string>("[bold]Frase[/] (mínimo 5 caracteres): ")
                            .PromptStyle("green")
                            .ValidationErrorMessage("[red]La frase debe tener al menos 5 caracteres[/]")
                            .Validate(phrase => 
                            {
                                return !string.IsNullOrWhiteSpace(phrase) && phrase.Length >= 5 
                                    ? ValidationResult.Success() 
                                    : ValidationResult.Error();
                            }));

                    // Obtener lista de ciudades
                    var ciudades = new Dictionary<string, int>();
                    using (var conn = _dbFactory.CreateConnection())
                    {
                        conn.Open();
                        var cmd = new MySqlCommand(
                            @"SELECT c.id, CONCAT(c.nombre, ' (', d.nombre, ')') as nombre_completo 
                            FROM ciudades c 
                            JOIN departamentos d ON c.departamento_id = d.id 
                            ORDER BY d.nombre, c.nombre", 
                            (MySqlConnection)conn);

                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                ciudades.Add(reader["nombre_completo"].ToString(), Convert.ToInt32(reader["id"]));
                            }
                        }
                    }
                    
                    // Solicitar ciudad con un selector
                    AnsiConsole.WriteLine();
                    AnsiConsole.MarkupLine("[bold]Selecciona tu ciudad:[/]");
                    
                    string ciudadSeleccionada = AnsiConsole.Prompt(
                        new SelectionPrompt<string>()
                            .Title("[bold]Ciudades disponibles:[/]")
                            .PageSize(10)
                            .HighlightStyle(new Style(foreground: Color.Aqua))
                            .AddChoices(ciudades.Keys));
                    
                    int ciudadId = ciudades[ciudadSeleccionada];

                    // Solicitar correo
                    string correo = AnsiConsole.Prompt(
                        new TextPrompt<string>("[bold]Correo electrónico:[/] ")
                            .PromptStyle("green")
                            .ValidationErrorMessage("[red]El correo debe contener @ y .[/]")
                            .Validate(email => 
                            {
                                return !string.IsNullOrWhiteSpace(email) && email.Contains("@") && email.Contains(".") 
                                    ? ValidationResult.Success() 
                                    : ValidationResult.Error();
                            }));
                    
                    // Solicitar contraseña
                    string contrasena = AnsiConsole.Prompt(
                        new TextPrompt<string>("[bold]Contraseña[/] (mínimo 6 caracteres): ")
                            .PromptStyle("green")
                            .Secret()
                            .ValidationErrorMessage("[red]La contraseña debe tener al menos 6 caracteres[/]")
                            .Validate(pass => 
                            {
                                return !string.IsNullOrWhiteSpace(pass) && pass.Length >= 6 
                                    ? ValidationResult.Success() 
                                    : ValidationResult.Error();
                            }));

                    // Mostrar un spinner mientras se registra el usuario
                    await AnsiConsole.Status()
                        .StartAsync("Registrando usuario...", async ctx => 
                        {
                            ctx.Spinner(Spinner.Known.Star);
                            ctx.SpinnerStyle(Style.Parse("green"));
                            
                            // Registrar el usuario
                            await _usuarioService.RegistrarUsuario(nombre, edad, genero, intereses, carrera, frase, ciudadId);
                            
                            using (var conn = _dbFactory.CreateConnection())
                            {
                                conn.Open();
                                var cmd = new MySqlCommand("SELECT LAST_INSERT_ID()", (MySqlConnection)conn);
                                int nuevoUsuarioId = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                                
                                bool loginRegistrado = await _authService.RegistrarLogin(nuevoUsuarioId, correo, contrasena);
                                if (loginRegistrado)
                                {
                                    _currentUserId = nuevoUsuarioId;
                                    return true;
                                }
                                return false;
                            }
                        });
                    
                    if (_currentUserId > 0)
                    {
                        // Mostrar mensaje de éxito
                        var successPanel = new Panel("[bold green]¡Usuario registrado exitosamente![/]");
                        successPanel.Border = BoxBorder.Rounded;
                        successPanel.Padding = new Padding(2, 1, 2, 1);
                        successPanel.BorderColor(Color.Green);
                        AnsiConsole.Write(successPanel);
                        
                        AnsiConsole.MarkupLine("\nPresione cualquier tecla para continuar.");
                        Console.ReadKey();
                        return;
                    }
                    else
                    {
                        // Mostrar mensaje de error
                        var errorPanel = new Panel("[bold red]Error al registrar las credenciales de login.[/]");
                        errorPanel.Border = BoxBorder.Rounded;
                        errorPanel.Padding = new Padding(2, 1, 2, 1);
                        errorPanel.BorderColor(Color.Red);
                        AnsiConsole.Write(errorPanel);
                        
                        // Preguntar si desea intentar de nuevo
                        var opcion = AnsiConsole.Prompt(
                            new SelectionPrompt<string>()
                                .Title("¿Qué deseas hacer?")
                                .PageSize(3)
                                .AddChoices(new[] { "Intentar de nuevo", "Volver al menú principal" }));
                        
                        if (opcion == "Volver al menú principal")
                            return;
                    }
                }
                catch (Exception ex)
                {
                    // Mostrar mensaje de error
                    var errorPanel = new Panel($"[bold red]Error al registrar usuario: {ex.Message}[/]");
                    errorPanel.Border = BoxBorder.Rounded;
                    errorPanel.Padding = new Padding(2, 1, 2, 1);
                    errorPanel.BorderColor(Color.Red);
                    AnsiConsole.Write(errorPanel);
                    
                    // Preguntar si desea intentar de nuevo
                    var opcion = AnsiConsole.Prompt(
                        new SelectionPrompt<string>()
                            .Title("¿Qué deseas hacer?")
                            .PageSize(3)
                            .AddChoices(new[] { "Intentar de nuevo", "Volver al menú principal" }));
                    
                    if (opcion == "Volver al menú principal")
                        return;
                }
            }
        }

        private async Task VerPerfiles()
        {
            Console.Clear();
            
            // Crear título con Spectre.Console
            AnsiConsole.Write(
                new FigletText("Perfiles")
                    .Centered()
                    .Color(Color.Green));
            
            AnsiConsole.WriteLine();
            
            // Crear un panel informativo
            var panel = new Panel("[bold]Encuentra tu match ideal[/]");
            panel.Border = BoxBorder.Rounded;
            panel.Padding = new Padding(2, 1, 2, 1);
            panel.BorderColor(Color.Green);
            AnsiConsole.Write(panel);
            
            AnsiConsole.WriteLine();
            
            // Cargar perfiles con un spinner
            var perfiles = await AnsiConsole.Status()
                .StartAsync("Cargando perfiles...", async ctx => 
                {
                    ctx.Spinner(Spinner.Known.Star);
                    ctx.SpinnerStyle(Style.Parse("green"));
                    
                    // Obtener perfiles disponibles
                    return await _usuarioService.ObtenerPerfiles(_currentUserId);
                });
            
            if (perfiles.Count == 0)
            {
                var noProfilesPanel = new Panel("[bold yellow]No hay más perfiles disponibles por el momento. ¡Vuelve más tarde![/]");
                noProfilesPanel.Border = BoxBorder.Rounded;
                noProfilesPanel.Padding = new Padding(2, 1, 2, 1);
                noProfilesPanel.BorderColor(Color.Yellow);
                AnsiConsole.Write(noProfilesPanel);
                
                AnsiConsole.MarkupLine("\nPresione cualquier tecla para continuar.");
                Console.ReadKey();
                return;
            }
            
            // Iniciar navegación de perfiles
            int indiceActual = 0;
            bool continuarNavegacion = true;
            int targetId = -1;
            bool leGusto = false;
            
            while (continuarNavegacion)
            {
                Console.Clear();
                
                var perfilActual = perfiles[indiceActual];
                
                // Mostrar título
                AnsiConsole.Write(
                    new FigletText("Perfil")
                        .Centered()
                        .Color(Color.Green));
                
                // Crear tarjeta de perfil
                var perfilPanel = new Panel(
                    $"[bold]Nombre:[/] [green]{perfilActual.Nombre}[/]\n" +
                    $"[bold]Edad:[/] [green]{perfilActual.Edad}[/]\n" +
                    $"[bold]Carrera:[/] [green]{perfilActual.Carrera}[/]\n" +
                    $"[bold]Ciudad:[/] [green]{perfilActual.Ciudad}[/]\n" +
                    $"[bold]Intereses:[/] [green]{perfilActual.Intereses}[/]\n\n" +
                    $"[bold italic]\"{perfilActual.Frase}\"[/]");
                
                perfilPanel.Border = BoxBorder.Double;
                perfilPanel.Padding = new Padding(2, 1, 2, 1);
                
                // Mostrar un indicador si ya se ha interactuado con este perfil
                string headerText = $"[bold]Perfil ID: {perfilActual.Id}[/]";
                if (perfilActual.YaInteractuado)
                {
                    headerText += " [yellow](Ya has interactuado con este perfil)[/]";
                    perfilPanel.BorderColor(Color.Yellow);
                }
                else
                {
                    perfilPanel.BorderColor(Color.Green);
                }
                
                perfilPanel.Header = new PanelHeader(headerText);
                AnsiConsole.Write(perfilPanel);
                
                // Mostrar contador de perfiles
                AnsiConsole.MarkupLine($"\n[grey]Perfil {indiceActual + 1} de {perfiles.Count}[/]");
                
                // Opciones de navegación
                var opciones = new List<string>();
                
                if (indiceActual > 0)
                    opciones.Add("⬅️ Anterior");
                
                opciones.Add("❤️ Me gusta");
                opciones.Add("👎 No me gusta");
                
                if (indiceActual < perfiles.Count - 1)
                    opciones.Add("➡️ Siguiente");
                
                opciones.Add("🚪 Salir");
                
                var opcion = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[bold]¿Qué deseas hacer?[/]")
                        .PageSize(5)
                        .HighlightStyle(new Style(foreground: Color.Green))
                        .AddChoices(opciones));
                
                // Procesar opción seleccionada
                if (opcion.Contains("Anterior"))
                {
                    indiceActual--;
                }
                else if (opcion.Contains("Siguiente"))
                {
                    indiceActual++;
                }
                else if (opcion.Contains("Me gusta"))
                {
                    targetId = perfilActual.Id;
                    leGusto = true;
                    continuarNavegacion = false;
                }
                else if (opcion.Contains("No me gusta"))
                {
                    targetId = perfilActual.Id;
                    leGusto = false;
                    continuarNavegacion = false;
                }
                else if (opcion.Contains("Salir"))
                {
                    return;
                }
            }
            
            // Si llegamos aquí, el usuario ha seleccionado un perfil para dar like o dislike
            if (targetId > 0)
            {
                // Obtener información de likes disponibles antes de la acción
                var likesInfo = leGusto ? await _interaccionService.ObtenerLikesInfo(_currentUserId) : (0, 0);
                bool interaccionExitosa = true;
                
                // Mostrar un spinner mientras se registra la interacción
                await AnsiConsole.Status()
                    .StartAsync("Registrando interacción...", async ctx => 
                    {
                        ctx.Spinner(Spinner.Known.Dots);
                        ctx.SpinnerStyle(Style.Parse("green"));
                        
                        // Registrar la interacción (ahora devuelve un valor booleano)
                        interaccionExitosa = await _interaccionService.RegistrarInteraccion(_currentUserId, targetId, leGusto);
                        
                        if (interaccionExitosa)
                        {
                            // Verificar coincidencia si le gustó
                            if (leGusto)
                            {
                                await _interaccionService.VerificarCoincidencia(_currentUserId, targetId);
                            }
                            
                            // Actualizar estadísticas
                            await _estadisticaService.ActualizarEstadisticas(_currentUserId);
                            await _estadisticaService.ActualizarEstadisticas(targetId);
                        }
                    });
                
                // Si no se pudo registrar la interacción por límite de likes
                if (leGusto && !interaccionExitosa)
                {
                    // Mostrar mensaje de error por límite alcanzado
                    var limitPanel = new Panel(
                        $"[bold red]¡Has alcanzado tu límite diario de likes ({likesInfo.Item2})![/]\n" +
                        "[yellow]Vuelve mañana para seguir dando likes a nuevos perfiles.[/]");
                    limitPanel.Border = BoxBorder.Rounded;
                    limitPanel.Padding = new Padding(2, 1, 2, 1);
                    limitPanel.BorderColor(Color.Red);
                    AnsiConsole.Write(limitPanel);
                    
                    // Preguntar si desea seguir viendo perfiles o volver al menú principal
                    var opcionContinuarSinLikes = AnsiConsole.Prompt(
                        new SelectionPrompt<string>()
                            .Title("[bold]¿Qué deseas hacer ahora?[/]")
                            .PageSize(3)
                            .HighlightStyle(new Style(foreground: Color.Blue))
                            .AddChoices(new[] { "Seguir viendo perfiles 🔎", "Volver al menú principal 🔙" }));
                    
                    if (opcionContinuarSinLikes.Contains("Seguir"))
                    {
                        // Volver a llamar a la función para seguir viendo perfiles
                        await VerPerfiles();
                    }
                    
                    return;
                }
                
                // Mostrar mensaje de éxito
                var successPanel = new Panel(leGusto 
                    ? "[bold green]¡Has dado Like! Si hay coincidencia, aparecerá en tu lista de matches[/]"
                    : "[bold yellow]Has dado Dislike. Seguiremos buscando perfiles para ti[/]");
                successPanel.Border = BoxBorder.Rounded;
                successPanel.Padding = new Padding(2, 1, 2, 1);
                successPanel.BorderColor(leGusto ? Color.Green : Color.Yellow);
                AnsiConsole.Write(successPanel);
                
                // Preguntar si desea seguir viendo perfiles o volver al menú principal
                var opcionContinuar = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[bold]¿Qué deseas hacer ahora?[/]")
                        .PageSize(3)
                        .HighlightStyle(new Style(foreground: Color.Blue))
                        .AddChoices(new[] { "Seguir viendo perfiles 🔎", "Volver al menú principal 🔙" }));
                
                if (opcionContinuar.Contains("Seguir"))
                {
                    // Volver a llamar a la función para seguir viendo perfiles
                    await VerPerfiles();
                }
            }
        }

        private async Task VerCoincidencias()
        {
            Console.Clear();
            
            // Crear título con Spectre.Console
            AnsiConsole.Write(
                new FigletText("Matches")
                    .Centered()
                    .Color(Color.HotPink));
            
            AnsiConsole.WriteLine();
            
            // Crear un panel informativo
            var panel = new Panel("[bold]Tus coincidencias de amor[/]");
            panel.Border = BoxBorder.Rounded;
            panel.Padding = new Padding(2, 1, 2, 1);
            panel.BorderColor(Color.HotPink);
            AnsiConsole.Write(panel);
            
            AnsiConsole.WriteLine();
            
            // Mostrar coincidencias con un spinner mientras se cargan
            var coincidencias = new List<(int id, string nombre)>();
            
            await AnsiConsole.Status()
                .StartAsync("Buscando tus coincidencias...", async ctx => 
                {
                    ctx.Spinner(Spinner.Known.Hearts);
                    ctx.SpinnerStyle(Style.Parse("hotpink"));
                    
                    using (var conn = _dbFactory.CreateConnection())
                    {
                        conn.Open();
                        var cmd = new MySqlCommand(
                            @"SELECT c.id, 
                                CASE 
                                    WHEN c.usuario1_id = @currentUserId THEN u2.nombre
                                    ELSE u1.nombre
                                END as nombre_match
                            FROM coincidencias c 
                            JOIN usuarios u1 ON c.usuario1_id = u1.id 
                            JOIN usuarios u2 ON c.usuario2_id = u2.id 
                            WHERE c.usuario1_id = @currentUserId 
                            OR c.usuario2_id = @currentUserId", 
                            (MySqlConnection)conn);
                        
                        cmd.Parameters.AddWithValue("@currentUserId", _currentUserId);
                        
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                coincidencias.Add((
                                    Convert.ToInt32(reader["id"]),
                                    reader["nombre_match"].ToString()
                                ));
                            }
                        }
                    }
                });
            
            // Mostrar resultados en una tabla
            if (coincidencias.Count > 0)
            {
                var table = new Table();
                table.Border(TableBorder.Rounded);
                table.Expand();
                
                // Añadir columnas
                table.AddColumn(new TableColumn("[bold]ID[/]").Centered());
                table.AddColumn(new TableColumn("[bold]Nombre[/]").Centered());
                table.AddColumn(new TableColumn("[bold]Estado[/]").Centered());
                
                // Añadir filas
                foreach (var (id, nombre) in coincidencias)
                {
                    table.AddRow(
                        $"[yellow]{id}[/]",
                        $"[green]{nombre}[/]",
                        $"[hotpink]❤️ Match ❤️[/]");
                }
                
                AnsiConsole.Write(table);
            }
            else
            {
                // Mostrar mensaje si no hay coincidencias
                var noMatchesPanel = new Panel("[bold yellow]Aún no tienes coincidencias. ¡Sigue dando likes para encontrar tu match![/]");
                noMatchesPanel.Border = BoxBorder.Rounded;
                noMatchesPanel.Padding = new Padding(2, 1, 2, 1);
                noMatchesPanel.BorderColor(Color.Yellow);
                AnsiConsole.Write(noMatchesPanel);
            }
            
            AnsiConsole.MarkupLine("\nPresione cualquier tecla para continuar.");
            Console.ReadKey();
        }

        private async Task VerEstadisticas()
        {
            Console.Clear();
            
            // Crear título con Spectre.Console
            AnsiConsole.Write(
                new FigletText("Estadisticas")
                    .Centered()
                    .Color(Color.Blue));
            
            AnsiConsole.WriteLine();
            
            // Crear un panel informativo
            var panel = new Panel("[bold]Conoce las estadísticas del sistema[/]");
            panel.Border = BoxBorder.Rounded;
            panel.Padding = new Padding(2, 1, 2, 1);
            panel.BorderColor(Color.Blue);
            AnsiConsole.Write(panel);
            
            AnsiConsole.WriteLine();
            
            // Obtener datos de estadísticas
            var topUsuarios = await AnsiConsole.Status()
                .StartAsync("Cargando estadísticas...", async ctx => 
                {
                    ctx.Spinner(Spinner.Known.Star);
                    ctx.SpinnerStyle(Style.Parse("blue"));
                    
                    // Obtener estadísticas
                    return await _estadisticaService.ObtenerUsuariosConMasLikes();
                });
            
            var estadisticasGenerales = await _estadisticaService.ObtenerEstadisticasGenerales();
            
            // Mostrar estadísticas generales en un panel
            var statsPanel = new Panel(
                $"[bold]Total Usuarios:[/] [green]{estadisticasGenerales["TotalUsuarios"]:N0}[/]\n" +
                $"[bold]Total Likes:[/] [red]{estadisticasGenerales["TotalLikes"]:N0}[/]\n" +
                $"[bold]Total Matches:[/] [hotpink]{estadisticasGenerales["TotalMatches"]:N0}[/]\n" +
                $"[bold]Usuarios Masculinos:[/] [blue]{estadisticasGenerales["TotalHombres"]:N0}[/]\n" +
                $"[bold]Usuarios Femeninos:[/] [magenta]{estadisticasGenerales["TotalMujeres"]:N0}[/]"
            );
            statsPanel.Border = BoxBorder.Double;
            statsPanel.Padding = new Padding(2, 1, 2, 1);
            statsPanel.Header = new PanelHeader("[bold]Estadísticas Generales[/]");
            statsPanel.BorderColor(Color.Blue);
            AnsiConsole.Write(statsPanel);
            
            AnsiConsole.WriteLine();
            
            // Mostrar top usuarios en una tabla
            if (topUsuarios.Any())
            {
                AnsiConsole.MarkupLine("[bold blue]Top Usuarios con más Likes[/]");
                
                var table = new Table();
                table.Border(TableBorder.Rounded);
                table.Expand();
                
                // Añadir columnas
                table.AddColumn(new TableColumn("[bold]Nombre[/]"));
                table.AddColumn(new TableColumn("[bold]Likes[/]").Centered());
                table.AddColumn(new TableColumn("[bold]Matches[/]").Centered());
                table.AddColumn(new TableColumn("[bold]% Éxito[/]").Centered());
                
                // Añadir filas
                foreach (var usuario in topUsuarios)
                {
                    table.AddRow(
                        $"[green]{usuario.Nombre}[/]",
                        $"[red]{usuario.LikesRecibidos:N0}[/]",
                        $"[hotpink]{usuario.Matches:N0}[/]",
                        $"[blue]{usuario.PorcentajeExito:N1}%[/]");
                }
                
                AnsiConsole.Write(table);
            }
            
            AnsiConsole.MarkupLine("\nPresione cualquier tecla para continuar.");
            Console.ReadKey();
        }

        private async Task SeleccionarEstrategiaEmparejamiento()
        {
            Console.Clear();
            
            // Crear título con Spectre.Console
            AnsiConsole.Write(
                new FigletText("Estrategias")
                    .Centered()
                    .Color(Color.Yellow));
            
            AnsiConsole.WriteLine();
            
            // Obtener las estrategias disponibles
            var estrategias = _usuarioService.ObtenerEstrategiasDisponibles();
            var estrategiaActual = _usuarioService.ObtenerEstrategiaActual();
            
            // Mostrar información sobre la estrategia actual
            var panelActual = new Panel($"[bold]¡Encuentra tu match perfecto![/]\n\n[blue]Estrategia actual:[/] [green]{estrategiaActual.Nombre}[/]\n[yellow]{estrategiaActual.Descripcion}[/]");
            panelActual.Border = BoxBorder.Rounded;
            panelActual.Padding = new Padding(2, 1, 2, 1);
            panelActual.BorderColor(Color.Yellow);
            AnsiConsole.Write(panelActual);
            
            AnsiConsole.WriteLine();
            
            // Crear una lista de opciones con las estrategias disponibles
            var opcionesEstrategias = estrategias.Select(e => $"{e.Nombre}: {e.Descripcion}").ToList();
            opcionesEstrategias.Add("Volver al menú principal");
            
            // Crear una selección con Spectre.Console
            var opcion = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[bold]¿Qué estrategia de emparejamiento prefieres?[/]")
                    .PageSize(5)
                    .HighlightStyle(new Style(foreground: Color.Yellow))
                    .AddChoices(opcionesEstrategias));
            
            // Si el usuario no elige volver al menú principal, cambiar la estrategia
            if (!opcion.Contains("Volver"))
            {
                // Encontrar el índice de la estrategia seleccionada
                int indiceSeleccionado = opcionesEstrategias.IndexOf(opcion);
                
                if (indiceSeleccionado >= 0 && indiceSeleccionado < estrategias.Count)
                {
                    // Cambiar la estrategia
                    _usuarioService.CambiarEstrategiaPorIndice(indiceSeleccionado);
                    
                    // Mostrar mensaje de confirmación
                    AnsiConsole.MarkupLine("\n[bold green]¡Estrategia actualizada con éxito![/]");
                    AnsiConsole.MarkupLine($"[yellow]Ahora los perfiles se mostrarán según: {estrategias[indiceSeleccionado].Nombre}[/]");
                    AnsiConsole.MarkupLine("\nPresione cualquier tecla para continuar.");
                    Console.ReadKey();
                }
            }
            
            // Volver al menú principal
            await MostrarMenuPrincipal();
        }
        
        public async Task MostrarMenuAdministrador()
        {
            bool volver = false;
            
            while (!volver)
            {
                Console.Clear();
                
                // Crear título con Spectre.Console
                AnsiConsole.Write(
                    new FigletText("Panel Admin")
                        .Centered()
                        .Color(Color.Red));
                
                AnsiConsole.WriteLine();
                
                // Crear un panel con información
                var panelAdmin = new Panel($"[bold]Acceso administrativo - Usuario ID: {_currentUserId}[/]");
                panelAdmin.Border = BoxBorder.Double;
                panelAdmin.Padding = new Padding(2, 1, 2, 1);
                panelAdmin.BorderColor(Color.Red);
                AnsiConsole.Write(panelAdmin);
                
                AnsiConsole.WriteLine();
                
                // Crear una selección con Spectre.Console
                var opcion = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[bold]Panel de Administración[/]")
                        .PageSize(12)
                        .HighlightStyle(new Style(foreground: Color.Red))
                        .AddChoices(new[]
                        {
                            "1. Gestionar Usuarios",
                            "2. Gestionar Interacciones",
                            "3. Gestionar Coincidencias",
                            "4. Gestionar Ciudades",
                            "5. Gestionar Departamentos",
                            "6. Gestionar Géneros",
                            "7. Cambiar Usuario (Modo Multicliente)",
                            "8. Volver al Menú Principal"
                        }));
                
                // Extraer el número de la opción
                string seleccion = opcion.Split('.')[0].Trim();
                
                switch (seleccion)
                {
                    case "1":
                        await GestionarUsuarios();
                        break;
                    case "2":
                        await GestionarInteracciones();
                        break;
                    case "3":
                        await GestionarCoincidencias();
                        break;
                    case "4":
                        await GestionarCiudades();
                        break;
                    case "5":
                        await GestionarDepartamentos();
                        break;
                    case "6":
                        await GestionarGeneros();
                        break;
                    case "7":
                        await ModoMulticliente();
                        break;
                    case "8":
                        volver = true;
                        break;
                    default:
                        AnsiConsole.MarkupLine("[red]Opción no válida. Presione cualquier tecla para continuar.[/]");
                        Console.ReadKey();
                        break;
                }
            }
        }

        public async Task ModoMulticliente()
        {
            Console.Clear();
            
            AnsiConsole.Write(
                new FigletText("Cambiar Usuario")
                    .Centered()
                    .Color(Color.Yellow));
            
            AnsiConsole.WriteLine();
            
            var panel = new Panel("[bold]Modo Multicliente Ficticio[/]");
            panel.Border = BoxBorder.Rounded;
            panel.Padding = new Padding(2, 1, 2, 1);
            panel.BorderColor(Color.Yellow);
            AnsiConsole.Write(panel);
            
            AnsiConsole.WriteLine();
            
            // Mostrar lista de usuarios para elegir
            AnsiConsole.WriteLine("\n[bold]Usuarios disponibles:[/]");
            
            // Obtener lista de usuarios
            var usuarios = await ObtenerListaUsuarios();
            
            if (usuarios.Count > 0)
            {
                var opcionesUsuarios = usuarios.Select(u => $"{u.Id}. {u.Nombre}").ToList();
                opcionesUsuarios.Add("Ingresar ID manualmente");
                opcionesUsuarios.Add("Volver");
                
                var opcion = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[bold]Seleccione un usuario:[/]")
                        .PageSize(15)
                        .HighlightStyle(new Style(foreground: Color.Yellow))
                        .AddChoices(opcionesUsuarios));
                
                if (opcion == "Volver")
                {
                    return;
                }
                else if (opcion == "Ingresar ID manualmente")
                {
                    var userId = AnsiConsole.Prompt(
                        new TextPrompt<int>("[yellow]Ingrese el ID del usuario a simular:[/]")
                            .PromptStyle("yellow")
                            .ValidationErrorMessage("[red]Por favor ingrese un número válido[/]"));
                    
                    _currentUserId = userId;
                }
                else
                {
                    string idStr = opcion.Split('.')[0].Trim();
                    if (int.TryParse(idStr, out int userId))
                    {
                        _currentUserId = userId;
                    }
                }
                
                AnsiConsole.MarkupLine($"\n[green]Simulando como usuario ID: {_currentUserId}[/]");
                AnsiConsole.WriteLine("\nPresione cualquier tecla para continuar.");
                Console.ReadKey();
            }
            else
            {
                AnsiConsole.MarkupLine("[red]No hay usuarios disponibles.[/]");
                AnsiConsole.WriteLine("Presione cualquier tecla para continuar.");
                Console.ReadKey();
            }
        }

        private async Task MostrarCiudadesDisponibles()
        {
            Console.WriteLine("\nCiudades disponibles:");
            Console.WriteLine("--------------------");
            using (var conn = _dbFactory.CreateConnection())
            {
                conn.Open();
                var cmd = new MySqlCommand(
                    @"SELECT c.id, c.nombre, d.nombre as departamento 
                    FROM ciudades c 
                    JOIN departamentos d ON c.departamento_id = d.id 
                    ORDER BY d.nombre, c.nombre", 
                    (MySqlConnection)conn);

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    string departamentoActual = "";
                    while (await reader.ReadAsync())
                    {
                        string departamento = reader["departamento"].ToString();
                        if (departamento != departamentoActual)
                        {
                            Console.WriteLine($"\n{departamento}:");
                            departamentoActual = departamento;
                        }
                        Console.WriteLine($"  [{reader["id"]}] {reader["nombre"]}");
                    }
                }
            }
            Console.WriteLine("--------------------\n");
        }

        // Métodos para gestionar las tablas desde el menú de administrador
        
        private async Task<List<(int Id, string Nombre)>> ObtenerListaUsuarios()
        {
            var usuarios = new List<(int Id, string Nombre)>();
            
            using (var conn = _dbFactory.CreateConnection())
            {
                conn.Open();
                var cmd = new MySqlCommand(
                    @"SELECT id, nombre FROM usuarios ORDER BY id", 
                    (MySqlConnection)conn);
                
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        int id = reader.GetInt32(0);
                        string nombre = reader.GetString(1);
                        usuarios.Add((id, nombre));
                    }
                }
            }
            
            return usuarios;
        }

        public async Task GestionarUsuarios()
        {
            bool volver = false;
            
            while (!volver)
            {
                Console.Clear();
                
                // Crear título con Spectre.Console
                AnsiConsole.Write(
                    new FigletText("Gestión de Usuarios")
                        .Centered()
                        .Color(Color.Blue));
                
                AnsiConsole.WriteLine();
                
                // Crear tabla con usuarios
                var table = new Table();
                table.Border = TableBorder.Rounded;
                table.Expand();
                
                // Añadir columnas
                table.AddColumn(new TableColumn("[b]ID[/]").Centered());
                table.AddColumn(new TableColumn("[b]Nombre[/]").Centered());
                table.AddColumn(new TableColumn("[b]Edad[/]").Centered());
                table.AddColumn(new TableColumn("[b]Género[/]").Centered());
                table.AddColumn(new TableColumn("[b]Ciudad[/]").Centered());
                table.AddColumn(new TableColumn("[b]Carrera[/]").Centered());
                
                // Obtener datos de usuarios para la tabla
                using (var conn = _dbFactory.CreateConnection())
                {
                    conn.Open();
                    var cmd = new MySqlCommand(
                        @"SELECT u.id, u.nombre, u.edad, g.descripcion as genero, c.nombre as ciudad, u.carrera 
                        FROM usuarios u 
                        LEFT JOIN generos g ON u.genero = g.id 
                        LEFT JOIN ciudades c ON u.ciudad_id = c.id 
                        ORDER BY u.id", 
                        (MySqlConnection)conn);
                    
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            table.AddRow(
                                reader["id"].ToString(),
                                reader["nombre"].ToString() ?? "-",
                                reader["edad"].ToString() ?? "-",
                                reader["genero"].ToString() ?? "-",
                                reader["ciudad"].ToString() ?? "-",
                                reader["carrera"].ToString() ?? "-"
                            );
                        }
                    }
                }
                
                AnsiConsole.Write(table);
                AnsiConsole.WriteLine();
                
                // Mostrar opciones de gestión
                var opcion = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[bold]¿Qué deseas hacer?[/]")
                        .PageSize(10)
                        .HighlightStyle(new Style(foreground: Color.Blue))
                        .AddChoices(new[]
                        {
                            "1. Añadir Usuario",
                            "2. Editar Usuario",
                            "3. Eliminar Usuario",
                            "4. Ver Detalles de Usuario",
                            "5. Volver al Menú Administrador"
                        }));
                
                string seleccion = opcion.Split('.')[0].Trim();
                
                switch (seleccion)
                {
                    case "1":
                        await _adminUIHelper.AñadirUsuario();
                        break;
                    case "2":
                        await _adminUIHelper.EditarUsuario();
                        break;
                    case "3":
                        await _adminUIHelper.EliminarUsuario();
                        break;
                    case "4":
                        await _adminUIHelper.VerDetallesUsuario();
                        break;
                    case "5":
                        volver = true;
                        break;
                    default:
                        AnsiConsole.MarkupLine("[red]Opción no válida.[/]");
                        Console.ReadKey();
                        break;
                }
            }
        }

        public async Task GestionarInteracciones()
        {
            bool volver = false;
            
            while (!volver)
            {
                Console.Clear();
                
                // Crear título con Spectre.Console
                AnsiConsole.Write(
                    new FigletText("Gestión de Interacciones")
                        .Centered()
                        .Color(Color.Yellow));
                
                AnsiConsole.WriteLine();
                
                // Crear tabla con interacciones
                var table = new Table();
                table.Border = TableBorder.Rounded;
                table.Expand();
                
                // Añadir columnas
                table.AddColumn(new TableColumn("[b]ID[/]").Centered());
                table.AddColumn(new TableColumn("[b]Usuario Emisor[/]").Centered());
                table.AddColumn(new TableColumn("[b]Usuario Receptor[/]").Centered());
                table.AddColumn(new TableColumn("[b]Tipo[/]").Centered());
                table.AddColumn(new TableColumn("[b]Fecha[/]").Centered());
                
                // Obtener datos de interacciones para la tabla
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
                        ORDER BY i.fecha_interaccion DESC LIMIT 50", 
                        (MySqlConnection)conn);
                    
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            table.AddRow(
                                reader["id"].ToString(),
                                reader["emisor"].ToString(),
                                reader["receptor"].ToString(),
                                reader["tipo"].ToString(),
                                reader["fecha"].ToString()
                            );
                        }
                    }
                }
                
                AnsiConsole.Write(table);
                AnsiConsole.WriteLine();
                
                // Mostrar opciones de gestión
                var opcion = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[bold]¿Qué deseas hacer?[/]")
                        .PageSize(8)
                        .HighlightStyle(new Style(foreground: Color.Yellow))
                        .AddChoices(new[]
                        {
                            "1. Añadir Interacción",
                            "2. Eliminar Interacción",
                            "3. Ver Más Interacciones",
                            "4. Volver al Menú Administrador"
                        }));
                
                string seleccion = opcion.Split('.')[0].Trim();
                
                switch (seleccion)
                {
                    case "1":
                        await _adminUIHelper.AñadirInteraccion();
                        break;
                    case "2":
                        await _adminUIHelper.EliminarInteraccion();
                        break;
                    case "3":
                        await _adminUIHelper.VerMasInteracciones();
                        break;
                    case "4":
                        volver = true;
                        break;
                    default:
                        AnsiConsole.MarkupLine("[red]Opción no válida.[/]");
                        Console.ReadKey();
                        break;
                }
            }
        }

        public async Task GestionarCoincidencias()
        {
            bool volver = false;
            
            while (!volver)
            {
                Console.Clear();
                
                // Crear título con Spectre.Console
                AnsiConsole.Write(
                    new FigletText("Gestión de Matches")
                        .Centered()
                        .Color(Color.HotPink));
                
                AnsiConsole.WriteLine();
                
                // Crear tabla con coincidencias
                var table = new Table();
                table.Border = TableBorder.Rounded;
                table.Expand();
                
                // Añadir columnas
                table.AddColumn(new TableColumn("[b]ID[/]").Centered());
                table.AddColumn(new TableColumn("[b]Usuario 1[/]").Centered());
                table.AddColumn(new TableColumn("[b]Usuario 2[/]").Centered());
                table.AddColumn(new TableColumn("[b]Fecha[/]").Centered());
                
                // Obtener datos de coincidencias para la tabla
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
                            table.AddRow(
                                reader["id"].ToString(),
                                reader["usuario1"].ToString(),
                                reader["usuario2"].ToString(),
                                reader["fecha"].ToString()
                            );
                        }
                    }
                }
                
                AnsiConsole.Write(table);
                AnsiConsole.WriteLine();
                
                // Mostrar opciones de gestión
                var opcion = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[bold]¿Qué deseas hacer?[/]")
                        .PageSize(5)
                        .HighlightStyle(new Style(foreground: Color.HotPink))
                        .AddChoices(new[]
                        {
                            "1. Eliminar Coincidencia",
                            "2. Volver al Menú Administrador"
                        }));
                
                string seleccion = opcion.Split('.')[0].Trim();
                
                switch (seleccion)
                {
                    case "1":
                        await _adminUIHelper.EliminarCoincidencia();
                        break;
                    case "2":
                        volver = true;
                        break;
                    default:
                        AnsiConsole.MarkupLine("[red]Opción no válida.[/]");
                        Console.ReadKey();
                        break;
                }
            }
        }

        public async Task GestionarCiudades()
        {
            bool volver = false;
            
            while (!volver)
            {
                Console.Clear();
                
                // Crear título con Spectre.Console
                AnsiConsole.Write(
                    new FigletText("Gestión de Ciudades")
                        .Centered()
                        .Color(Color.Green));
                
                AnsiConsole.WriteLine();
                
                // Crear tabla con ciudades
                var table = new Table();
                table.Border = TableBorder.Rounded;
                table.Expand();
                
                // Añadir columnas
                table.AddColumn(new TableColumn("[b]ID[/]").Centered());
                table.AddColumn(new TableColumn("[b]Nombre[/]").Centered());
                table.AddColumn(new TableColumn("[b]Departamento[/]").Centered());
                
                // Obtener datos de ciudades para la tabla
                using (var conn = _dbFactory.CreateConnection())
                {
                    conn.Open();
                    var cmd = new MySqlCommand(
                        @"SELECT c.id, c.nombre, d.nombre as departamento 
                        FROM ciudades c 
                        JOIN departamentos d ON c.departamento_id = d.id 
                        ORDER BY d.nombre, c.nombre", 
                        (MySqlConnection)conn);
                    
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            table.AddRow(
                                reader["id"].ToString(),
                                reader["nombre"].ToString(),
                                reader["departamento"].ToString()
                            );
                        }
                    }
                }
                
                AnsiConsole.Write(table);
                AnsiConsole.WriteLine();
                
                // Mostrar opciones de gestión
                var opcion = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[bold]¿Qué deseas hacer?[/]")
                        .PageSize(8)
                        .HighlightStyle(new Style(foreground: Color.Green))
                        .AddChoices(new[]
                        {
                            "1. Añadir Ciudad",
                            "2. Editar Ciudad",
                            "3. Eliminar Ciudad",
                            "4. Volver al Menú Administrador"
                        }));
                
                string seleccion = opcion.Split('.')[0].Trim();
                
                switch (seleccion)
                {
                    case "1":
                        await _adminUIHelper.AñadirCiudad();
                        break;
                    case "2":
                        await _adminUIHelper.EditarCiudad();
                        break;
                    case "3":
                        await _adminUIHelper.EliminarCiudad();
                        break;
                    case "4":
                        volver = true;
                        break;
                    default:
                        AnsiConsole.MarkupLine("[red]Opción no válida.[/]");
                        Console.ReadKey();
                        break;
                }
            }
        }

        public async Task GestionarDepartamentos()
        {
            bool volver = false;
            
            while (!volver)
            {
                Console.Clear();
                
                // Crear título con Spectre.Console
                AnsiConsole.Write(
                    new FigletText("Gestión de Departamentos")
                        .Centered()
                        .Color(Color.Aqua));
                
                AnsiConsole.WriteLine();
                
                // Crear tabla con departamentos
                var table = new Table();
                table.Border = TableBorder.Rounded;
                table.Expand();
                
                // Añadir columnas
                table.AddColumn(new TableColumn("[b]ID[/]").Centered());
                table.AddColumn(new TableColumn("[b]Nombre[/]").Centered());
                table.AddColumn(new TableColumn("[b]País[/]").Centered());
                
                // Obtener datos de departamentos para la tabla
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
                            table.AddRow(
                                reader["id"].ToString(),
                                reader["nombre"].ToString(),
                                reader["pais"].ToString()
                            );
                        }
                    }
                }
                
                AnsiConsole.Write(table);
                AnsiConsole.WriteLine();
                
                // Mostrar opciones de gestión
                var opcion = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[bold]¿Qué deseas hacer?[/]")
                        .PageSize(8)
                        .HighlightStyle(new Style(foreground: Color.Aqua))
                        .AddChoices(new[]
                        {
                            "1. Añadir Departamento",
                            "2. Editar Departamento",
                            "3. Eliminar Departamento",
                            "4. Volver al Menú Administrador"
                        }));
                
                string seleccion = opcion.Split('.')[0].Trim();
                
                switch (seleccion)
                {
                    case "1":
                        await _adminUIHelper.AñadirDepartamento();
                        break;
                    case "2":
                        await _adminUIHelper.EditarDepartamento();
                        break;
                    case "3":
                        await _adminUIHelper.EliminarDepartamento();
                        break;
                    case "4":
                        volver = true;
                        break;
                    default:
                        AnsiConsole.MarkupLine("[red]Opción no válida.[/]");
                        Console.ReadKey();
                        break;
                }
            }
        }

        public async Task GestionarGeneros()
        {
            bool volver = false;
            
            while (!volver)
            {
                Console.Clear();
                
                // Crear título con Spectre.Console
                AnsiConsole.Write(
                    new FigletText("Gestión de Géneros")
                        .Centered()
                        .Color(Color.Purple));
                
                AnsiConsole.WriteLine();
                
                // Crear tabla con géneros
                var table = new Table();
                table.Border = TableBorder.Rounded;
                table.Expand();
                
                // Añadir columnas
                table.AddColumn(new TableColumn("[b]ID[/]").Centered());
                table.AddColumn(new TableColumn("[b]Nombre[/]").Centered());
                
                // Obtener datos de géneros para la tabla
                using (var conn = _dbFactory.CreateConnection())
                {
                    conn.Open();
                    var cmd = new MySqlCommand(
                        @"SELECT id, descripcion as nombre FROM generos ORDER BY id", 
                        (MySqlConnection)conn);
                    
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            table.AddRow(
                                reader["id"].ToString(),
                                reader["nombre"].ToString()
                            );
                        }
                    }
                }
                
                AnsiConsole.Write(table);
                AnsiConsole.WriteLine();
                
                // Mostrar opciones de gestión
                var opcion = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[bold]¿Qué deseas hacer?[/]")
                        .PageSize(8)
                        .HighlightStyle(new Style(foreground: Color.Purple))
                        .AddChoices(new[]
                        {
                            "1. Añadir Género",
                            "2. Editar Género",
                            "3. Eliminar Género",
                            "4. Volver al Menú Administrador"
                        }));
                
                string seleccion = opcion.Split('.')[0].Trim();
                
                switch (seleccion)
                {
                    case "1":
                        await _adminUIHelper.AñadirGenero();
                        break;
                    case "2":
                        await _adminUIHelper.EditarGenero();
                        break;
                    case "3":
                        await _adminUIHelper.EliminarGenero();
                        break;
                    case "4":
                        volver = true;
                        break;
                    default:
                        AnsiConsole.MarkupLine("[red]Opción no válida.[/]");
                        Console.ReadKey();
                        break;
                }
            }
        }
    }
} 