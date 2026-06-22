namespace FcgPayments.SharedKernel.Exceptions;

public sealed class NotFoundException(string message) : BusinessException(message);
