// Services/IRecordService.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using web_service.Models.Record;

namespace web_service.Services
{
    /// Сервис для работы с «записями на обслуживание».
    public interface IRecordService
    {

        /// Создаёт новую запись на обслуживание для указанного клиента и автомобиля.

        Task CreateRecordAsync(string userId, Guid carId, DateTime DateAppointment, string comment);

        /// Возвращает все записи на обслуживание, принадлежащие данному клиенту.
        Task<IEnumerable<RecordViewModel>> GetUserRecordsAsync(string userId);
    }
}
