using FinancialAdvisorTelegramBot.Models.Core;
using FinancialAdvisorTelegramBot.Models.Telegram;
using FinancialAdvisorTelegramBot.Repositories.Core;
using FinancialAdvisorTelegramBot.Utils.Attributes;

namespace FinancialAdvisorTelegramBot.Services.Core
{
    [CustomService]
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
                TelegramUser = telegramUser,
                CreatedAt = DateTime.Now,
            };

            User created = await _repository.Add(user);
            return await _repository.GetById(created.Id) ?? throw new Exception("User was not created");
        }

        public async Task DeleteById(int userId)
        {
            var user = await _repository.GetById(userId);
            await _repository.Delete(user ?? throw new InvalidDataException("User not found"));
        }
        
        public async Task<User> Update(User user)
        {
            user.UpdatedAt = DateTime.Now;
            return await _repository.Update(user);
        }
    }
}
