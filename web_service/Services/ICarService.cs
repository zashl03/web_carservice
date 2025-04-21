using web_service.Domain.Entities;

public interface ICarService
{
    Task AddCarAsync(Car car, string clientProfileId);
    Task<IEnumerable<Car>> GetCarsByOwnerAsync(string clientProfileId);
}