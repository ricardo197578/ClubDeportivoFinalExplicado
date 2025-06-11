using System;
using System.Collections.Generic;

namespace ClubManagement
{
    public class Socio : Persona
    {
        public Socio()
        {
            
            HistorialCuotas = new List<Cuota>();
            ActividadesInscritas = new List<Actividad>();
        }

        
        public int NroSocio { get; set; }
        public DateTime FechaInscripcion { get; set; }
        public AptoFisico AptoFisico { get; set; }
        public bool EstadoActivo { get; set; }
        public DateTime FechaVencimientoCuota { get; set; }
        public List<Cuota> HistorialCuotas { get; set; }
        public List<Actividad> ActividadesInscritas { get; set; }
        public TipoMembresia Membresia { get; set; }

        public void PagarCuota(Cuota cuota)
        {
            if (cuota == null)
                throw new ArgumentNullException("cuota");

            if (HistorialCuotas == null)
                HistorialCuotas = new List<Cuota>();

            HistorialCuotas.Add(cuota);
            FechaVencimientoCuota = cuota.FechaVencimiento;
        }

        public void RenovarMembresia()
        {
            EstadoActivo = true;
            FechaVencimientoCuota = DateTime.Now.AddMonths(1);
        }

        public override string ObtenerEstadoMembresia()
        {
            if (Membresia == null)
            {
                return "Sin membresía asignada";
            }
            else
            {
                return Membresia.Nombre + " - " + (EstadoActivo ? "Activa" : "Inactiva");
            }
        }


    }
}
