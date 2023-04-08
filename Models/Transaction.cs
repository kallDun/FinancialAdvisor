namespace FinancialAdvisorTelegramBot.Models
{
    public class Transaction
    {
        public int Id { get; set; }

        public int AccountId { get; set; }

        public virtual Account? Account { get; set; }

        public decimal Amount { get; set; }

        public string? Type { get; set; }

        public string? Communicator { get; set; }

        public int CategoryId { get; set; }
        
        public virtual Category? Category { get; set; }

        public DateTime TransactionTime { get; set; }

        public string? Details { get; set; }
    }
}
