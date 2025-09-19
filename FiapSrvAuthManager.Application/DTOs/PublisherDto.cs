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
public class PublisherDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public DateTime BornDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public UserRole Role { get; set; }
    public string PublisherName { get; set; }
    public string Cnpj { get; set; }
    public string CompanyName { get; set; }

    public PublisherDto(Publisher publisher)
    {
        Id = publisher.Id;
        Name = publisher.Name;
        Username = publisher.PublisherName;
        Email = publisher.Email;
        BornDate = publisher.BornDate;
        CreatedAt = publisher.CreatedAt;
        Role = publisher.Role;
        PublisherName = publisher.PublisherName;
        Cnpj = publisher.Cnpj;
        CompanyName = publisher.CompanyName;
    }
}
