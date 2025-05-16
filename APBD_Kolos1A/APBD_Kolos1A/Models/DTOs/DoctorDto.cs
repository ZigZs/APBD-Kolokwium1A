using System.ComponentModel.DataAnnotations;

namespace APBD_Kolos1A.Models.DTOs;

public class DoctorDto
{
    [Required]
    public int DoctorId { get; set; }
    
    [Required]
    public string PWZ { get; set; }
}