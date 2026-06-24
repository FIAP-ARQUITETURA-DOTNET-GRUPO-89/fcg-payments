using System.Text.RegularExpressions;
using FcgPayments.SharedKernel.Exceptions;

namespace FcgPayments.Domain.ValueObjects;

public sealed partial record Address
{
    public string Street { get; }
    public string City { get; }
    public string State { get; }
    public string Cep { get; }

    private Address()
    {
        Street = null!;
        City = null!;
        State = null!;
        Cep = null!;
    }

    public Address(string street, string city, string state, string cep)
    {
        if (string.IsNullOrWhiteSpace(street))
        {
            throw new InvalidAddressException("A rua é obrigatória.");
        }

        if (string.IsNullOrWhiteSpace(city))
        {
            throw new InvalidAddressException("A cidade é obrigatória.");
        }

        if (string.IsNullOrWhiteSpace(state))
        {
            throw new InvalidAddressException("O estado é obrigatório.");
        }

        if (string.IsNullOrWhiteSpace(cep))
        {
            throw new InvalidAddressException("O CEP é obrigatório.");
        }

        if (state.Trim().Length != 2)
        {
            throw new InvalidAddressException("O estado deve possuir 2 caracteres.");
        }

        var normalizedCep = CepNormalizationRegex().Replace(cep, "");

        if (normalizedCep.Length != 8)
        {
            throw new InvalidAddressException("O CEP deve conter 8 dígitos.");
        }

        Street = street.Trim();
        City = city.Trim();
        State = state.Trim().ToUpperInvariant();
        Cep = normalizedCep;
    }

    [GeneratedRegex(@"\D")]
    private static partial Regex CepNormalizationRegex();
}
