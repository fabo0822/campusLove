using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace campusLove.domain.entities
{
    public class Coincidencia
    {
    public int Id { get; set; }
    public int Usuario1Id { get; set; }
    public int Usuario2Id { get; set; }
    public DateTime FechaCoincidencia { get; set; }
    
    }
}