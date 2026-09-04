namespace HotelManagementAPI.Entities
{
    public class Reservation : BaseEntity
    {
        public Guid RoomId { get; set; }

        public Guid UserId { get; set; }

        public DateTime CheckInDate { get; set; }

        public DateTime CheckOutDate { get; set; }

        public decimal TotalPrice { get; set; }

        public string? Status { get; set; }

        public decimal CancellationFee { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}