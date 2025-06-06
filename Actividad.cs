namespace ClubManagement
{
    public class Actividad
    {
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public string Horario { get; set; }
        public Profesor Profesor { get; set; }
        public double PrecioNoSocio { get; set; }
        public bool ExclusivaSocios { get; set; }
    }
}