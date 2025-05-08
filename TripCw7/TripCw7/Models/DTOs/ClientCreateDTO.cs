using System.ComponentModel.DataAnnotations;

namespace TripCw7.Models.DTOs;

public class ClientCreateDTO
{
    [Required]
    [MaxLength(120)]
    public string FirstName { get; set; }

    [Required]
    [MaxLength(120)]
    public string LastName { get; set; }

    [Required]
    [MaxLength(120)]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    [RegularExpression(@"^\d{9}$", ErrorMessage = "Telefon musi składać się z dokładnie 9 cyfr.")]
    public string Telephone { get; set; }

    [Required]
    [RegularExpression(@"^\d{11}$", ErrorMessage = "Pesel musi składać się z dokładnie 11 cyfr.")]
    public string Pesel { get; set; }
}