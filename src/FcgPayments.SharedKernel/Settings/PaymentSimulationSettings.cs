using System.ComponentModel.DataAnnotations;

namespace FcgPayments.SharedKernel.Settings;

/// <summary>
/// Configurações da simulação de pagamento utilizada pelo PaymentsAPI.
/// </summary>
public record PaymentSimulationSettings
{
    /// <summary>
    /// Taxa de aprovação (0.0 a 1.0). Cada pagamento processado é aprovado
    /// com essa probabilidade. Default 1.0 (sempre aprova).
    /// </summary>
    [Range(0.0, 1.0)]
    public double ApprovalRate { get; init; } = 1.0;
}
