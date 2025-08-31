namespace Megatlon.Payments.Domain.Entities
{
    public class Moneda
    {
        /// <summary>
        /// Id único de moneda (32 para arg, 840 para usd eeuu, etc)
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Nombre de la moneda (Pesos Argentinos, Dólares EEUU, etc)
        /// </summary>
        public required string Nombre { get; set; }

        /// <summary>
        /// Descripcion de la moneda
        /// </summary>
        public required string Descripcion { get; set; }

        /// <summary>
        /// Código ISO (ARS, USD, UYU)
        /// </summary>
        public required string ISOCode { get; set; }
    }
}
