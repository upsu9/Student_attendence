using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Student_attendence.RequestModels
{
    public class MarkRegAttendence
    {
        public string uniqueId { get; set; }
        public string dayStatus { get; set; }
        public string date { get; set; }
    }
}
