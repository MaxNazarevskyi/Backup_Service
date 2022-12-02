using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backup_Service.Data.DataModels
{
    [Table("Files")]
    public class Files : IEntity
    {
        [Key]
        [Column("FileId")]
        public int Id { get; set; }

        [Required]
        public string BackupName { get; set; } = null!;

        [Required]
        public string Path { get; set; } = null!;

        [ForeignKey("User")]
        public int UserId { get; set; }
        public virtual User User { get; set; }
    }
}
