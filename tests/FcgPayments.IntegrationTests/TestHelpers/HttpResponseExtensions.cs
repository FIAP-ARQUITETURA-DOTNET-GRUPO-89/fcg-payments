using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace FcgPayments.IntegrationTests.TestHelpers;

/// <summary>
/// Extensões para facilitar a leitura e interpretação de respostas HTTP nos testes de integração.
/// </summary>
public static class HttpResponseExtensions
{
    /// <summary>
    /// Lê o conteúdo da resposta HTTP e desserializa para o tipo especificado.
    /// </summary>
    public static async Task<T?> ReadContentAsync<T>(this HttpResponseMessage response, CancellationToken cancellationToken)
        => await response.Content.ReadFromJsonAsync<T>(cancellationToken);

    /// <summary>
    /// Lê o conteúdo da resposta HTTP e desserializa para ProblemDetails.
    /// </summary>
    public static async Task<ProblemDetails> ReadProblemDetailsAsync(this HttpResponseMessage response, CancellationToken cancellationToken)
        => (await response.Content.ReadFromJsonAsync<ProblemDetails>(cancellationToken))!;

    /// <summary>
    /// Extrai os erros de validação a partir de uma resposta no formato ProblemDetails.
    /// </summary>
    public static async Task<Dictionary<string, string[]>> ReadValidationErrorsAsync(this HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var problem = await response.ReadProblemDetailsAsync(cancellationToken);

        if (problem?.Extensions.TryGetValue("errors", out var errorsObj) == true && errorsObj is not null)
        {
            var element = (JsonElement)errorsObj;
            return JsonSerializer.Deserialize<Dictionary<string, string[]>>(element.GetRawText()) ?? [];
        }

        return [];
    }
}
