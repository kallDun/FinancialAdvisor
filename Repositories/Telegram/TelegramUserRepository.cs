using FinancialAdvisorTelegramBot.Data;
using FinancialAdvisorTelegramBot.Models.Telegram;
using Microsoft.EntityFrameworkCore;

namespace FinancialAdvisorTelegramBot.Repositories.Telegram
{
    public class TelegramUserRepository : ITelegramUserRepository
    {
        private readonly AppDbContext _context;

        public TelegramUserRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<int> Add(TelegramUser entity)
        {
            entity.CreatedAt = DateTime.Now;
            _context.TelegramUsers.Add(entity);
            await _context.SaveChangesAsync();
            return entity.Id;
        }

        public async Task Delete(TelegramUser entity)
        {
            _context.TelegramUsers.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<TelegramUser?> Get(int id)
        {
            return await _context.TelegramUsers.FindAsync(id);
        }

        public async Task<IList<TelegramUser>> GetAll()
        {
            return await _context.TelegramUsers.ToListAsync();
        }

        public async Task<TelegramUser?> GetByChatId(long chatId)
        {
            return await _context.TelegramUsers.FirstOrDefaultAsync(x => x.ChatId == chatId);
        }

        public async Task<TelegramUser> Update(TelegramUser entity)
        {
            entity.UpdatedAt = DateTime.Now;
            _context.TelegramUsers.Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }
    }
}
