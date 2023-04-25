using FinancialAdvisorTelegramBot.Models.Operations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinancialAdvisorTelegramBot.Data.Configurations
{
    public class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
    {
        public void Configure(EntityTypeBuilder<Subscription> builder)
        {
            builder.ToTable("subscriptions");
            builder.HasKey(x => x.Id);

            builder.HasOne(x => x.User)
                .WithMany(x => x.Subscriptions)
                .HasForeignKey(x => x.UserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Account)
                .WithMany(x => x.Subscriptions)
                .HasForeignKey(x => x.AccountId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasMany(x => x.Transactions)
                .WithMany(x => x.Subscriptions)
                .UsingEntity(entity => entity.ToTable("subscriptions_to_transactions"));

            builder.Property(x => x.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            builder.Property(x => x.UserId)
                .HasColumnName("user_id")
                .IsRequired();

            builder.Property(x => x.AccountId)
                .HasColumnName("account_id");

            builder.Property(x => x.Name)
                .HasColumnName("name")
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.Amount)
                .HasColumnName("amount")
                .HasColumnType("decimal(12,4)")
                .IsRequired();

            builder.Property(x => x.PaymentDay)
                .HasColumnName("payment_day")
                .IsRequired();

            builder.Property(x => x.AutoPay)
                .HasColumnName("auto_pay")
                .IsRequired();

            builder.Property(x => x.LastPaymentDate)
                .HasColumnName("last_payment_date")
                .HasColumnType("timestamp with time zone")
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
