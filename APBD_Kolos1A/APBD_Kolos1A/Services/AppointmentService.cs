using APBD_Kolos1A.Models.DTOs;
using System.Data;
using System.Data.Common;
using APBD_Kolos1A.Exceptions;
using APBD_Kolos1A.Services;
using Microsoft.Data.SqlClient;

namespace APBD_Kolos1A.Services;

public class AppointmentService : IAppointmentService
{
    private readonly String _connectionString;

    public AppointmentService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("Default") ?? string.Empty;
    }
    
    public async Task<AppointmentDto> GetAppointment(int id)
    {
        await using SqlConnection connection = new SqlConnection(_connectionString);
        await using SqlCommand command = new SqlCommand();
        
        command.Connection = connection;

        var query = @"SELECT a.date, p.first_name, p.last_name, p.date_of_birth, d.doctor_id, d.PWZ, s.name, s.base_fee
                    FROM Appointment a
                    join Patient p on p.patient_id = a.patient_id
                    join Doctor d on d.doctor_id = a.doctor_id
                    join Appointment_Service sa on sa.appoitment_id = a.appoitment_id
                    join Service s on s.service_id = sa.service_id
                    WHERE a.appoitment_id = @id;
                    ";
        
        command.CommandText = query;
        command.Parameters.AddWithValue("@id", id);
        await connection.OpenAsync();
        var answear = await command.ExecuteReaderAsync();

        AppointmentDto? appointment = null;
        List<ServiceDto> appointmentServices = new List<ServiceDto>();

        while (await answear.ReadAsync())
        {
            if (appointment == null)
            {
                appointment = new AppointmentDto
                {
                    Date = answear.GetDateTime(0),
                    Patient = new PatientDto()
                    {
                        FirstName = answear.GetString(1),
                        LastName = answear.GetString(2),
                        DateOfBirth = answear.GetDateTime(3)
                    },
                    Doctor = new DoctorDto()
                    {
                        DoctorId = answear.GetInt32(4),
                        PWZ = answear.GetString(5)
                    },
                    AppointmentServices = appointmentServices
                };
            }

            ServiceDto serviceDto = new ServiceDto();
            serviceDto.Name = answear.GetString(6);
            serviceDto.ServiceFee = answear.GetDecimal(7);
            appointmentServices.Add(serviceDto);
        }

        if (appointment == null)
        {
            throw new NotFoundException("Appointment not found");
        }
        return appointment;
    }

    public async Task AddAppointment(AppointmentPostDto appointment)
    {
        await using SqlConnection connection = new SqlConnection(_connectionString);
        await using SqlCommand command = new SqlCommand();
        
        command.Connection = connection;
        await connection.OpenAsync();
        
        DbTransaction transaction = await connection.BeginTransactionAsync();
        command.Transaction = transaction as SqlTransaction;
        
        try
        {
            await using (var checkCommand = new SqlCommand(
                             "SELECT 1 FROM Appointment WHERE appoitment_id = @AppoitmentId;", 
                             connection, 
                             (SqlTransaction)transaction))
            {
                checkCommand.Parameters.AddWithValue("@AppoitmentId", appointment.AppointmentId);
                if (await checkCommand.ExecuteScalarAsync() != null)
                {
                    throw new ConflictException($"Appointment with ID {appointment.AppointmentId} already exists");
                }
            }
            
            command.CommandText = @"SELECT patient_id
                                    FROM Patient WHERE patient_id = @IdPatient
                                    ;";
            command.Parameters.AddWithValue("@IdPatient", appointment.PatientId);
            var patient = await command.ExecuteScalarAsync();
            if (patient == null)
            {
                throw new NotFoundException($"Patient with id: {appointment.PatientId} not found");
            }
    
            command.Parameters.Clear();
            command.CommandText = @"SELECT doctor_id
                                    FROM Doctor WHERE PWZ = @PWZ
                                    ;";
            command.Parameters.AddWithValue("@PWZ", appointment.PWZ);
            var doctor = await command.ExecuteScalarAsync();
            if (doctor == null)
            {
                throw new NotFoundException($"Doctor with PWZ: {appointment.PWZ} not found");
            }
    
            command.Parameters.Clear();
            command.CommandText = @"INSERT INTO Appointment(appoitment_id, patient_id, doctor_id, date)
                                        VALUES (@IdAppointment, @IdPatient, @IdDoctor, @Date)
                ";
            command.Parameters.AddWithValue("@IdAppointment", appointment.AppointmentId);
            command.Parameters.AddWithValue("@IdPatient", patient);
            command.Parameters.AddWithValue("@IdDoctor", doctor);
            command.Parameters.AddWithValue("@Date", DateTime.Now);
    
            await command.ExecuteNonQueryAsync();
    
            List<ServicePostDto> services = new List<ServicePostDto>();
    
            foreach (var service in appointment.Services)
            {
                command.Parameters.Clear();
                command.CommandText = @"SELECT service_id FROM Service S WHERE S.name = @Name";
                command.Parameters.AddWithValue("@Name", service.ServiceName);
    
                var result = await command.ExecuteScalarAsync();
    
                if (result == null || result == DBNull.Value)
                {
                    throw new NotFoundException($"Service with name: '{service.ServiceName}' not found");
                }
    
                int serviceId = Convert.ToInt32(result);
    
                command.Parameters.Clear();
                command.CommandText = @"
                     INSERT INTO Appointment_Service(appoitment_id, service_id, service_fee)
                    VALUES (@IdAppointment, @IdService, @FeeService)
                ";
    
                command.Parameters.AddWithValue("@IdAppointment", appointment.AppointmentId);
                command.Parameters.AddWithValue("@IdService", serviceId);
                command.Parameters.AddWithValue("@FeeService", service.ServiceFee);
                
                await command.ExecuteNonQueryAsync();
            }
    
            await transaction.CommitAsync();
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync();
            throw e;
        }
    }
}