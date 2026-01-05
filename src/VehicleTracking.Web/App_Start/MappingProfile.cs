using AutoMapper;
using VehicleTracking.Domain.Entities;
using VehicleTracking.Web.Models;

namespace VehicleTracking.Web.App_Start
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Vehicle -> VehicleDto
            CreateMap<Vehicle, VehicleDto>();

            // Vehicle -> VehicleListDto
            CreateMap<Vehicle, VehicleListDto>();

            // GpsPosition -> GpsPositionDto
            CreateMap<GpsPosition, GpsPositionDto>();

            // GpsPositionData -> GpsPosition
            CreateMap<GpsPositionData, GpsPosition>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.VehicleId, opt => opt.Ignore())
                .ForMember(dest => dest.Vehicle, opt => opt.Ignore());

            // SubmitGpsPositionRequest -> GpsPosition
            CreateMap<SubmitGpsPositionRequest, GpsPosition>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Vehicle, opt => opt.Ignore());
        }
    }
}
