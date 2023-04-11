using FinancialAdvisorTelegramBot.Models.Telegram;
using FinancialAdvisorTelegramBot.Repositories.Telegram;

namespace FinancialAdvisorTelegramBot.Services.Telegram
{
    public class TelegramUserService : ITelegramUserService
    {
        private readonly ITelegramUserRepository _telegramUserRepository;

        public TelegramUserService(ITelegramUserRepository telegramUserRepository)
        {
            _telegramUserRepository = telegramUserRepository;
        }

        public async Task<TelegramUser> GetExistingOrCreateNewTelegramUser(long chatId, string? username, string? firstName, string? lastName)
        {
            TelegramUser? telegramUser = await _telegramUserRepository.GetByChatId(chatId);
            if (telegramUser == null)
            {
                telegramUser = new TelegramUser
                {
                    ChatId = chatId,
                    Username = username,
                    FirstName = firstName,
                    LastName = lastName
                };
                int id = await _telegramUserRepository.Add(telegramUser);
                telegramUser = await _telegramUserRepository.Get(id);
                if (telegramUser == null)
                {
                    throw new Exception("Telegram user was not created");
                }
                return telegramUser;
            }

            if (telegramUser.Username != username
                || telegramUser.FirstName != firstName
                || telegramUser.LastName != lastName)
            {
                telegramUser.Username = username;
                telegramUser.FirstName = firstName;
                telegramUser.LastName = lastName;
                return await _telegramUserRepository.Update(telegramUser);
            }

            return telegramUser;
        }
    }
}
