namespace FcgPayments.SharedKernel.Exceptions;

public class InvalidPaymentException(string message) : BusinessException(message);
