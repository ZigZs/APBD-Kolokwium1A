using System.ComponentModel.DataAnnotations;

namespace APBD_Kolos1A.Models.DTOs;

public class PatientDto
{
    [Required]
    public string FirstName { get; set; }
    
    [Required]
    public string LastName { get; set; }
    
    [Required]
    public DateTime DateOfBirth { get; set; }
}