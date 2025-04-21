using AutoMapper;
using Microsoft.EntityFrameworkCore;
using web_service.Data;
using web_service.Data.Entities;
using web_service.Domain.Entities;

public class CarService : ICarService
{
    private readonly ApplicationDbContext _db;
    private readonly IMapper _mapper;

    public CarService(ApplicationDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task AddCarAsync(Car car, string clientProfileId)
    {
        // Проверяем существование клиента
        var clientExists = await _db.ClientProfiles
            .AnyAsync(c => c.UserId == clientProfileId);

        if (!clientExists)
            throw new InvalidOperationException("Клиент не найден");

        // Проверяем уникальность VIN
        if (await _db.Cars.AnyAsync(c => c.VIN == car.VIN))
            throw new InvalidOperationException("VIN уже существует");

        var entity = _mapper.Map<CarEntity>(car);
        entity.ClientProfileId = clientProfileId;  // Устанавливаем связь

        _db.Cars.Add(entity);
        await _db.SaveChangesAsync();
    }

    public async Task<IEnumerable<Car>> GetCarsByOwnerAsync(string clientProfileId)
    {
        var ents = await _db.Cars
            .Where(c => c.ClientProfileId == clientProfileId)
            .ToListAsync();

        return _mapper.Map<IEnumerable<Car>>(ents);
    }
}