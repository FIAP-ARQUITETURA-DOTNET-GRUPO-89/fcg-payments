namespace FcgPayments.SharedKernel.Exceptions;

public sealed class MissingAddressException(string message) : BusinessException(message);
