using FinancialAdvisorTelegramBot.Bot.Args;
using FinancialAdvisorTelegramBot.Bot.Commands;
using FinancialAdvisorTelegramBot.Bot.ReplyArgs;
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

        public static string COMMAND_TEXT_STYLE => "New profile";
        public static string COMMAND_DEFAULT_STYLE => "/profile_create";
        public virtual bool IsFinished { get; private set; } = false;

        private readonly IBot _bot;
        private readonly IUserService _userService;

        [CommandSerializeData] private CreatingProfileStatus Status;
        [CommandSerializeData] private string Name = "";
        [CommandSerializeData] private string? Surname = null;
        [CommandSerializeData] private string? Email = null;

        public CreateProfileCommand(IBot bot, IUserService userService)
        {
            _bot = bot;
            _userService = userService;
        }

        public bool CanExecute(UpdateArgs update, TelegramUser user)
            => (update.GetTextData() == COMMAND_DEFAULT_STYLE 
            || update.GetTextData() == COMMAND_TEXT_STYLE)
            && user.UserId is null;

        public async Task Execute(UpdateArgs update, TelegramUser user)
        {
            try
            {
                switch (Status)
                {
                    case CreatingProfileStatus.AskName:
                        await _bot.Write(user, new TextMessageArgs
                        {
                            Text = $"Write your name:",
                            HideKeyboard = true
                        });
                        break;

                    case CreatingProfileStatus.AskLastname:
                        string name = update.GetTextData().Trim();
                        if (name.Length > 20) throw new ArgumentOutOfRangeException("Name is too long!");
                        Name = name;
                        await _bot.Write(user, new TextMessageArgs 
                        { 
                            Text = $"Write your surname:" +
                            $"\n(you can {GeneralCommands.Skip} this field)" 
                        });
                        break;

                    case CreatingProfileStatus.AskEmail:
                        if (update.GetTextData() != GeneralCommands.Skip)
                        {
                            string surname = update.GetTextData().Trim();
                            if (surname.Length > 20) throw new ArgumentOutOfRangeException("Surname is too long!");
                            Surname = surname;
                        }
                        await _bot.Write(user, new TextMessageArgs
                        {
                            Text = $"Write your email:" +
                            $"\n(you can {GeneralCommands.Skip} this field)"
                        });
                        break;

                    case CreatingProfileStatus.Finished:
                        if (update.GetTextData() != GeneralCommands.Skip)
                        {
                            string email = update.GetTextData().Trim();
                            if (email.Length > 50) throw new ArgumentOutOfRangeException("Email is too long!");
                            if (!Validators.ValidateEmail(email)) throw new ArgumentException("Email is not correct!");
                            Email = email;
                        }

                        User profile = await _userService.Create(user, Name, Surname, Email);
                        await _bot.Write(user, new TextMessageArgs
                        {
                            Text = $"Profile for {profile.FirstName}" + 
                            $"{(profile.LastName is null ? "" : " ")}{profile.LastName} was created!"
                        });
                        break;
                }
                if (Status is CreatingProfileStatus.Finished)
                {
                    IsFinished = true;
                }
                else
                {
                    Status += 1;
                }
            }
            catch (Exception e)
            {
                await _bot.Write(user, new TextMessageArgs
                {
                    Text = $"Could not create profile. {e.Message}"
                });
                IsFinished = true;
            }
        }
    }
}
