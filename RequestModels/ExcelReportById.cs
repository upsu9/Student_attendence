using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Student_attendence.RequestModels
{
    public class ExcelReportById
    {
        public string FromDate { get; set; }
        public string TillDate { get; set; }
        public string UniqueId { get; set; }
        public string DayStatus { get; set; }
    }
}
