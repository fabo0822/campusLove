namespace campusLove.domain.models
{
    public class EstadisticaUsuario
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public int LikesRecibidos { get; set; }
        public int Matches { get; set; }
        public double PorcentajeExito => Matches > 0 && LikesRecibidos > 0 
            ? (double)Matches / LikesRecibidos * 100 
            : 0;
    }
} 