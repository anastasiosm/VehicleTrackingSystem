using AutoMapper;
using VehicleTracking.Core.Entities;
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
        }
    }
}