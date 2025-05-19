using System;

namespace campusLove.domain.models
{
    /// <summary>
    /// Representa el perfil de un usuario que se muestra durante la navegación
    /// </summary>
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
}
