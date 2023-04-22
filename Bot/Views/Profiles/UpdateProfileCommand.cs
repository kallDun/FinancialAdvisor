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
            AskName, AskLastname, AskEmail, Finished
        }

        public static string TEXT_STYLE => "Update profile";
        public static string DEFAULT_STYLE => "/update";
        public virtual bool IsFinished { get; private set; } = false;

        [CommandPropertySerializable] public int Status { get; set; }
        [CommandPropertySerializable] public bool SkipName { get; set; }
        [CommandPropertySerializable] public string Name { get; set; } = "";
        [CommandPropertySerializable] public bool SkipSurname { get; set; }
        [CommandPropertySerializable] public string? Surname { get; set; }
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
            switch (_status)
            {
                case CreatingProfileStatus.AskName:
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

                    Status++;
                    return;


                case CreatingProfileStatus.AskLastname:

                    if (update.GetTextData() != GeneralCommands.Skip)
                    {
                        string name = update.GetTextData().Trim();
                        Validators.ValidateName(name);
                        Name = name;
                    }
                    SkipName = update.GetTextData() == GeneralCommands.Skip;

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

                    Status++;
                    return;


                case CreatingProfileStatus.AskEmail:

                    if (update.GetTextData() != GeneralCommands.SetEmpty
                        && update.GetTextData() != GeneralCommands.Skip)
                    {
                        string surname = update.GetTextData().Trim();
                        Validators.ValidateName(surname);
                        Surname = surname;
                    }
                    SkipSurname = update.GetTextData() == GeneralCommands.Skip;

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

                    Status++;
                    return;


                case CreatingProfileStatus.Finished:

                    if (update.GetTextData() != GeneralCommands.SetEmpty
                        && update.GetTextData() != GeneralCommands.Skip)
                    {
                        string email = update.GetTextData().Trim();
                        Validators.ValidateEmail(email);
                        Email = email;
                    }
                    SkipEmail = update.GetTextData() == GeneralCommands.Skip;

                    if (SkipName && SkipSurname && SkipEmail)
                    {
                        await _bot.Write(user, new TextMessageArgs
                        {
                            Text = "You didn't update any field!"
                        });
                    }
                    else
                    {
                        if (user.UserId is null) throw new InvalidOperationException("User id is null!");
                        User profile = await _userService.GetById((int)user.UserId) ?? throw new InvalidOperationException("Cannot find profile!");

                        if (!SkipName) profile.FirstName = Name;
                        if (!SkipSurname) profile.LastName = Surname;
                        if (!SkipEmail) profile.Email = Email;
                        await _userService.Update(profile);

                        await _bot.Write(user, new TextMessageArgs
                        {
                            Text = $"{profile.FirstName}'s profile has been updated"
                        });
                    }

                    IsFinished = true;
                    return;
            }
        }
    }
}
