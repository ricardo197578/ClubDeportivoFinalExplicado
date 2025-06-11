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

        public Profesor()
        {
            Actividades = new List<Actividad>();
        }

        // Implementaci�n espec�fica para Profesor
        public override string ObtenerTipoMembresia()
        {
            return EsTitular ? "Profesor Titular" : "Profesor Suplente";
        }

        // Profesores tienen acceso gratuito
        public override decimal CalcularDescuento(decimal montoOriginal)
        {
            return 0; // Acceso gratuito
        }
    }
}
