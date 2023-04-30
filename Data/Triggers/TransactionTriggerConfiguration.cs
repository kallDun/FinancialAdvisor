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
                    (tableRefs, group) => group.Id == tableRefs.New.TransactionGroupId && tableRefs.New.Amount > 0,
                    (tableRefs, group) => new TransactionGroup() { TotalIncome = group.TotalIncome + tableRefs.New.Amount })
                .Update<TransactionGroup>(
                    (tableRefs, group) => group.Id == tableRefs.New.TransactionGroupId && tableRefs.New.Amount < 0,
                    (tableRefs, group) => new TransactionGroup() { TotalExpense = group.TotalExpense + Math.Abs(tableRefs.New.Amount) })
                .Update<TransactionGroupToCategory>(
                    (tableRefs, groupToCategory) => groupToCategory.TransactionGroupId == tableRefs.New.TransactionGroupId
                        && groupToCategory.CategoryId == tableRefs.New.CategoryId && tableRefs.New.Amount > 0,
                    (tableRefs, groupToCategory) => new TransactionGroupToCategory() { TotalIncome = groupToCategory.TotalIncome + tableRefs.New.Amount })
                .Update<TransactionGroupToCategory>(
                    (tableRefs, groupToCategory) => groupToCategory.TransactionGroupId == tableRefs.New.TransactionGroupId
                        && groupToCategory.CategoryId == tableRefs.New.CategoryId && tableRefs.New.Amount < 0,
                    (tableRefs, groupToCategory) => new TransactionGroupToCategory() { TotalExpense = groupToCategory.TotalExpense + Math.Abs(tableRefs.New.Amount) })
                ));
        }
    }
}
