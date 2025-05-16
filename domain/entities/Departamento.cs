using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace campusLove.domain.entities
{
    public class Departamento
    {
        public int Id { get; set; }
        public string? Nombre { get; set; }
        public int PaisId { get; set; }
        public string? Pais { get; set; }
        public ICollection<Ciudad> Ciudades { get; set; }
    }
}