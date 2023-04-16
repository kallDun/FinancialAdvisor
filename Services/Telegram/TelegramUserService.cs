using FinancialAdvisorTelegramBot.Bot.Commands;
using FinancialAdvisorTelegramBot.Models.Telegram;
using FinancialAdvisorTelegramBot.Repositories.Telegram;
using FinancialAdvisorTelegramBot.Utils.CommandSerializing;

namespace FinancialAdvisorTelegramBot.Services.Telegram
{
    public class TelegramUserService : ITelegramUserService
    {
        private readonly ITelegramUserRepository _telegramUserRepository;
        private readonly ICommandContainer _commandContainer;

        public TelegramUserService(ITelegramUserRepository telegramUserRepository, ICommandContainer commandContainer)
        {
            _telegramUserRepository = telegramUserRepository;
            _commandContainer = commandContainer;
        }


        public async Task<TelegramUser> GetExistingOrCreateNewTelegramUser(long chatId, long telegramId,
            string? username, string? firstName, string? lastName)
        {
            TelegramUser? telegramUser = await _telegramUserRepository.GetByTelegramId(telegramId);
            if (telegramUser == null)
            {
                telegramUser = new TelegramUser
                {
                    ChatId = chatId,
                    TelegramId = telegramId,
                    Username = username,
                    FirstName = firstName,
                    LastName = lastName
                };
                int id = await _telegramUserRepository.Add(telegramUser);
                telegramUser = await _telegramUserRepository.GetById(id);
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
                await _telegramUserRepository.Update(telegramUser);
            }

            return telegramUser;
        }

        public async Task SaveCurrentCommand(TelegramUser user, ICommand? command)
        {
            if (command != null)
            {
                string commandType = command.GetType().ToString();
                string commandData = CommandDataSerializer.Serialize(command);
                if (user.CurrentView == null)
                {
                    user.CurrentView = new TelegramUserView
                    {
                        CurrentCommandType = commandType,
                        CurrentCommandData = commandData,
                        CreatedAt = DateTime.Now
                    };
                }
                else
                {
                    user.CurrentView.CurrentCommandType = commandType;
                    user.CurrentView.CurrentCommandData = commandData;
                    user.CurrentView.UpdatedAt = DateTime.Now;
                }
            }
            else
            {
                user.CurrentView = null;
            }

            await _telegramUserRepository.Update(user);
        }
        
        public ICommand? GetCurrentCommand(TelegramUser user)
        {
            ICommand? currentCommand = null;
            foreach (var command in _commandContainer.Commands)
            {
                if (command.GetType().ToString() == user.CurrentView?.CurrentCommandType)
                {
                    currentCommand = command;
                }
            }
            if (currentCommand != null)
            {
                CommandDataSerializer.Deserialize(user.CurrentView?.CurrentCommandData ?? string.Empty, currentCommand);
            }
            return currentCommand;
        }

        /*public async Task DeleteProfile(TelegramUser user)
        {
            user.UserId = null;
            await _telegramUserRepository.Update(user);
        }*/
    }
}
