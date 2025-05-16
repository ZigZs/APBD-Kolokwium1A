using System.ComponentModel.DataAnnotations;

namespace APBD_Kolos1A.Models.DTOs;

public class ServiceDto
{
    [Required]
    public string Name { get; set; }
    [Required]
    public decimal ServiceFee { get; set; }
}