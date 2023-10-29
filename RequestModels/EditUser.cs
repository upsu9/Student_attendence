using Student_attendence.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Student_attendence.RequestModels
{
    public class EditUser
    {
        [Required]
        public string EditedAdminUserId { get; set; }
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
