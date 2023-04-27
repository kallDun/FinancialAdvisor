using FinancialAdvisorTelegramBot.Models.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinancialAdvisorTelegramBot.Data.Configurations
{
    public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
    {
        public void Configure(EntityTypeBuilder<Transaction> builder)
        {
            builder.ToTable("transactions");
            builder.HasKey(x => x.Id);

            builder.HasOne(x => x.Account)
                .WithMany(x => x.Transactions)
                .HasForeignKey(x => x.AccountId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.TransactionGroup)
                .WithMany(x => x.Transactions)
                .HasForeignKey(x => x.TransactionGroupId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Category)
                .WithMany(x => x.Transactions)
                .HasForeignKey(x => x.CategoryId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(x => x.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            builder.Property(x => x.TransactionGroupId)
                .HasColumnName("transaction_group_id")
                .IsRequired();

            builder.Property(x => x.AccountId)
                .HasColumnName("account_id")
                .IsRequired();

            builder.Property(x => x.Amount)
                .HasColumnName("amount")
                .HasColumnType("decimal(8,2)")
                .IsRequired();

            builder.Property(x => x.Communicator)
                .HasColumnName("communicator")
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(x => x.TransactionTime)
                .HasColumnName("transaction_time")
                .HasColumnType("timestamp with time zone")
                .IsRequired();

            builder.Property(x => x.CategoryId)
                .HasColumnName("category_id")
                .IsRequired();

            builder.Property(x => x.Details)
                .HasColumnName("details")
                .HasColumnType("text");
        }
    }
}
