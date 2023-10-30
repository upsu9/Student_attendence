using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Student_attendence.RequestModels
{
    public class PhaseData
    {
        public string Phase { get; set; }
        public int Boys { get; set; }
        public int Girls { get; set; }
        public int Total { get; set; }
    }
}
