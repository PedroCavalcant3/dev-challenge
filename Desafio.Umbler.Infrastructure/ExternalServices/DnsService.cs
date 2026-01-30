using Desafio.Umbler.Application.Interfaces;

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
        var result = await _lookup.QueryAsync(domainName, DnsClient.QueryType.A, DnsClient.QueryClass.IN, ct);
        var record = result.Answers.ARecords().FirstOrDefault();

        return new DnsLookupResult(
            record?.Address?.ToString(),
            record?.TimeToLive ?? 0
        );
    }
}
