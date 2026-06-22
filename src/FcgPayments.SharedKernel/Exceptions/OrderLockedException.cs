namespace FcgPayments.SharedKernel.Exceptions;

public sealed class OrderLockedException(string message) : BusinessException(message);
