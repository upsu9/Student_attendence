using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Student_attendence.Models
{
    public class Student
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int StudentId { get; set; }
        [MaxLength(40)]
        public string StudentName { get; set; }
        [MaxLength(20)]
        public string UniqueId { get; set; }
        [MaxLength(20)]

        public string School { get; set; }
        [MaxLength(20)]

        public string Category { get; set; }
        [MaxLength(20)]

        public string Class { get; set; }
        public DateTime BirthDate { get; set; }
        [MaxLength(40)]
        public string FatherName { get; set; }
        [MaxLength(20)]

        public string FatherMobileNumber { get; set; }
        [MaxLength(20)]

        public string MotherName { get; set; }
        [MaxLength(20)]

        public string MotherMobileNumber { get; set; }
        [MaxLength(40)]

        public string Branch { get; set; }
        [MaxLength(40)]

        public string EmailId { get; set; }

        [MaxLength(40)]
        public string FatherDOI { get; set; }

        [MaxLength(40)]
        public string MotherDOI { get; set; }

        public IsSatsangiEnum IsSatsangi { get; set; }

        [MaxLength(40)]
        public string Mohalla { get; set; }

        public Gender Gender { get; set; }

        public IsActiveEnum IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
