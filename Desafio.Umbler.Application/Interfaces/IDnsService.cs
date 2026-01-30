namespace Desafio.Umbler.Application.Interfaces;

public interface IDnsService
{
    Task<DnsLookupResult> GetARecordAsync(string domainName, CancellationToken ct = default);
}

public record DnsLookupResult(string? Ip, long TtlSeconds);