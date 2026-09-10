using HotelManagementAPI.Domain.Entities;

namespace HotelManagementAPI.Application.Repositories
{
    public interface IUserRepository
    {
        bool EmailExists(string email);

        User? GetByEmail(string email);

        User? GetById(Guid id);

        User Add(User user);

        void Update(User user);

        void UpdateBalance(Guid userId, decimal amount);

        void SaveChanges();
    }
}