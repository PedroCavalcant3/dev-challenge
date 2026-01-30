using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Desafio.Umbler.Application.Exceptions;

public class InvalidDomainException : Exception
{
    public InvalidDomainException(string message) : base(message) { }
}
