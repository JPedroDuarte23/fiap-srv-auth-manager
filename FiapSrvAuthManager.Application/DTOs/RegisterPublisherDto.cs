using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FiapSrvAuthManager.Application.DTOs;

[ExcludeFromCodeCoverage]
public class RegisterPublisherDto
{
    [Required(ErrorMessage = "O nome é obrigatório.")]
    [StringLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres.")]
    public string Name { get; set; }

    [Required(ErrorMessage = "O nome da publicadora é obrigatório.")]
    [StringLength(100, ErrorMessage = "O nome da publicadora deve ter no máximo 100 caracteres.")]
    public string PublisherName { get; set; }

    [Required(ErrorMessage = "O nome da empresa é obrigatório.")]
    [StringLength(100, ErrorMessage = "O nome da empresa deve ter no máximo 100 caracteres.")]
    public string CompanyName { get; set; }

    [Required(ErrorMessage = "O nome de usuário é obrigatório.")]
    [StringLength(50, ErrorMessage = "O nome de usuário deve ter no máximo 50 caracteres.")]
    public string Username { get; set; }

    [Required(ErrorMessage = "O e-mail é obrigatório.")]
    [EmailAddress(ErrorMessage = "Favor informar um endereço de e-mail válido.")]
    [StringLength(100, ErrorMessage = "O e-mail deve ter no máximo 100 caracteres.")]
    public string Email { get; set; }

    [Required(ErrorMessage = "A senha é obrigatória.")]
    [MinLength(8, ErrorMessage = "A senha deve ter pelo menos 8 caracteres.")]
    [RegularExpression(@"^(?=.*[a-zA-Z])(?=.*\d)(?=.*[\W_]).+$",
        ErrorMessage = "A senha deve conter ao menos uma letra, um número e um caractere especial.")]
    public string Password { get; set; }

    [Required(ErrorMessage = "A data de nascimento é obrigatória.")]
    [DataType(DataType.Date, ErrorMessage = "Data de nascimento inválida.")]
    public DateTime BornDate { get; set; }

    [Required(ErrorMessage = "O CNPJ é obrigatório.")]
    [RegularExpression(@"^\d{14}$", ErrorMessage = "O CNPJ deve conter exatamente 14 dígitos numéricos.")]
    public string Cnpj { get; set; }
}
