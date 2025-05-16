using System.ComponentModel.DataAnnotations;

namespace APBD_Kolos1A.Models.DTOs;

public class AppointmentDto
{
    [Required]
    public DateTime Date { get; set; }
    [Required]
    public PatientDto Patient { get; set; }
    [Required]
    public DoctorDto Doctor{ get; set; }
    [Required]
    public List<ServiceDto> AppointmentServices { get; set; }
}