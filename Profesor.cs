using System;
using System.Collections.Generic;

namespace ClubManagement
{
    public class Profesor : Persona
    {
        public string Legajo { get; set; }
        public DateTime FechaContratacion { get; set; }
        public bool EsTitular { get; set; }
        public List<Actividad> Actividades { get; set; }

       

        // Implementaci�n espec�fica para Profesor futura implementacion
        public override string ObtenerEstadoMembresia()
        {
            return EsTitular ? "Profesor Titular" : "Profesor Suplente";
        }

       
    }
}
