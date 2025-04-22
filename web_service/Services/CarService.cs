/*
 * Реализация сервиса для управления автомобилями.
 * Обрабатывает бизнес-логику: добавление авто, проверки, работу с БД через EF Core.
 * Связывает доменные модели с Entity Framework и AutoMapper.
 */
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using web_service.Data;
using web_service.Data.Entities;
using web_service.Domain.Entities;

public class CarService : ICarService
{
    private readonly ApplicationDbContext _db;  // Контекст БД для работы с Entity Framework Core
    private readonly IMapper _mapper;           // AutoMapper для преобразования моделей (Domain <-> Entity)

    public CarService(ApplicationDbContext db, IMapper mapper)
    {
        _db = db;     // Внедрение зависимости контекста БД
        _mapper = mapper;  // Внедрение AutoMapper через DI
    }

    public async Task AddCarAsync(Car car, string clientProfileId) // Добавление автомобиля асинхронно
    {
        // Проверка существования клиента в БД
        var clientExists = await _db.ClientProfiles
            .AnyAsync(c => c.UserId == clientProfileId);

        if (!clientExists)
            throw new InvalidOperationException("Клиент не найден");  // Блокировка создания авто без владельца

        // Проверка уникальности VIN в системе
        if (await _db.Cars.AnyAsync(c => c.VIN == car.VIN))
            throw new InvalidOperationException("VIN уже существует");  // Защита от дубликатов

        var entity = _mapper.Map<CarEntity>(car);
        entity.ClientProfileId = clientProfileId;  // Явная привязка к владельцу

        _db.Cars.Add(entity);
        await _db.SaveChangesAsync();  // Сохранение в БД в транзакции
    }

    public async Task<IEnumerable<Car>> GetCarsByOwnerAsync(string clientProfileId)
    {
        // Запрос автомобилей только для указанного владельца
        var ents = await _db.Cars
            .Where(c => c.ClientProfileId == clientProfileId)
            .ToListAsync();  // Асинхронное выполнение запроса

        return _mapper.Map<IEnumerable<Car>>(ents);  // Преобразование Entity -> Domain Model
    }
}