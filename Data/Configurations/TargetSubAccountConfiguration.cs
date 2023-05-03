using FinancialAdvisorTelegramBot.Models.Operations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinancialAdvisorTelegramBot.Data.Configurations
{
    public class TargetSubAccountConfiguration : IEntityTypeConfiguration<TargetSubAccount>
    {
        public void Configure(EntityTypeBuilder<TargetSubAccount> builder)
        {
            builder.ToTable("target_subaccounts");
            builder.HasKey(x => x.Id);

            builder.HasOne(x => x.Account)
                .WithMany(x => x.TargetAccounts)
                .HasForeignKey(x => x.AccountId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.Transactions)
                .WithMany(x => x.TargetSubAccounts)
                .UsingEntity(entity => entity.ToTable("target_to_transactions"));

            builder.Property(x => x.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            builder.Property(x => x.AccountId)
                .HasColumnName("account_id")
                .IsRequired();

            builder.Property(x => x.Name)
                .HasColumnName("name")
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(x => x.Description)
                .HasColumnName("description")
                .HasColumnType("text");

            builder.Property(x => x.CurrentBalance)
                .HasColumnName("balance")
                .HasColumnType("decimal(12,6)")
                .IsRequired();

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
