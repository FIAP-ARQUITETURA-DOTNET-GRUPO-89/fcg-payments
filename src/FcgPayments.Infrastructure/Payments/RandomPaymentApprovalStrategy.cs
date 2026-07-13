using FcgPayments.Domain.Services;
using FcgPayments.SharedKernel.Settings;
using Microsoft.Extensions.Options;

namespace FcgPayments.Infrastructure.Payments;

/// <summary>
/// Estratégia de aprovação aleatória de pagamento. A probabilidade de aprovação
/// é configurada via <see cref="PaymentSimulationSettings.ApprovalRate"/>.
/// </summary>
public sealed class RandomPaymentApprovalStrategy(IOptions<PaymentSimulationSettings> settings) : IPaymentApprovalStrategy
{
    private readonly double _approvalRate = settings.Value.ApprovalRate;
#pragma warning disable CA5394 // simulação não-criptográfica
    private static readonly Random Random = Random.Shared;
#pragma warning restore CA5394

    public bool ShouldApprove(decimal amount)
    {
        if (amount <= 0)
        {
            return false;
        }

        if (_approvalRate >= 1.0)
        {
            return true;
        }

        if (_approvalRate <= 0.0)
        {
            return false;
        }

        return Random.NextDouble() < _approvalRate;
    }
}
