using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Student_attendence.Models
{
    public class AdminUser
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AdminUserId { get; set; }
        [MaxLength(40)]

        public string AdminUserName { get; set; }
        [MaxLength(40)]

        public string UniqueId { get; set; }
        [MaxLength(40)]

        public string FatherName { get; set; }
        [MaxLength(20)]

        public string Branch { get; set; }
        [MaxLength(150)]

        public string Password { get; set; }
        [MaxLength(20)]

        public string EmailId { get; set; }
        [MaxLength(20)]

        public string PhoneNumber { get; set; }
        public AdminUserStatusEnum Status { get; set; }
        public IsActiveEnum IsActive { get; set; }
        public Gender Gender { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int UpdatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public int CreatedBy { get; set; }
    }
}
