using FinancialAdvisorTelegramBot.Bot.Args;
using FinancialAdvisorTelegramBot.Bot.Commands;
using FinancialAdvisorTelegramBot.Models.Core;
using FinancialAdvisorTelegramBot.Models.Operations;
using FinancialAdvisorTelegramBot.Models.Telegram;
using FinancialAdvisorTelegramBot.Services.Core;
using FinancialAdvisorTelegramBot.Services.Operations;
using FinancialAdvisorTelegramBot.Utils;
using FinancialAdvisorTelegramBot.Utils.CommandSerializing;

namespace FinancialAdvisorTelegramBot.Bot.Views.Target
{
    public class CreateTargetCommand : ICommand
    {
        private enum CreatingTargetStatus
        {
            AskName, AskGoal, AskDescription, Finished
        }
        
        public static string TEXT_STYLE => "Create new target";
        public static string DEFAULT_STYLE => "/create";
        public bool IsFinished { get; private set; } = false;
        public bool ShowContextMenuAfterExecution => true;

        private readonly IBot _bot;
        private readonly IAccountService _accountService;
        private readonly ITargetService _targetService;

        private CreatingTargetStatus _status => (CreatingTargetStatus)Status;
        [CommandPropertySerializable] public int Status { get; set; }
        [CommandPropertySerializable] public string? Name { get; set; }
        [CommandPropertySerializable] public string? Description { get; set; }
        [CommandPropertySerializable] public decimal GoalAmount { get; set; }

        public CreateTargetCommand(IBot bot, IAccountService accountService, ITargetService targetService)
        {
            _bot = bot;
            _accountService = accountService;
            _targetService = targetService;
        }
        
        public bool CanExecute(UpdateArgs update, TelegramUser user)
        {
            var split = (string.IsNullOrEmpty(user.ContextMenu) ? string.Empty : user.ContextMenu).Split('/');
            return split.Length == 3 && split[0] == ContextMenus.Account && split[2] == ContextMenus.Target
                && (update.GetTextData() == DEFAULT_STYLE
                || update.GetTextData() == TEXT_STYLE)
                && user.UserId is not null;
        }

        public async Task Execute(UpdateArgs update, TelegramUser user)
        {
            if (user.UserId is null) throw new InvalidDataException("User id cannot be null");
            var contextMenuSplit = user.ContextMenu?.Split('/') ?? throw new InvalidDataException("Missing context menu");
            string text = update.GetTextData();

            Task function = _status switch
            {
                CreatingTargetStatus.AskName => AskTargetName(user, text),
                CreatingTargetStatus.AskGoal => AskTargetGoal(user, text),
                CreatingTargetStatus.AskDescription => AskTargetDescription(user, text),
                CreatingTargetStatus.Finished => ProcessResult(user, text, contextMenuSplit),
                _ => throw new ArgumentOutOfRangeException()
            };
            await function;

            if (_status is CreatingTargetStatus.Finished)
            {
                IsFinished = true;
            }
            Status++;
        }

        private async Task ProcessResult(TelegramUser user, string text, string[] contextMenuSplit)
        {
            if (text != GeneralCommands.SetEmpty)
            {
                Description = text.Trim();
            }

            if (Name is null) throw new InvalidDataException("Name cannot be null");
            Account account = await _accountService.GetByName(user.UserId
                ?? throw new InvalidDataException("Profile id cannot be null"), contextMenuSplit[1])
                ?? throw new InvalidDataException($"Account with name {contextMenuSplit[1]} not found");

            TargetSubAccount target = await _targetService.Create(account, Name, Description, GoalAmount);

            await _bot.Write(user, new TextMessageArgs
            {
                Text = $"Target <code>{target.Name}</code> subaccount created successfully " +
                $"with goal <code>{target.GoalAmount}</code>"
            });
        }

        private async Task AskTargetDescription(TelegramUser user, string text)
        {
            GoalAmount = Converters.ToDecimal(text.Trim());
            if (GoalAmount <= 0) throw new ArgumentException("Goal amount cannot be less or equal to zero");

            await _bot.Write(user, new TextMessageArgs
            {
                Text = $"Write target description:",
                MarkupType = ReplyMarkupType.InlineKeyboard,
                InlineKeyboardButtons = new List<List<InlineButton>>()
                {
                    new() { new("Set description empty", GeneralCommands.SetEmpty) }
                }
            });
        }

        private async Task AskTargetGoal(TelegramUser user, string text)
        {
            string name = text.Trim();
            Validators.ValidateName(name);
            Name = name;

            await _bot.Write(user, new TextMessageArgs
            {
                Text = $"Write positive goal amount:"
            });
        }

        private async Task AskTargetName(TelegramUser user, string text)
        {
            await _bot.Write(user, new TextMessageArgs
            {
                Text = $"Write target name:"
            });
        }
    }
}
