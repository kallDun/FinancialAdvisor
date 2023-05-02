using FinancialAdvisorTelegramBot.Models.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinancialAdvisorTelegramBot.Data.Configurations
{
    public class TransactionGroupToCategoryConfiguration : IEntityTypeConfiguration<TransactionGroupToCategory>
    {
        public void Configure(EntityTypeBuilder<TransactionGroupToCategory> builder)
        {
            builder.ToTable("transaction_group_to_categories");

            builder.HasKey(x => new { x.TransactionGroupId, x.CategoryId });

            builder.HasOne(x => x.TransactionGroup)
                .WithMany(x => x.TransactionGroupToCategories)
                .HasForeignKey(x => x.TransactionGroupId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Category)
                .WithMany(x => x.TransactionGroupToCategories)
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
            
            builder.Property(x => x.TransactionGroupId)
                .HasColumnName("transaction_group_id")
                .IsRequired();

            builder.Property(x => x.CategoryId)
                .HasColumnName("category_id")
                .IsRequired();

            builder.Property(x => x.TotalIncome)
                .HasColumnType("decimal(14,6)")
                .HasColumnName("total_income")
                .HasDefaultValue(0)
                .IsRequired();

            builder.Property(x => x.TotalExpense)
                .HasColumnType("decimal(14,6)")
                .HasColumnName("total_expense")
                .HasDefaultValue(0)
                .IsRequired();
        }
    }
}
