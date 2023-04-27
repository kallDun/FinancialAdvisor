using FinancialAdvisorTelegramBot.Models.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinancialAdvisorTelegramBot.Data.Configurations
{
    public class TransactionGroupConfiguration : IEntityTypeConfiguration<TransactionGroup>
    {
        public void Configure(EntityTypeBuilder<TransactionGroup> builder)
        {
            builder.ToTable("transaction_groups");
            builder.HasKey(x => x.Id);

            builder.HasOne(x => x.Account)
                .WithMany(x => x.TransactionGroups)
                .HasForeignKey(x => x.AccountId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            builder.Property(x => x.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            builder.Property(x => x.AccountId)
                .HasColumnName("account_id")
                .IsRequired();

            builder.Property(x => x.Index)
                .HasColumnName("index")
                .IsRequired();

            builder.Property(x => x.TotalAmount)
                .HasColumnName("total_amount")
                .HasColumnType("decimal(14,4)")
                .IsRequired();

            builder.Property(x => x.DateFrom)
                .HasColumnName("date_from")
                .HasColumnType("timestamp with time zone")
                .IsRequired();
            
            builder.Property(x => x.DateTo)
                .HasColumnName("date_to")
                .HasColumnType("timestamp with time zone")
                .IsRequired();

            builder.Property(x => x.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("timestamp with time zone")
                .IsRequired();
        }
    }
}
