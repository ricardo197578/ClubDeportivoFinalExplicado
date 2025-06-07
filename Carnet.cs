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
            if (Socio == null)
            {
                throw new InvalidOperationException("Debe asignar un socio primero");
            }

            if (Socio.NroSocio <= 0)
            {
                throw new InvalidOperationException("El socio debe estar guardado en BD primero");
            }

            FechaEmision = DateTime.Now;
            FechaVencimiento = DateTime.Now.AddYears(1);
            NroCarnet = Socio.NroSocio; // Usamos el mismo número de socio
        }

        public string NumeroCarnetFormateado
        {
            get { return NroCarnet.ToString().PadLeft(6, '0'); }
        }
    }
}
