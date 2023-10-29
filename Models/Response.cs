using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Student_attendence.Models
{
    public class Response
    {
        public bool IsSuccess { get; set; }
        public string ResponseMessage { get; set; }
        public Object Result { get; set; }
    }
}
