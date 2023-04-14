using FinancialAdvisorTelegramBot.Models.Telegram;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinancialAdvisorTelegramBot.Data.Configurations
{
    public class TelegramUserViewConfiguration : IEntityTypeConfiguration<TelegramUserView>
    {
        public void Configure(EntityTypeBuilder<TelegramUserView> builder)
        {
            builder.ToTable("telegram_user_views");
            builder.HasKey(t => t.Id);

            builder.HasOne(x => x.TelegramUser)
                .WithOne(x => x.CurrentView)
                .HasForeignKey<TelegramUserView>(x => x.TelegramUserId);

            builder.Property(x => x.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            builder.Property(x => x.TelegramUserId)
                .HasColumnName("telegram_user_id")
                .IsRequired();

            builder.Property(x => x.CurrentCommandType)
                .HasColumnName("current_command_type")
                .HasColumnType("text")
                .IsRequired();

            builder.Property(x => x.CurrentCommandData)
                .HasColumnName("current_command_data")
                .HasColumnType("text")
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
