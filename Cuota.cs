using System;

namespace ClubManagement
{
    public class Cuota
    {
        public double Monto { get; set; }
        public DateTime FechaPago { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public MetodoPago MetodoPago { get; set; }
        public int Cuotas { get; set; }
        public bool Pagada { get; set; }
    }
}