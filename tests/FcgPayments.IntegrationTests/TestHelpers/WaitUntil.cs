namespace FcgPayments.IntegrationTests.TestHelpers;

public static class WaitUntil
{
    /// <summary>
    /// Executa um polling assíncrono aguardando até que a condição do predicado seja satisfeita ou que o tempo limite de timeout seja atingido.
    /// </summary>
    /// <typeparam name="T">O tipo do dado que está sendo consultado e avaliado.</typeparam>
    /// <param name="fetchAction">A função assíncrona responsável por buscar o dado atualizado (ex: uma consulta ao banco de dados).</param>
    /// <param name="predicate">A condição/validação que o dado retornado precisa satisfazer para interromper a espera.</param>
    /// <param name="timeout">O tempo máximo total permitido para que a condição seja atendida antes de encerrar o polling.</param>
    /// <param name="delayMs">O intervalo de tempo em milissegundos de espera entre cada tentativa de busca. O padrão é 500ms.</param>
    /// <returns>
    /// Retorna o último resultado obtido da <paramref name="fetchAction"/>. 
    /// Se a condição foi satisfeita, retorna o valor esperado; caso contrário, retorna o estado final do dado após o timeout.
    /// </returns>
    public static async Task<T?> ForAsync<T>(
        Func<Task<T?>> fetchAction,
        Func<T?, bool> predicate,
        TimeSpan timeout,
        int delayMs = 500)
    {
        var deadline = DateTime.UtcNow.Add(timeout);

        while (DateTime.UtcNow < deadline)
        {
            var result = await fetchAction();

            if (predicate(result))
            {
                return result;
            }

            await Task.Delay(delayMs);
        }

        return await fetchAction();
    }
}
