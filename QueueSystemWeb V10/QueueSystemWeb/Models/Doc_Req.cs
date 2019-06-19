using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QueueSystemWeb.Models
{
    public class Doc_Req
    {
        public int id { get; set; }
        public string namedoc { get; set; }
        public string notes { get; set; }
        public int serviceID { get; set; }
    }
}