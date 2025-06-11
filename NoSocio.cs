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

        public override string ObtenerEstadoMembresia()
        {
            return "Visitante";
        }
    }
}
