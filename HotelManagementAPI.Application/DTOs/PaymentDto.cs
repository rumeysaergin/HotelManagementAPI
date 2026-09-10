namespace HotelManagementAPI.Application.DTOs
{
    public class PaymentDto
    {
        public Guid Id { get; set; }
        public Guid ReservationId { get; set; }
        public decimal Amount { get; set; }
        public string PaymentType { get; set; }
        public string Status { get; set; }
        public DateTime TransactionDate { get; set; }
    }
}