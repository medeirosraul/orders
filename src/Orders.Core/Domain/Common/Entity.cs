namespace Orders.Core.Domain.Common
{
    /// <summary>
    /// Classe abstrata base para todas as entidades.
    /// </summary>
    public abstract class Entity
    {
        /// <summary>
        /// Identificador.
        /// </summary>
        public virtual string Id { get; set; } = default!;

        /// <summary>
        /// Data de criação.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Data da última atualização.
        /// </summary>
        public DateTime ModifiedAt { get; set; }

        /// <summary>
        /// Entidade deletada virtualmente.
        /// </summary>
        public bool Deleted { get; set; }

        public Entity()
        {
            Id = Guid.NewGuid().ToString();
            var now = DateTime.UtcNow;
            CreatedAt = now;
            ModifiedAt = now;
            Deleted = false;
        }
    }
}