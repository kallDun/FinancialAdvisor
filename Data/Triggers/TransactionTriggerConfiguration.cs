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
                .Update<Account>(
                    (tableRefs, account) => account.Id == tableRefs.New.AccountId,
                    (tableRefs, account) => new Account() { CurrentBalance = account.CurrentBalance + tableRefs.New.Amount })
                .Update<TransactionGroup>(
                    (tableRefs, group) => group.Id == tableRefs.New.TransactionGroupId,
                    (tableRefs, group) => new TransactionGroup() { TotalAmount = group.TotalAmount + tableRefs.New.Amount })
                ));
        }
    }
}
