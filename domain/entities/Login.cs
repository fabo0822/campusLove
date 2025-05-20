using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace campusLove.domain.entities
{
    public class Login
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public Usuario Usuario { get; set; }
        public string Correo { get; set; }
        public string Contrasena { get; set; }
        public bool EsAdmin { get; set; } = false;
    }
}