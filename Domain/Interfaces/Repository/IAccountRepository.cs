using Domain.Models;

namespace Domain.Interfaces.Repository
{
    public interface IAccountRepository
    {
        Task<User> AddUserAsync(User user);
        Task<User> GetUserByEmailAsync(string email);
        Task<bool> IsEmailExistedAsync(string email);
        Task<User> GetUserById(Guid id);
        Task<bool> SaveAsync();
        Task<bool> UserConfirmAsync(string email, string verificationToken);
        Task<bool> AddRefreshTokenAsync(string refreshToken, string email);
    }
}
