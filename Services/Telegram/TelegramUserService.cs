﻿using FinancialAdvisorTelegramBot.Bot.Commands;
using FinancialAdvisorTelegramBot.Bot.Views;
using FinancialAdvisorTelegramBot.Models.Telegram;
using FinancialAdvisorTelegramBot.Repositories.Telegram;
using FinancialAdvisorTelegramBot.Utils.Attributes;
using FinancialAdvisorTelegramBot.Utils.CommandSerializing;

namespace FinancialAdvisorTelegramBot.Services.Telegram
{
    [CustomService]
    public class TelegramUserService : ITelegramUserService
    {
        private readonly ITelegramUserRepository _telegramUserRepository;

        public TelegramUserService(ITelegramUserRepository telegramUserRepository)
        {
            _telegramUserRepository = telegramUserRepository;
        }

        public async Task<TelegramUser> GetExistingOrCreateNewTelegramUser(long chatId, long telegramId,
            string? username, string? firstName, string? lastName, string languageCode)
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
                    LastName = lastName,
                    LanguageCode = languageCode,
                    ContextMenu = ContextMenus.MainMenu,
                    CreatedAt = DateTime.Now,
                };
                TelegramUser created = await _telegramUserRepository.Add(telegramUser);
                return await _telegramUserRepository.GetById(created.Id) ?? throw new Exception("Telegram user was not created");
            }

            if (telegramUser.Username != username
                || telegramUser.FirstName != firstName
                || telegramUser.LastName != lastName
                || telegramUser.LanguageCode != languageCode)
            {
                telegramUser.Username = username;
                telegramUser.FirstName = firstName;
                telegramUser.LastName = lastName;
                telegramUser.LanguageCode = languageCode;

                telegramUser.UpdatedAt = DateTime.Now;
                return await _telegramUserRepository.Update(telegramUser);
            }

            return telegramUser;
        }

        public async Task SetContextMenu(TelegramUser user, string contextMenu)
        {
            if (user.ContextMenu != contextMenu)
            {
                user.ContextMenu = contextMenu;
                user.UpdatedAt = DateTime.Now;
                await _telegramUserRepository.Update(user);
            }
        }

        public async Task SaveCurrentCommand(TelegramUser user, ICommand? command)
        {
            if (command != null)
            {
                string commandType = command.GetType().ToString();
                string commandData = CommandDataSerializer.Serialize(command);
                if (user.CurrentCommand == null)
                {
                    user.CurrentCommand = new TelegramCurrentCommand
                    {
                        Type = commandType,
                        DataJson = commandData,
                        CreatedAt = DateTime.Now
                    };
                }
                else
                {
                    user.CurrentCommand.Type = commandType;
                    user.CurrentCommand.DataJson = commandData;
                    user.CurrentCommand.UpdatedAt = DateTime.Now;
                }
            }
            else
            {
                user.CurrentCommand = null;
            }

            user.UpdatedAt = DateTime.Now;
            await _telegramUserRepository.Update(user);
        }
        
        public ICommand? GetCurrentCommand(TelegramUser user, ICommandContainer commandContainer)
        {
            ICommand? currentCommand = null;
            foreach (var command in commandContainer.Commands)
            {
                if (command.GetType().ToString() == user.CurrentCommand?.Type)
                {
                    currentCommand = command;
                }
            }
            if (currentCommand != null)
            {
                CommandDataSerializer.Deserialize(user.CurrentCommand?.DataJson ?? string.Empty, currentCommand);
            }
            return currentCommand;
        }
    }
}
