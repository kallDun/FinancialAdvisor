using FinancialAdvisorTelegramBot.Bot.Args;
using FinancialAdvisorTelegramBot.Bot.Commands;
using FinancialAdvisorTelegramBot.Models.Core;
using FinancialAdvisorTelegramBot.Models.Telegram;
using FinancialAdvisorTelegramBot.Services.Core;
using FinancialAdvisorTelegramBot.Utils;
using FinancialAdvisorTelegramBot.Utils.CommandSerializing;

namespace FinancialAdvisorTelegramBot.Bot.Views.Profiles
{
    public class CreateProfileCommand : ICommand
    {
        private enum CreatingProfileStatus
        {
            AskName, AskLastname, AskOccupation, AskEmail, Finished
        }

        public static string TEXT_STYLE => "Create new profile";
        public static string DEFAULT_STYLE => "/create";
        public bool IsFinished { get; private set; } = false;
        public bool ShowContextMenuAfterExecution => true;

        private readonly IBot _bot;
        private readonly IUserService _userService;

        private CreatingProfileStatus _status => (CreatingProfileStatus)Status;
        [CommandPropertySerializable] public int Status { get; set; }
        [CommandPropertySerializable] public string? Name { get; set; }
        [CommandPropertySerializable] public string? Surname { get; set; }
        [CommandPropertySerializable] public string? Occupation { get; set; }
        [CommandPropertySerializable] public string? Email { get; set; }

        public CreateProfileCommand(IBot bot, IUserService userService)
        {
            _bot = bot;
            _userService = userService;
        }

        public bool CanExecute(UpdateArgs update, TelegramUser user)
            => user.ContextMenu == ContextMenus.Profile
            && (update.GetTextData() == DEFAULT_STYLE || update.GetTextData() == TEXT_STYLE)
            && user.UserId is null;

        public async Task Execute(UpdateArgs update, TelegramUser user)
        {
            string text = update.GetTextData();
            Task function = _status switch
            {
                CreatingProfileStatus.AskName => AskName(user),
                CreatingProfileStatus.AskLastname => AskLastname(user, text),
                CreatingProfileStatus.AskOccupation => AskOccupation(user, text),
                CreatingProfileStatus.AskEmail => AskEmail(user, text),
                CreatingProfileStatus.Finished => ProcessResult(user, text),
                _ => throw new InvalidDataException("Invalid status")
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
            if (text != GeneralCommands.SetEmpty)
            {
                string email = text.Trim();
                Validators.ValidateEmail(email);
                Email = email;
            }
            if (Name is null) throw new InvalidDataException("Name cannot be empty!");
            if (Occupation is null) throw new InvalidDataException("Occupation cannot be empty!");

            User profile = await _userService.Create(user, Name, Surname, Occupation, Email);

            await _bot.Write(user, new TextMessageArgs
            {
                Text = $"{profile.FirstName}'s profile has been created!"
            });
        }

        private async Task AskEmail(TelegramUser user, string text)
        {
            string occupation = text.Trim();
            Validators.ValidateName(occupation);
            Occupation = occupation;
            
            await _bot.Write(user, new TextMessageArgs
            {
                Text = $"Write your email:",
                MarkupType = ReplyMarkupType.InlineKeyboard,
                InlineKeyboardButtons = new()
                {
                    new()
                    {
                        new InlineButton("Set email empty", GeneralCommands.SetEmpty)
                    }
                }
            });
        }

        private async Task AskOccupation(TelegramUser user, string text)
        {
            if (text != GeneralCommands.SetEmpty)
            {
                string surname = text.Trim();
                Validators.ValidateName(surname);
                Surname = surname;
            }

            await _bot.Write(user, new TextMessageArgs
            {
                Text = $"Write your occupation:"
            });
        }

        private async Task AskLastname(TelegramUser user, string text)
        {
            string name = text.Trim();
            Validators.ValidateName(name);
            Name = name;
            
            await _bot.Write(user, new TextMessageArgs
            {
                Text = $"Write your surname:",
                MarkupType = ReplyMarkupType.InlineKeyboard,
                InlineKeyboardButtons = new()
                {
                    new()
                    {
                        new InlineButton("Set surname empty", GeneralCommands.SetEmpty)
                    }
                }
            });
        }

        private async Task AskName(TelegramUser user)
        {
            await _bot.Write(user, new TextMessageArgs
            {
                Text = $"Write your name:"
            });
        }
    }
}
