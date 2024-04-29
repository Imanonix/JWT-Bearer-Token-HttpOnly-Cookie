using Microsoft.EntityFrameworkCore;
using Domain.Interfaces.Repository;
using Domain.Models;
using Infrastructure.AuthDbContext;


namespace Infrastructure.Implementation
{
    public class AccountRepository : IAccountRepository
    {
        private DbAuthDbContext _context;
        public AccountRepository(DbAuthDbContext context)
        {
            _context = context;
        }

        public async Task<bool> AddRefreshTokenAsync(string refreshToken, string email)
        {
            var user = await  _context.Users.SingleOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                throw new Exception("user not found");
            };
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpired = DateTime.UtcNow.AddHours(12);
            return true;
        }

        public async Task<User> AddUserAsync(User user)
        {
            await _context.Users.AddAsync(user);
            return user;
        }

        public async Task<User> GetUserByEmailAsync(string email)
        {
            var returnedUser = await _context.Users.SingleOrDefaultAsync(u => u.Email == email);

            return returnedUser;
        }

        public async Task<User> GetUserById(Guid id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            return user != null ? user : throw new Exception("user not found");
        }

        public async Task<bool> IsEmailExistedAsync(string email)
        {
            var user = await _context.Users.AnyAsync(u => u.Email == email);
            if (!user)
            {
                return false;
            }
            return true;
        }

        public async Task<bool> SaveAsync()
        {
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UserConfirmAsync(string email, string verificationToken)
        {
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == email && u.VerificationToken == verificationToken);
            if (user == null)
            {
                throw new Exception("user is not found");
            }
            user.VerifiedDate = DateTime.UtcNow;
            
            return true;
        }
    }
}
