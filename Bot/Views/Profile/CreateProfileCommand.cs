using FinancialAdvisorTelegramBot.Bot.Args;
using FinancialAdvisorTelegramBot.Bot.Commands;
using FinancialAdvisorTelegramBot.Models.Core;
using FinancialAdvisorTelegramBot.Models.Telegram;
using FinancialAdvisorTelegramBot.Services.Core;
using FinancialAdvisorTelegramBot.Utils;
using FinancialAdvisorTelegramBot.Utils.CommandSerializing;

namespace FinancialAdvisorTelegramBot.Bot.Views.Profile
{
    public class CreateProfileCommand : ICommand
    {
        private enum CreatingProfileStatus
        {
            AskName, AskLastname, AskEmail, Finished
        }

        public static string TEXT_STYLE => "New profile";
        public static string DEFAULT_STYLE => "/profile_create";
        public virtual bool IsFinished { get; private set; } = false;

        private readonly IBot _bot;
        private readonly IUserService _userService;

        private CreatingProfileStatus _status => (CreatingProfileStatus)Status;
        [CommandPropertySerializable] public int Status { get; set; }
        [CommandPropertySerializable] public string? Name { get; set; }
        [CommandPropertySerializable] public string? Surname { get; set; }
        [CommandPropertySerializable] public string? Email { get; set; }

        public CreateProfileCommand(IBot bot, IUserService userService)
        {
            _bot = bot;
            _userService = userService;
        }

        public bool CanExecute(UpdateArgs update, TelegramUser user)
            => (update.GetTextData() == DEFAULT_STYLE 
            || update.GetTextData() == TEXT_STYLE)
            && user.UserId is null;

        public async Task Execute(UpdateArgs update, TelegramUser user)
        {
            switch (_status)
            {
                case CreatingProfileStatus.AskName:

                    await _bot.Write(user, new TextMessageArgs
                    {
                        Text = $"Write your name:"
                    });

                    Status++;
                    return;


                case CreatingProfileStatus.AskLastname:

                    string name = update.GetTextData().Trim();
                    Validators.ValidateName(name);
                    Name = name;
                    await _bot.Write(user, new TextMessageArgs
                    {
                        Text = $"Write your surname:" +
                        $"\n(you can {GeneralCommands.SetEmpty} this field)"
                    });

                    Status++;
                    return;


                case CreatingProfileStatus.AskEmail:
                    if (update.GetTextData() != GeneralCommands.Skip)

                    if (update.GetTextData() != GeneralCommands.SetEmpty)
                    {
                        string surname = update.GetTextData().Trim();
                        Validators.ValidateName(surname);
                        Surname = surname;
                    }
                    await _bot.Write(user, new TextMessageArgs
                    {
                        Text = $"Write your email:" +
                        $"\n(you can {GeneralCommands.SetEmpty} this field)"
                    });

                    Status++;
                    return;


                case CreatingProfileStatus.Finished:
                    if (update.GetTextData() != GeneralCommands.Skip)

                    if (update.GetTextData() != GeneralCommands.SetEmpty)
                    {
                        string email = update.GetTextData().Trim();
                        Validators.ValidateEmail(email);
                        Email = email;
                    }


                    User profile = await _userService.Create(user, Name, Surname, Email);
                    await _bot.Write(user, new TextMessageArgs
                    {
                        Text = $"{profile.FirstName}'s profile has been created!",
                        MarkupType = ReplyMarkupType.KeyboardRemove
                    });

                    IsFinished = true;
                    return;
            }
        }
    }
}
