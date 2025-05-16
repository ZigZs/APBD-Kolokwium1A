using System.ComponentModel.DataAnnotations;

namespace APBD_Kolos1A.Models.DTOs;

public class AppointmentPostDto
{
    [Required]
    public int AppointmentId { get; set; }
    
    [Required]
    public int PatientId { get; set; }
    
    [Required]
    public string PWZ { get; set; }
    
    [Required]
    public List<ServicePostDto> Services { get; set; }
}