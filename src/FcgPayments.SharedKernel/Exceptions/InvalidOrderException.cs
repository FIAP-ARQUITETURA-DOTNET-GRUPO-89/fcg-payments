namespace FcgPayments.SharedKernel.Exceptions;

public sealed class InvalidOrderException(string message): BusinessException(message);
