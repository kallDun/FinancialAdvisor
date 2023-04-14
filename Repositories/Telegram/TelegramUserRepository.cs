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

        public async Task<TelegramUser?> GetById(int id)
        {
            return await _context.TelegramUsers
                .Include(x => x.CurrentView)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<IList<TelegramUser>> GetAll()
        {
            return await _context.TelegramUsers.ToListAsync();
        }

        public async Task<TelegramUser?> GetByTelegramId(long telegramId)
        {
            return await _context.TelegramUsers
                .Include(x => x.CurrentView)
                .FirstOrDefaultAsync(x => x.TelegramId == telegramId);
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
