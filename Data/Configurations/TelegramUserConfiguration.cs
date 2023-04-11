using FinancialAdvisorTelegramBot.Models.Telegram;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinancialAdvisorTelegramBot.Data.Configurations
{
    public class TelegramUserConfiguration : IEntityTypeConfiguration<TelegramUser>
    {
        public void Configure(EntityTypeBuilder<TelegramUser> builder)
        {
            builder.ToTable("telegram_users");
            builder.HasKey(x => x.Id);

            builder.HasOne(x => x.User)
                .WithOne(x => x.TelegramUser)
                .HasForeignKey<TelegramUser>(x => x.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Property(x => x.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd()
                .UseIdentityColumn();

            builder.Property(x => x.ChatId)
                .HasColumnName("chat_id")
                .IsRequired();

            builder.Property(x => x.UserId)
                .HasColumnName("user_id");
            
            builder.Property(x => x.FirstName)
                .HasColumnName("first_name")
                .HasMaxLength(100);
            
            builder.Property(x => x.LastName)
                .HasColumnName("last_name")
                .HasMaxLength(100);
            
            builder.Property(x => x.Username)
                .HasColumnName("username")
                .HasMaxLength(100);

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
