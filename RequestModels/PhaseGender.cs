using Student_attendence.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Student_attendence.RequestModels
{
    public class PhaseGender
    {
        public string Phase { get; set; }
        public Gender Gender { get; set; }
    }
}
