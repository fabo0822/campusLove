using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace campusLove.domain.entities
{
    public class Interaccion
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public Usuario Usuario { get; set; }
        public int ObjetivoUsuarioId { get; set; }
        public Usuario ObjetivoUsuario { get; set; }
        public bool LeGusto { get; set; }
        public DateTime FechaInteraccion { get; set; }
    }
}