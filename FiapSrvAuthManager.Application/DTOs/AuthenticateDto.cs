using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FiapSrvAuthManager.Application.DTOs;

[ExcludeFromCodeCoverage]
public class AuthenticateDto
{
    public string Email { get; set; } = default!;
    public string Password { get; set; } = default!;
}
