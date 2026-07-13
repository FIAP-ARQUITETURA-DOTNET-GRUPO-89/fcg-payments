namespace FcgPayments.Domain.Services;

public interface IPaymentApprovalStrategy
{
    /// <summary>
    /// Determina se um pagamento deve ser aprovado com base no valor e na estratégia configurada.
    /// </summary>
    /// <param name="amount">Valor do pagamento a ser avaliado.</param>
    /// <returns><c>true</c> se o pagamento deve ser aprovado; <c>false</c> caso contrário.</returns>
    bool ShouldApprove(decimal amount);
}
