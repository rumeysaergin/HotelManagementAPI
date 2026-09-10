using HotelManagementAPI.Application.Repositories;
using HotelManagementAPI.Domain.Entities;
using HotelManagementAPI.Infrastructure.Data;

namespace HotelManagementAPI.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly HotelManagementDbContext _context;

        public UserRepository(HotelManagementDbContext context)
        {
            _context = context;
        }

        public bool EmailExists(string email)
        {
            return _context.Users.Any(x =>
                x.Email == email &&
                !x.IsDeleted);
        }

        public User? GetByEmail(string email)
        {
            return _context.Users.FirstOrDefault(x =>
                x.Email == email &&
                !x.IsDeleted);
        }

        public User? GetById(Guid id)
        {
            return _context.Users.FirstOrDefault(x =>
                x.Id == id &&
                !x.IsDeleted);
        }

        public User Add(User user)
        {
            _context.Users.Add(user);
            return user;
        }

        public void Update(User user)
        {
            _context.Users.Update(user);
        }

        public void UpdateBalance(Guid userId, decimal amount)
        {
            var user = _context.Users.FirstOrDefault(x =>
                x.Id == userId &&
                !x.IsDeleted);

            if (user == null)
                return;

            user.Balance += amount;
        }

        public void SaveChanges()
        {
            _context.SaveChanges();
        }
    }
}