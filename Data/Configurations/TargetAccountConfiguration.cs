using FinancialAdvisorTelegramBot.Models.Core;
using FinancialAdvisorTelegramBot.Models.Operations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinancialAdvisorTelegramBot.Data.Configurations
{
    public class TargetAccountConfiguration : IEntityTypeConfiguration<TargetAccount>
    {
        public void Configure(EntityTypeBuilder<TargetAccount> builder)
        {
            builder.ToTable("target_accounts");
            builder.HasKey(x => x.Id);

            builder.HasOne(x => x.Account)
                .WithOne(x => x.TargetAccount)
                .HasForeignKey<Account>(x => x.TargetAccountId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Property(x => x.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            builder.Property(x => x.GoalAmount)
                .HasColumnName("goal_amount")
                .HasColumnType("decimal(12,2)")
                .IsRequired();

            builder.Property(x => x.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("timestamp with time zone")
                .IsRequired();

            builder.Property(x => x.UpdatedAt)
                .HasColumnName("updated_at")
                .HasColumnType("timestamp with time zone");
        }
    }
}
