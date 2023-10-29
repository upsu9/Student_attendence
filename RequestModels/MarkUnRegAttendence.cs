using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Student_attendence.RequestModels
{
    public class MarkUnRegAttendence
    {
        public string studentName { get; set; }
        public string studentFatherName { get; set; }
        public string dayStatus { get; set; }
        public string date { get; set; }
    }
}
