using Desafio.Umbler.Domain.Entities;

namespace Desafio.Umbler.Application.Interfaces;

public interface IDomainDataAccess
{
    Task<DomainInfo?> GetByNameAsync(string domainName, CancellationToken ct = default);
    Task AddAsync(DomainInfo domain, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
