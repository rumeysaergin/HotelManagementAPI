using AutoMapper;
using HotelManagementAPI.Application.DTOs;
using HotelManagementAPI.Application.Repositories;
using HotelManagementAPI.Domain.Entities;

namespace HotelManagementAPI.Application.Services
{
    public class HotelService : IHotelService
    {
        private readonly IHotelRepository _repository;
        private readonly IMapper _mapper;
        private readonly ErrorMessageService _errorMessageService;

        public HotelService(
            IHotelRepository repository,
            IMapper mapper,
            ErrorMessageService errorMessageService)
        {
            _repository = repository;
            _mapper = mapper;
            _errorMessageService = errorMessageService;
        }

        public List<HotelDto> GetHotels(Guid userId)
        {
            var hotels = _repository.GetHotels(userId);

            return _mapper.Map<List<HotelDto>>(hotels);
        }

        public HotelDto CreateHotel(Guid userId, HotelDto hotelDto)
        {
            var hotel = _mapper.Map<Hotel>(hotelDto);

            hotel.Id = Guid.NewGuid();
            hotel.UserId = userId;
            hotel.IsDeleted = false;

            _repository.Add(hotel);
            _repository.SaveChanges();

            return _mapper.Map<HotelDto>(hotel);
        }

        public HotelDto? UpdateHotel(
            Guid userId,
            Guid id,
            HotelDto hotelDto)
        {
            var hotel = _repository.GetById(id);

            if (hotel == null)
            {
                return null;
            }

            if (hotel.UserId != userId)
            {
                var error = _errorMessageService.Get(
                    "HOTEL_UPDATE_UNAUTHORIZED");

                throw new UnauthorizedAccessException(
                    $"{error.Code}: {error.Message}");
            }

            _mapper.Map(hotelDto, hotel);

            _repository.Update(hotel);
            _repository.SaveChanges();

            return _mapper.Map<HotelDto>(hotel);
        }

        public bool DeleteHotel(Guid userId, Guid id)
        {
            var hotel = _repository.GetById(id);

            if (hotel == null)
            {
                return false;
            }

            if (hotel.UserId != userId)
            {
                var error = _errorMessageService.Get(
                    "HOTEL_DELETE_UNAUTHORIZED");

                throw new UnauthorizedAccessException(
                    $"{error.Code}: {error.Message}");
            }

            hotel.IsDeleted = true;

            _repository.Update(hotel);
            _repository.SaveChanges();

            return true;
        }
    }
}