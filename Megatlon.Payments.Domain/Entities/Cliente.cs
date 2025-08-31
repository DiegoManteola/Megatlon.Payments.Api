namespace Megatlon.Payments.Domain.Entities
{
    public class Cliente
    {
        /// <summary>
        /// Id único de Cliente
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Nombre de cliente
        /// </summary>
        public required string Nombre { get; set; }

        /// <summary>
        /// Email del cliente
        /// </summary>
        public required string Email { get; set; }

        /// <summary>
        /// Fecha de alta, es decir, cuando ocurrió la primer transacción con dicho cliente.
        /// </summary>
        public DateTime FechaAlta { get; set; }
    }
}
