namespace FcgPayments.Domain.Services;

public interface IPaymentApprovalStrategy
{
    bool ShouldApprove(decimal amount);
}
