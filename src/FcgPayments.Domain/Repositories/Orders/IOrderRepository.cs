using FcgPayments.Domain.Entities;

namespace FcgPayments.Domain.Repositories.Orders;

public interface IOrderRepository
{
    /// <summary>
    /// Adiciona um novo pedido ao contexto.
    /// </summary>
    /// <param name="order">Pedido a ser adicionado.</param>
    void Add(Order order);

    /// <summary>
    /// Recupera um pedido específico através do seu identificador único.
    /// </summary>
    /// <param name="id">O identificador único do pedido.</param>
    /// <param name="cancellationToken">Token utilizado para cancelar a consulta.</param>
    /// <returns>
    /// A instância do pedido se encontrado; caso contrário, nulo.
    /// </returns>
    Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Recupera um pedido específico através do seu identificador único.
    /// </summary>
    /// <param name="id">O identificador único do pedido.</param>
    /// <param name="cancellationToken">Token utilizado para cancelar a consulta.</param>
    /// <returns>
    /// A instância do pedido se encontrado; caso contrário, nulo.
    /// </returns>
    Task<Order?> GetByIdAsNoTrackingAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém uma lista paginada de pedidos com base nos filtros informados.
    /// </summary>
    /// <param name="page">O número da página atual (índice baseado em 1).</param>
    /// <param name="pageSize">A quantidade máxima de registros a serem retornados por página.</param>
    /// <param name="customerId">O identificador opcional do cliente para filtrar pedidos específicos. Se nulo, retorna dados globais.</param>
    /// <param name="cancellationToken">Token utilizado para cancelar a consulta.</param>
    /// <returns>
    /// Uma tupla contendo: coleção de pedidos da página solicitada e quantidade total de registros encontrados antes da paginação.
    /// </returns>
    Task<(IReadOnlyCollection<Order> Items, int TotalCount)> GetPagedAsNoTrackingAsync(int page, int pageSize, string? customerId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Persiste no banco de dados todas as alterações pendentes no contexto atual.
    /// </summary>
    /// <returns>
    /// Quantidade de registros afetados.
    /// </returns>
    Task<int> SaveChangesAsync();
}
