using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace B2COBOWeb.Models
{
    public class ErrorMsg
    {
        public string version { get => "1.0.1"; }
        public string status { get; set; }
        public string userMessage { get; set; }
    }
}
