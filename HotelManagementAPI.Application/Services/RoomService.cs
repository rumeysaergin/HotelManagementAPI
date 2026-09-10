using AutoMapper;
using HotelManagementAPI.Application.DTOs;
using HotelManagementAPI.Application.Repositories;
using HotelManagementAPI.Domain.Entities;

namespace HotelManagementAPI.Application.Services
{
    public class RoomService : IRoomService
    {
        private readonly IRoomRepository _repository;
        private readonly IHotelRepository _hotelRepository;
        private readonly IMapper _mapper;
        private readonly ErrorMessageService _errorMessageService;

        public RoomService(
            IRoomRepository repository,
            IHotelRepository hotelRepository,
            IMapper mapper,
            ErrorMessageService errorMessageService)
        {
            _repository = repository;
            _hotelRepository = hotelRepository;
            _mapper = mapper;
            _errorMessageService = errorMessageService;
        }

        public List<RoomDto> GetRooms(Guid? hotelId)
        {
            var rooms = _repository.GetRooms(hotelId);

            return _mapper.Map<List<RoomDto>>(rooms);
        }

        public RoomDto? CreateRoom(
            Guid userId,
            RoomDto roomDto)
        {
            var hotel =
                _hotelRepository.GetById(roomDto.HotelId);

            if (hotel == null ||
                hotel.UserId != userId)
            {
                return null;
            }

            var room = _mapper.Map<Room>(roomDto);

            room.Id = Guid.NewGuid();
            room.IsDeleted = false;
            room.IsAvailable = true;

            _repository.Add(room);
            _repository.SaveChanges();

            return _mapper.Map<RoomDto>(room);
        }

        public RoomDto? UpdateRoom(
            Guid userId,
            Guid id,
            RoomDto roomDto)
        {
            var room = _repository.GetById(id);

            if (room == null)
            {
                return null;
            }

            var hotel =
                _hotelRepository.GetById(room.HotelId);

            if (hotel == null ||
                hotel.UserId != userId)
            {
                var error = _errorMessageService.Get(
                    "ROOM_UPDATE_UNAUTHORIZED");

                throw new UnauthorizedAccessException(
                    $"{error.Code}: {error.Message}");
            }

            _mapper.Map(roomDto, room);

            _repository.Update(room);
            _repository.SaveChanges();

            return _mapper.Map<RoomDto>(room);
        }

        public bool DeleteRoom(
            Guid userId,
            Guid id)
        {
            var room = _repository.GetById(id);

            if (room == null)
            {
                return false;
            }

            var hotel =
                _hotelRepository.GetById(room.HotelId);

            if (hotel == null ||
                hotel.UserId != userId)
            {
                var error = _errorMessageService.Get(
                    "ROOM_DELETE_UNAUTHORIZED");

                throw new UnauthorizedAccessException(
                    $"{error.Code}: {error.Message}");
            }

            room.IsDeleted = true;

            _repository.Update(room);
            _repository.SaveChanges();

            return true;
        }
    }
}