namespace Megatlon.Payments.Domain.Entities
{
    public class MedioPago
    {
        /// <summary>
        /// Id de medio de pago
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Nombre de medio de pago (Efectivo, Cheque, Tarjeta)
        /// </summary>
        public required string Nombre { get; set; }

        /// <summary>
        /// Descripcion del medio de pago
        /// </summary>
        public required string Descripcion { get; set; }

        /// <summary>
        /// Código de medio de pago (EFEC, CHEQ, TARJ)
        /// </summary>
        public required string Code { get; set; }
    }
}
