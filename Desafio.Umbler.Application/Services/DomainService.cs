using Desafio.Umbler.Application.DTOs;
using Desafio.Umbler.Application.Interfaces;
using Desafio.Umbler.Domain.Entities;

namespace Desafio.Umbler.Application.Services;

public class DomainService : IDomainService
{
    private readonly IDomainDataAccess _data;
    private readonly IDnsService _dns;
    private readonly IWhoisService _whois;

    public DomainService(IDomainDataAccess data, IDnsService dns, IWhoisService whois)
    {
        _data = data;
        _dns = dns;
        _whois = whois;
    }

    public async Task<DomainInfoDto> GetAsync(string domainName, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(domainName) || !domainName.Contains('.'))
            throw new ArgumentException("Nome de domínio inválido");

        var domain = await _data.GetByNameAsync(domainName, ct);

        if (domain == null)
        {
            domain = await CreateAsync(domainName, ct);
            await _data.AddAsync(domain, ct);
            await _data.SaveChangesAsync(ct);
        }
        else if (IsExpired(domain))
        {
            await RefreshAsync(domain, domainName, ct);
            await _data.SaveChangesAsync(ct);
        }

        return new DomainInfoDto(domain.Name, domain.Ip, domain.HostedAt, domain.WhoIs);
    }

    private static bool IsExpired(DomainInfo domain)
    {
        var ageSeconds = (DateTime.UtcNow - domain.UpdatedAt).TotalSeconds;
        return domain.Ttl > 0 && ageSeconds > domain.Ttl;
    }

    private async Task<DomainInfo> CreateAsync(string domainName, CancellationToken ct)
    {
        var whoisDomain = await _whois.QueryAsync(domainName, ct);
        var dns = await _dns.GetARecordAsync(domainName, ct);

        if (string.IsNullOrWhiteSpace(dns.Ip))
            throw new InvalidOperationException("Nenhum registro encontrado para este domínio.");

        var whoisIp = await _whois.QueryAsync(dns.Ip, ct);

        return new DomainInfo
        {
            Name = domainName,
            Ip = dns.Ip,
            UpdatedAt = DateTime.UtcNow,
            WhoIs = whoisDomain.Raw,
            Ttl = dns.TtlSeconds > int.MaxValue
            ? int.MaxValue
            : (int)dns.TtlSeconds,
            HostedAt = whoisIp.OrganizationName
        };
    }

    private async Task RefreshAsync(DomainInfo domain, string domainName, CancellationToken ct)
    {
        var whoisDomain = await _whois.QueryAsync(domainName, ct);
        var dns = await _dns.GetARecordAsync(domainName, ct);

        if (string.IsNullOrWhiteSpace(dns.Ip))
            throw new InvalidOperationException("Nenhum registro encontrado para este domínio.");

        var whoisIp = await _whois.QueryAsync(dns.Ip, ct);

        domain.Ip = dns.Ip;
        domain.UpdatedAt = DateTime.UtcNow;
        domain.WhoIs = whoisDomain.Raw;
        domain.Ttl = dns.TtlSeconds > int.MaxValue
            ? int.MaxValue
            : (int)dns.TtlSeconds;
        domain.HostedAt = whoisIp.OrganizationName;
    }
}

