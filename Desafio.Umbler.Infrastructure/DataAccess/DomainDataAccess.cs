using Desafio.Umbler.Application.Interfaces;
using Desafio.Umbler.Domain.Entities;
using Desafio.Umbler.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Desafio.Umbler.Infrastructure.DataAccess;

public class DomainDataAccess : IDomainDataAccess
{
    private readonly DatabaseContext _db;

    public DomainDataAccess(DatabaseContext db)
    {
        _db = db;
    }

    public Task<DomainInfo?> GetByNameAsync(string domainName, CancellationToken ct = default)
        => _db.Domains.FirstOrDefaultAsync(d => d.Name == domainName, ct);

    public async Task AddAsync(DomainInfo domain, CancellationToken ct = default)
        => await _db.Domains.AddAsync(domain, ct);

    public Task SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);
}
