using FinancialAdvisorTelegramBot.Models.Core;
using Laraue.EfCoreTriggers.Common.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinancialAdvisorTelegramBot.Data.Triggers
{
    public class TransactionTriggerConfiguration : IEntityTypeConfiguration<Transaction>
    {
        public void Configure(EntityTypeBuilder<Transaction> builder)
        {
            builder.AfterInsert(trigger => trigger
                .Action(triggerAction => triggerAction
                .Upsert((tableRefs, account) => account.Id == tableRefs.New.AccountId, // find need account
                        (tableRefs) => new Account() { CurrentBalance = tableRefs.New.Amount }, // add account if it doesnt exist -- impossible in this way
                        (tableRefs, account) => new Account() { CurrentBalance = account.CurrentBalance + tableRefs.New.Amount } // update current balance
            )));
        }
    }
}
