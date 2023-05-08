using FinancialAdvisorTelegramBot.Data;
using FinancialAdvisorTelegramBot.Models.Telegram;
using FinancialAdvisorTelegramBot.Utils.Attributes;
using Microsoft.EntityFrameworkCore;

namespace FinancialAdvisorTelegramBot.Repositories.Telegram
{
    [CustomRepository]
    public class TelegramUserRepository : ITelegramUserRepository
    {
        private readonly TelegramDbContext _context;

        public TelegramUserRepository(TelegramDbContext context)
        {
            _context = context;
        }

        public DbContext DatabaseContext => _context;

        public DbSet<TelegramUser> DbSet => _context.TelegramUsers;

        public async Task<TelegramUser?> GetByTelegramId(long telegramId)
        {
            return await _context.TelegramUsers
                .Include(x => x.CurrentCommand)
                .FirstOrDefaultAsync(x => x.TelegramId == telegramId);
        }
    }
}
