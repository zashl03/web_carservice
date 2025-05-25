using AutoMapper;
using web_service.Data.Entities;
using web_service.Models.Car;
using web_service.Models.Record;
using web_service.Domain.Entities;

namespace web_service.Mappings
{
    public class DomainToEntityProfile : Profile
    {
        public DomainToEntityProfile()
        {
            // === Car mappings ===
            CreateMap<CarEntity, CarViewModel>();
            CreateMap<AddCarViewModel, Car>();
            CreateMap<Car, CarEntity>()
                .ForMember(dest => dest.ClientProfileId, opt => opt.Ignore())
                .ReverseMap();

            // === Record mappings ===
            // AddRecordViewModel -> RecordEntity
            CreateMap<AddRecordViewModel, RecordEntity>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CarId, opt => opt.MapFrom(src => src.SelectedCarId))
                .ForMember(dest => dest.BookingDate, opt => opt.MapFrom(src => src.BookingDate))
                .ForMember(dest => dest.Comment, opt => opt.MapFrom(src => src.Comment));

            // RecordEntity -> RecordViewModel
            CreateMap<RecordEntity, RecordViewModel>()
                .ForMember(dest => dest.CarDisplay,
                    opt => opt.MapFrom(src => $"{src.Car.Brand} {src.Car.Model} ({src.Car.VIN})"));
        }
    }
}