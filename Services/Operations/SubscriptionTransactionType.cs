namespace FinancialAdvisorTelegramBot.Services.Operations
{
    /// <summary>
    /// Transaction type for subscription payments
    /// </summary>
    public enum SubscriptionTransactionType
    {
        /// <summary>
        /// Create transaction, changes next payment day for next month, overdue payment number is not changed
        /// </summary>
        Default,

        /// <summary>
        /// Create transaction, decrease overdue payment number, next payment day is not changed
        /// </summary>
        Late,

        /// <summary>
        /// Transaction do NOT create, increase overdue payment number, changes next payment day for next month
        /// </summary>
        Delayed
    }
}
