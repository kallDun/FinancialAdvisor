using FinancialAdvisorTelegramBot.Models.Operations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinancialAdvisorTelegramBot.Data.Configurations
{
    public class LimitByCategoryConfiguration : IEntityTypeConfiguration<LimitByCategory>
    {
        public void Configure(EntityTypeBuilder<LimitByCategory> builder)
        {
            builder.ToTable("limit_by_categories");
            builder.HasKey(x => x.Id);

            builder.HasOne(x => x.User)
                .WithMany(x => x.LimitByCategories)
                .HasForeignKey(x => x.UserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Account)
                .WithMany(x => x.LimitByCategories)
                .HasForeignKey(x => x.AccountId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(x => x.Category)
                .WithMany(x => x.LimitByCategories)
                .HasForeignKey(x => x.CategoryId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            builder.Property(x => x.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            builder.Property(x => x.UserId)
                .HasColumnName("user_id")
                .IsRequired();

            builder.Property(x => x.AccountId)
                .HasColumnName("account_id");

            builder.Property(x => x.CategoryId)
                .HasColumnName("category_id")
                .IsRequired();

            builder.Property(x => x.Limit)
                .HasColumnName("limit")
                .HasColumnType("decimal(12,2)")
                .IsRequired();

            builder.Property(x => x.GroupCount)
                .HasColumnName("group_count")
                .IsRequired();

            builder.Property(x => x.GroupIndexFrom)
                .HasColumnName("group_index_from")
                .IsRequired();

            builder.Property(x => x.Enabled)
                .HasColumnName("enabled")
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
