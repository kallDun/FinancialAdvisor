using FinancialAdvisorTelegramBot.Bot.Args;
using FinancialAdvisorTelegramBot.Bot.Commands;
using FinancialAdvisorTelegramBot.Models.Core;
using FinancialAdvisorTelegramBot.Models.Telegram;
using FinancialAdvisorTelegramBot.Services.Core;
using FinancialAdvisorTelegramBot.Utils;
using FinancialAdvisorTelegramBot.Utils.CommandSerializing;

namespace FinancialAdvisorTelegramBot.Bot.Views.Profiles
{
    public class UpdateProfileCommand : ICommand
    {
        private enum CreatingProfileStatus
        {
            AskName, AskLastname, AskOccupation, AskEmail, Finished
        }

        public static string TEXT_STYLE => "Update profile";
        public static string DEFAULT_STYLE => "/update";
        public virtual bool IsFinished { get; private set; } = false;

        [CommandPropertySerializable] public int Status { get; set; }
        [CommandPropertySerializable] public bool SkipName { get; set; }
        [CommandPropertySerializable] public string? Name { get; set; }
        [CommandPropertySerializable] public bool SkipSurname { get; set; }
        [CommandPropertySerializable] public string? Surname { get; set; }
        [CommandPropertySerializable] public bool SkipOccupation { get; set; }
        [CommandPropertySerializable] public string? Occupation { get; set; }
        [CommandPropertySerializable] public bool SkipEmail { get; set; }
        [CommandPropertySerializable] public string? Email { get; set; }

        private CreatingProfileStatus _status => (CreatingProfileStatus)Status;

        private readonly IBot _bot;
        private readonly IUserService _userService;

        public UpdateProfileCommand(IBot bot, IUserService userService)
        {
            _bot = bot;
            _userService = userService;
        }

        public bool CanExecute(UpdateArgs update, TelegramUser user)
            => user.ContextMenu == ContextMenus.Profile
            && (update.GetTextData() == DEFAULT_STYLE || update.GetTextData() == TEXT_STYLE)
            && user.UserId is not null;

        public async Task Execute(UpdateArgs update, TelegramUser user)
        {
            string text = update.GetTextData();
            Task function = _status switch
            {
                CreatingProfileStatus.AskName => AskName(user),
                CreatingProfileStatus.AskLastname => AskLastName(user, text),
                CreatingProfileStatus.AskOccupation => AskOccupation(user, text),
                CreatingProfileStatus.AskEmail => AskEmail(user, text),
                CreatingProfileStatus.Finished => ProcessResult(user, text),
                _ => throw new NotImplementedException()
            };
            
            await function;

            if (_status is CreatingProfileStatus.Finished)
            {
                IsFinished = true;
            }
            Status++;
        }

        private async Task ProcessResult(TelegramUser user, string text)
        {
            if (text != GeneralCommands.SetEmpty
                && text != GeneralCommands.Skip)
            {
                string email = text.Trim();
                Validators.ValidateEmail(email);
                Email = email;
            }
            SkipEmail = text == GeneralCommands.Skip;

            if (SkipName && SkipSurname && SkipOccupation && SkipEmail) 
                throw new ArgumentException("You didn't update any field");

            if (user.UserId is null) throw new InvalidOperationException("User id is null!");
            User profile = await _userService.GetById((int)user.UserId) ?? throw new InvalidOperationException("Cannot find profile!");

            if (!SkipName) profile.FirstName = Name;
            if (!SkipSurname) profile.LastName = Surname;
            if (!SkipOccupation) profile.Occupation = Occupation;
            if (!SkipEmail) profile.Email = Email;
            await _userService.Update(profile);

            await _bot.Write(user, new TextMessageArgs
            {
                Text = $"{profile.FirstName}'s profile has been updated"
            });
        }

        private async Task AskEmail(TelegramUser user, string text)
        {
            if (text != GeneralCommands.Skip)
            {
                string occupation = text.Trim();
                Validators.ValidateName(occupation);
                Occupation = occupation;
            }
            SkipOccupation = text == GeneralCommands.Skip;

            await _bot.Write(user, new TextMessageArgs
            {
                Text = $"Write your new email:",
                MarkupType = ReplyMarkupType.InlineKeyboard,
                InlineKeyboardButtons = new()
                {
                    new()
                    {
                        new InlineButton("Set email empty", GeneralCommands.SetEmpty)
                    },
                    new()
                    {
                        new InlineButton("Do not update email", GeneralCommands.Skip)
                    }
                }
            });
        }

        private async Task AskOccupation(TelegramUser user, string text)
        {
            if (text != GeneralCommands.SetEmpty
                && text != GeneralCommands.Skip)
            {
                string surname = text.Trim();
                Validators.ValidateName(surname);
                Surname = surname;
            }
            SkipSurname = text == GeneralCommands.Skip;

            await _bot.Write(user, new TextMessageArgs
            {
                Text = $"Write your new occupation:",
                MarkupType = ReplyMarkupType.InlineKeyboard,
                InlineKeyboardButtons = new()
                {
                    new()
                    {
                        new InlineButton("Do not update occupation", GeneralCommands.Skip)
                    }
                }
            });
        }

        private async Task AskLastName(TelegramUser user, string text)
        {
            if (text != GeneralCommands.Skip)
            {
                string name = text.Trim();
                Validators.ValidateName(name);
                Name = name;
            }
            SkipName = text == GeneralCommands.Skip;

            await _bot.Write(user, new TextMessageArgs
            {
                Text = $"Write your new surname:",
                MarkupType = ReplyMarkupType.InlineKeyboard,
                InlineKeyboardButtons = new()
                {
                    new()
                    {
                        new InlineButton("Set surname empty", GeneralCommands.SetEmpty)
                    },
                    new()
                    {
                        new InlineButton("Do not update surname", GeneralCommands.Skip)
                    }
                }
            });
        }

        private async Task AskName(TelegramUser user)
        {
            await _bot.Write(user, new TextMessageArgs
            {
                Text = $"Write your new name:",
                MarkupType = ReplyMarkupType.InlineKeyboard,
                InlineKeyboardButtons = new()
                {
                    new()
                    {
                        new InlineButton("Do not update name", GeneralCommands.Skip)
                    }
                }
            });
        }
    }
}
