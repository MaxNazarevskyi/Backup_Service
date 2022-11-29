using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backup_Service.Data.DataModels
{
    [Table("User")]
    public class User : IEntity
    {
        [Key]
        [Column("UserId")]
        public int Id { get; set; }

        [Required]
        public string Username { get; set; } = null!;

        [Required]
        [StringLength(255)]
        public string Password { get; set; } = null!;
    }
}
