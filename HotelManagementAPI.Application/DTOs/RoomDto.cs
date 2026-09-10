namespace HotelManagementAPI.Application.DTOs
{
    public class RoomDto
    {
        public Guid Id { get; set; }
        public Guid HotelId { get; set; }
        public int RoomNumber { get; set; }
        public string RoomType { get; set; }
        public int Capacity { get; set; }
        public decimal PricePerNight { get; set; }
        public bool IsAvailable { get; set; }
    }
}