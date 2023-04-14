namespace FinancialAdvisorTelegramBot.Models.Core
{
    public class Category
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public virtual User? User { get; set; }

        public string? Name { get; set; }

        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public virtual IList<Transaction>? Transactions { get; internal set; }
    }
}
