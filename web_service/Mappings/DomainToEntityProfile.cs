using AutoMapper;
using web_service.Data.Entities;
using web_service.Domain.Entities;
using web_service.Models.Car;

/*
 * DTO (Data Transfer Object) — это объект передачи данных, 
 * который используется для переноса данных между слоями 
 * приложения, особенно при взаимодействии с API или 
 * при обмене данными между клиентом и сервером.
 */
public class DomainToEntityProfile : Profile
{
    public DomainToEntityProfile()
    {
        // Маппинг сущности БД -> ViewModel для отображения
        CreateMap<CarEntity, CarViewModel>();

        // Маппинг DTO добавления автомобиля -> доменная модель
        CreateMap<AddCarViewModel, Car>();

        // Двусторонний маппинг доменной модели <-> сущности БД
        CreateMap<Car, CarEntity>()
            .ForMember(dest => dest.ClientProfileId, opt => opt.Ignore()) // Игнорируем при маппинге (заполняется отдельно)
            .ReverseMap(); // Обратный маппинг (CarEntity -> Car)
    }
}