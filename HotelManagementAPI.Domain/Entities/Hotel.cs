namespace HotelManagementAPI.Domain.Entities
{
    public class Hotel : BaseEntity
    {
        public Guid UserId { get; set; }

        public string Name { get; set; }

        public string Address { get; set; }

        public string City { get; set; }
    }
}