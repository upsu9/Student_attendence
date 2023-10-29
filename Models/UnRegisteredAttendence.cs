using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Student_attendence.Models
{
    public class UnRegisteredAttendence
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AttendenceId { get; set; }
        [MaxLength(40)]
        public string StudentName { get; set; }
        [MaxLength(40)]
        public string FatherName { get; set; }
        public DayStatusEnum DayStatus { get; set; }
        public DateTime AttendenceDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public int CreatedBy { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int UpdatedBy { get; set; }
        public IsActiveEnum IsActive { get; set; }
    }
}
