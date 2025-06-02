// Services/RecordService.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using web_service.Data;
using web_service.Data.Entities;
using web_service.Models.Record;

namespace web_service.Services
{
    /// <summary>
    /// Реализация сервиса записей на обслуживание.
    /// </summary>
    public class RecordService : IRecordService
    {
        private readonly ApplicationDbContext _db;

        public RecordService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task CreateRecordAsync(string userId, Guid carId, DateTime DateAppointment, string comment)
        {
            // Проверяем, что автомобиль принадлежит текущему клиенту
            var car = await _db.Cars
                .AsNoTracking()
                .Include(c => c.Client)
                .FirstOrDefaultAsync(c => c.Id == carId && c.Client.UserId == userId);

            if (car == null)
                throw new InvalidOperationException("Автомобиль не найден или не принадлежит текущему пользователю.");

            var record = new RecordEntity
            {
                Id = Guid.NewGuid(),
                CarId = carId,
                DateAppointment = DateAppointment,
                Comment = comment
            };

            _db.Records.Add(record);
            await _db.SaveChangesAsync();
        }

        public async Task<IEnumerable<RecordViewModel>> GetUserRecordsAsync(string userId)
        {
            return await _db.Records
                .AsNoTracking()
                .Include(r => r.Car)
                .ThenInclude(c => c.Client)
                .Where(r => r.Car.Client.UserId == userId)
                .Select(r => new RecordViewModel
                {
                    Id = r.Id,
                    CarDisplay = $"{r.Car.Brand} {r.Car.Model} ({r.Car.VIN})",
                    DateAppointment = r.DateAppointment,
                    Comment = r.Comment
                })
                .ToListAsync();
        }
    }
}
