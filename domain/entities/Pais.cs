using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace campusLove.domain.entities
{
    public class Pais
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public ICollection<Departamento> Departamentos { get; set; }
    }
}