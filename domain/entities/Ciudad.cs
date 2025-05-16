using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace campusLove.domain.entities
{
    public class Ciudad
    {
        public int Id { get; set; }
        public string? Nombre { get; set; }
        public int DepartamentoId { get; set; }
        public Departamento? Departamento { get; set; }
    }
}