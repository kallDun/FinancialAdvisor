namespace FinancialAdvisorTelegramBot.Models
{
    public class User
    {
        public int Id { get; set; }

        public string? Name { get; set; }
        
        public string? LastName { get; set; }

        public string? Email { get; set; }

        public DateTime CreatedAt { get; set; }
        
        public DateTime? UpdatedAt { get; set; }

        public virtual IList<Account>? Accounts { get; set; }

        public virtual IList<Category>? Categories { get; set; }
    }
}
