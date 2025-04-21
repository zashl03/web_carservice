using AutoMapper;
using web_service.Data.Entities;
using web_service.Domain.Entities;
using web_service.Models.Car;

public class DomainToEntityProfile : Profile
{
    public DomainToEntityProfile()
    {
        CreateMap<CarEntity, CarViewModel>();
        CreateMap<AddCarViewModel, Car>();
        CreateMap<Car, CarEntity>()
            .ForMember(dest => dest.ClientProfileId, opt => opt.Ignore()) // Заполняется в сервисе
            .ReverseMap();
    }
}