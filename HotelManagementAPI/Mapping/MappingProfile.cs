using AutoMapper;
using HotelManagementAPI.Entities;
using HotelManagementAPI.Models.DTOs;

namespace HotelManagementAPI.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Hotel, HotelDto>();
            CreateMap<HotelDto, Hotel>();

            CreateMap<Payment, PaymentDto>();
            CreateMap<PaymentDto, Payment>();

            CreateMap<Reservation, ReservationDto>();
            CreateMap<ReservationDto, Reservation>();

            CreateMap<Room, RoomDto>();
            CreateMap<RoomDto, Room>();

            CreateMap<User, UserDto>();
        }
    }
}