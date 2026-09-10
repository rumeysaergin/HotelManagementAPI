namespace HotelManagementAPI.Domain.Entities
{
    public class Payment : BaseEntity
    {
        public Guid ReservationId { get; set; }

        public decimal Amount { get; set; }

        public string PaymentType { get; set; }

        public string Status { get; set; }

        public DateTime TransactionDate { get; set; }
    }
}