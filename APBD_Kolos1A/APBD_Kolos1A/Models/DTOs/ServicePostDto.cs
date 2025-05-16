using System.ComponentModel.DataAnnotations;

namespace APBD_Kolos1A.Models.DTOs;

public class ServicePostDto
{
    [Required]
    public string ServiceName { get; set; }
    [Required]
    public decimal ServiceFee { get; set; }
}