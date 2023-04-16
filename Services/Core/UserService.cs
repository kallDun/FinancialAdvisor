using FinancialAdvisorTelegramBot.Models.Core;
using FinancialAdvisorTelegramBot.Models.Telegram;
using FinancialAdvisorTelegramBot.Repositories.Core;

namespace FinancialAdvisorTelegramBot.Services.Core
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repository;

        public UserService(IUserRepository repository)
        {
            _repository = repository;
        }

        public async Task<User?> GetById(int userId)
        {
            return await _repository.GetById(userId);
        }

        public async Task<User> Create(TelegramUser telegramUser, string first_name, string? last_name, string? email)
        {
            var user = new User
            {
                FirstName = first_name,
                LastName = last_name,
                Email = email,
                TelegramUser = telegramUser
            };

            var userId = await _repository.Add(user);
            return await _repository.GetById(userId) ?? throw new Exception("User not found");
        }
        
        public async Task Update(User user)
        {
            await _repository.Update(user);
        }

        public async Task DeleteById(int userId)
        {
            await _repository.DeleteById(userId);
        }
    }
}
