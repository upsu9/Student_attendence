using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Student_attendence.Models.ModelDto
{
    public class AdminUserDto 
    {
        public string AdminUserName { get; set; }
        public string UniqueId { get; set; }
        public string FatherName { get; set; }
        public string Branch { get; set; }
        public string Password { get; set; }
        public string EmailId { get; set; }
        public string PhoneNumber { get; set; }
        public AdminUserStatusEnum Status { get; set; }
        public IsActiveEnum IsActive { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int UpdatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public int CreatedBy { get; set; }
    }
}
