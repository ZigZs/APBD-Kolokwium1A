using APBD_Kolos1A.Models.DTOs;

namespace APBD_Kolos1A.Services;

public interface IAppointmentService
{
    Task<AppointmentDto> GetAppointment(int id);
    
    Task AddAppointment(AppointmentPostDto appointment);
}