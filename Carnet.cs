using System;

namespace ClubManagement
{
    public class Carnet
    {
        public int Id { get; set; }
        public int NroCarnet { get; set; }
        public DateTime FechaEmision { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public Socio Socio { get; set; }

        public void GenerarCarnet()
        {
            FechaEmision = DateTime.Now;
            FechaVencimiento = DateTime.Now.AddYears(1);
            NroCarnet = new Random().Next(100000, 999999);
        }
    }
}