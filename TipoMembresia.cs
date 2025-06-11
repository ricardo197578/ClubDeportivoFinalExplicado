using System;

namespace ClubManagement
{
    public abstract class TipoMembresia
    {
        private decimal _precioMensual;
        private int _diasGraciaVencimiento;
        private string _beneficios;

        public abstract string Nombre { get; }

        public virtual decimal PrecioMensual
        {
            get { return _precioMensual; }
            set { _precioMensual = value; }
        }

        public virtual int DiasGraciaVencimiento
        {
            get { return _diasGraciaVencimiento; }
            set { _diasGraciaVencimiento = value; }
        }

        public virtual string Beneficios
        {
            get { return _beneficios; }
            set { _beneficios = value; }
        }

        public override string ToString()
        {
            return string.Format("{0} - ${1}/mes", Nombre, PrecioMensual);
        }
    }

    public class MembresiaStandard : TipoMembresia
    {
        public override string Nombre
        {
            get { return "Standard"; }
        }

        public MembresiaStandard()
        {
            // Valores por defecto
            PrecioMensual = 1500.00m;
            DiasGraciaVencimiento = 7;
            Beneficios = "Acceso a instalaciones básicas";
        }
    }

    public class MembresiaPremium : TipoMembresia
    {
        public override string Nombre
        {
            get { return "Premium"; }
        }

        public MembresiaPremium()
        {
            // Valores por defecto
            PrecioMensual = 2500.00m;
            DiasGraciaVencimiento = 15;
            Beneficios = "Acceso a todas las instalaciones + 2 invitados mensuales";
        }
    }

    public class MembresiaFamiliar : TipoMembresia
    {
        public override string Nombre
        {
            get { return "Familiar"; }
        }

        public MembresiaFamiliar()
        {
            // Valores por defecto
            PrecioMensual = 3500.00m;
            DiasGraciaVencimiento = 10;
            Beneficios = "Acceso familiar (hasta 4 personas)";
        }
    }
}