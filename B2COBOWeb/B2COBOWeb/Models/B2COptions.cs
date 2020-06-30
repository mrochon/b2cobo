using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace B2COBOWeb.Models
{
    public class B2COptions
    {
        public B2COptions()
        {
            redirectUri = "https://dummy"; // default, use it when registering apps in B2C
        }
        public string tenantName { get; set; }
        public string tenantId { get; set; }
        public string oboJourneyName { get; set; }
        public string redirectUri { get; set; } 
    }
}
