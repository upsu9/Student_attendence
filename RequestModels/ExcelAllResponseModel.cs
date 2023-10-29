using Student_attendence.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Student_attendence.RequestModels
{
    public class ExcelAllResponseModel
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; }
        public string StudentUniqueNumber { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime AttendenceDate { get; set; }
        public int CreatedBy { get; set; }
        public DayStatusEnum DayStatus { get; set; }
    }
}
