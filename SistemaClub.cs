using System;
using System.Collections.Generic;
using System.Linq;

namespace ClubManagement
{
    public class SistemaClub
    {
        private List<Socio> socios;
        private List<NoSocio> noSocios;
        private List<Profesor> profesores;
        private List<Actividad> actividades;

        public SistemaClub()
        {
            socios = new List<Socio>();
            noSocios = new List<NoSocio>();
            profesores = new List<Profesor>();
            actividades = new List<Actividad>();
        }

        public List<Socio> Socios
        {
            get { return socios; }
            set { socios = value; }
        }

        public List<NoSocio> NoSocios
        {
            get { return noSocios; }
            set { noSocios = value; }
        }

        public List<Profesor> Profesores
        {
            get { return profesores; }
            set { profesores = value; }
        }

        public List<Actividad> Actividades
        {
            get { return actividades; }
            set { actividades = value; }
        }

        public void RegistrarSocio(Socio socio)
        {
            socios.Add(socio);
        }

        public void RegistrarNoSocio(NoSocio noSocio)
        {
            noSocios.Add(noSocio);
        }

        public List<Socio> GenerarListadoVencimientos(DateTime fecha)
        {
            List<Socio> vencidos = new List<Socio>();
            foreach (Socio s in socios)
            {
                if (s.FechaVencimientoCuota <= fecha)
                {
                    vencidos.Add(s);
                }
            }
            return vencidos;
        }

        public void RegistrarPago(Socio socio, Cuota cuota)
        {
            socio.PagarCuota(cuota);
        }

        public void AsignarProfesor(Actividad actividad, Profesor profesor)
        {
            actividad.Profesor = profesor;
            profesor.Actividades.Add(actividad);
        }
    }
}
