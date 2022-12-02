using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Backup_Service.Data.DataModels;

namespace Backup_Service.Data.Configuration
{
    public class FilesConfiguration : IEntityTypeConfiguration<Files>
    {
        public void Configure(EntityTypeBuilder<Files> builder)
        {
            builder.HasKey(u => u.Id);

            builder.Property(u => u.Id)
                .IsRequired()
                .ValueGeneratedOnAdd();

            builder.Property(u => u.BackupName).IsRequired();
            builder.Property(u => u.Path).IsRequired();
            builder.Property(u => u.UserId).IsRequired();

            builder
                .HasOne(f => f.User)
                .WithMany(f => f.Files)
                .HasForeignKey(u => u.UserId);
        }
    }
}
