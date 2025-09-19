using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FiapSrvAuthManager.Domain.Entities;
using FiapSrvAuthManager.Domain.Enums;

namespace FiapSrvAuthManager.Application.DTOs;

[ExcludeFromCodeCoverage]
public class PlayerDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public DateTime BornDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public UserRole Role { get; set; }
    public string Cpf { get; set; }

    public PlayerDto (Player player)
    {
        Id = player.Id;
        Name = player.Name;
        Username = player.Username;
        Email = player.Email;
        BornDate = player.BornDate;
        CreatedAt = player.CreatedAt;
        Role = player.Role;
        Cpf = player.Cpf;
    }
}
