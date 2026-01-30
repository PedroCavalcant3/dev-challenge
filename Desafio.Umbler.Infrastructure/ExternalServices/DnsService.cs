using Desafio.Umbler.Application.Exceptions;
using Desafio.Umbler.Application.Interfaces;
using DnsClient;

namespace Desafio.Umbler.Infrastructure.ExternalServices;

public class DnsService : IDnsService
{
    private readonly DnsClient.ILookupClient _lookup;

    public DnsService(DnsClient.ILookupClient lookup)
    {
        _lookup = lookup;
    }

    public async Task<DnsLookupResult> GetARecordAsync(string domainName, CancellationToken ct = default)
    {
        try
        {
            var result = await _lookup.QueryAsync(domainName, QueryType.A, QueryClass.IN, ct);
            var record = result.Answers.ARecords().FirstOrDefault();

            return new DnsLookupResult(
                record?.Address?.ToString(),
                record?.TimeToLive ?? 0
            );
        }
        catch (DnsResponseException ex)
        {
            throw new ExternalLookupException("DNS", "Falha ao consultar DNS.", ex);
        }
        catch (OperationCanceledException ex)
        {
            throw new ExternalLookupException("DNS", "Timeout ao consultar DNS.", ex);
        }
    }
}
