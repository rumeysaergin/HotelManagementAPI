using AutoMapper;
using HotelManagementAPI.Application.DTOs;
using HotelManagementAPI.Domain.Entities;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace HotelManagementAPI.Application.Mapping
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