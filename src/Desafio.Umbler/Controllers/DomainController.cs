using Desafio.Umbler.Application.Exceptions;
using Desafio.Umbler.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Desafio.Umbler.Controllers
{
    public record ApiError(string Code, string Message);

    [Route("api")]
    public class DomainController : Controller
    {
        private readonly IDomainService _service;

        public DomainController(IDomainService service)
        {
            _service = service;
        }

        [HttpGet, Route("domain/{domainName}")]
        public async Task<IActionResult> Get(string domainName, CancellationToken ct)
        {
            try
            {
                var dto = await _service.GetAsync(domainName, ct);
                return Ok(dto);
            }
            catch (InvalidDomainException ex)
            {
                return BadRequest(new ApiError("invalid_domain", ex.Message));
            }
            catch (ExternalLookupException ex)
            {
                // DNS/WHOIS indisponível e timeout
                return StatusCode(503, new ApiError("external_service_unavailable", ex.Message));
            }
            catch (Exception)
            {
                // Catch genérico para impedir vazamento de detalhes internos
                return StatusCode(500, new ApiError("unexpected_error", "Ocorreu um erro inesperado."));
            }
        }
    }
}
