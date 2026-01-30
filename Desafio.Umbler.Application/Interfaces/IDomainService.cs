using Desafio.Umbler.Application.DTOs;

namespace Desafio.Umbler.Application.Interfaces;

public interface IDomainService
{
    Task<DomainInfoDto> GetAsync(string domainName, CancellationToken ct = default);
}
