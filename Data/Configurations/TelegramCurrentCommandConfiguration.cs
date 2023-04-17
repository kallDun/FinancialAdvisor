using FinancialAdvisorTelegramBot.Models.Telegram;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinancialAdvisorTelegramBot.Data.Configurations
{
    public class TelegramCurrentCommandConfiguration : IEntityTypeConfiguration<TelegramCurrentCommand>
    {
        public void Configure(EntityTypeBuilder<TelegramCurrentCommand> builder)
        {
            builder.ToTable("telegram_current_commands");
            builder.HasKey(t => t.Id);

            builder.HasOne(x => x.TelegramUser)
                .WithOne(x => x.CurrentCommand)
                .HasForeignKey<TelegramCurrentCommand>(x => x.TelegramUserId);

            builder.Property(x => x.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            builder.Property(x => x.TelegramUserId)
                .HasColumnName("telegram_user_id")
                .IsRequired();

            builder.Property(x => x.Type)
                .HasColumnName("command_type")
                .HasColumnType("text")
                .IsRequired();

            builder.Property(x => x.DataJson)
                .HasColumnName("command_data_json")
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
