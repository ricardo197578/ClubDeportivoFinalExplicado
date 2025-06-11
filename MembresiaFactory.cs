using System;

namespace ClubManagement
{
    public static class MembresiaFactory
    {
        public static TipoMembresia CrearMembresia(string nombre)
        {
            switch (nombre.ToLower()) // para hacerlo case-insensitive
            {
                case "standard":
                    return new MembresiaStandard();
                case "premium":
                    return new MembresiaPremium();
                case "familiar":
                    return new MembresiaFamiliar();
                default:
                    throw new ArgumentException(string.Format("Tipo de membresía '{0}' no válido", nombre));

            }
        }
    }
}