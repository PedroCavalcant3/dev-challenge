namespace Desafio.Umbler.Application.Interfaces;

public interface IWhoisService
{
    Task<WhoisResult> QueryAsync(string query, CancellationToken ct = default);
}

public record WhoisResult(string Raw, string? OrganizationName);