using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Desafio.Umbler.Application.Exceptions;

public class ExternalLookupException : Exception
{
    public ExternalLookupException(string service, string? message = null, Exception? inner = null)
        : base(message ?? $"{service} O serviço está indisponível.", inner)
    {
        Service = service;
    }

    public string Service { get; }
}
