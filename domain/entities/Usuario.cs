using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace campusLove.domain.entities
{
    public class Usuario
    {
    public int Id { get; set; }
    public string Nombre { get; set; }
    public int Edad { get; set; }
    public int Genero { get; set; }
    public string Intereses { get; set; }
    public string Carrera { get; set; }
    public string Frase { get; set; }
    public int LikesDiarios { get; set; }
    public int MaxLikesDiarios { get; set; }
    public int CiudadId { get; set; }
    }
}