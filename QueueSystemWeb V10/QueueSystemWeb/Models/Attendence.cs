using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QueueSystemWeb.Models
{
    public class Attendence
    {
        public int user_id { get; set; }
        public int app_id { get; set; }
        public string check { get; set; }
    }
}