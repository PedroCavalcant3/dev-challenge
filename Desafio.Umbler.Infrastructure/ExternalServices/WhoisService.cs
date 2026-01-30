using Desafio.Umbler.Application.Interfaces;
using Whois.NET;

namespace Desafio.Umbler.Infrastructure.ExternalServices;

public class WhoisService : IWhoisService
{
    public async Task<WhoisResult> QueryAsync(string query, CancellationToken ct = default)
    {
        var res = await WhoisClient.QueryAsync(query);

        return new WhoisResult(res.Raw, res.OrganizationName);
    }
}
