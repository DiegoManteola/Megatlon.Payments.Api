namespace Megatlon.Payments.Domain.Entities
{
    public class Pago
    {
        /// <summary>
        /// Id único de pago
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// ID de Cliente que hizo la compra
        /// </summary>
        public Guid ClienteId { get; set; }

        /// <summary>
        /// Cliente que hizo la compra
        /// </summary>
        public required Cliente Cliente { get; set; }

        /// <summary>
        /// Monto de Transacción
        /// </summary>
        public decimal Monto { get; set; }

        /// <summary>
        /// Fecha de pago
        /// </summary>
        public DateTime FechaPago { get; set; }

        /// <summary>
        /// Id de medio de pago utilizado
        /// </summary>
        public int MedioPagoId { get; set; }

        /// <summary>
        /// Medio de pago utilizado
        /// </summary>
        public required MedioPago MedioPago { get; set; }

        /// <summary>
        /// Id de moneda utilizada
        /// </summary>
        public int MonedaId { get; set; }

        /// <summary>
        /// Moneda utilizada
        /// </summary>
        public required Moneda Moneda { get; set; }

        /// <summary>
        /// Origen del evento (sistema emisor).
        /// </summary>
        public required string Source { get; set; }

        /// <summary>
        /// Referencia externa única dentro de un Source.
        /// </summary>
        public required string ExternalReference { get; set; }
    }
}
