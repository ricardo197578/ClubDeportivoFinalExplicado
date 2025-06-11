using System;
using System.Collections.Generic;

namespace ClubManagement
{
    public class NoSocio : Persona
    {
        public DateTime FechaRegistro { get; set; }
        public List<Actividad> Actividades { get; set; }

        public NoSocio()
        {
            Actividades = new List<Actividad>();
        }

        public void PagarActividad(Actividad actividad, MetodoPago metodo)
        {
            Actividades.Add(actividad);
        }

        // Implementación específica para NoSocio
        public override string ObtenerTipoMembresia()
        {
            return "Visitante";
        }

        // NoSocios pagan un recargo del 10%
        public override decimal CalcularDescuento(decimal montoOriginal)
        {
            return montoOriginal * 1.1m;
        }
    }
}
