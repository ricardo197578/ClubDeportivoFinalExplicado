using System;

namespace ClubManagement
{
    public abstract class Persona
    {
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Dni { get; set; }
        public DateTime FechaNacimiento { get; set; }
        public string Direccion { get; set; }
        public string Telefono { get; set; }
        public string Email { get; set; }

        // M�todo abstracto (todos deben implementarlo)
        public abstract string ObtenerTipoMembresia();

        // M�todo virtual (pueden sobrescribirlo si necesitan comportamiento espec�fico)
        public virtual string ObtenerInformacionContacto()
        {
            return string.Format("Tel: {0} | Email: {1}", Telefono, Email);

        }

        // M�todo polim�rfico para calcular descuentos
        public virtual decimal CalcularDescuento(decimal montoOriginal)
        {
            return montoOriginal; // Por defecto no hay descuento
        }
    }
}

