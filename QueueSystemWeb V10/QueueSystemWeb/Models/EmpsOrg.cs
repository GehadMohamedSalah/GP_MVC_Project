using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QueueSystemWeb.Models
{
    public class EmpsOrg
    {
        public int? id { get; set; }
        public int? emp_id { get; set; }
        public int? branch_id { get; set; }
        public int? service_id { get; set; }
        public IEnumerable<user> emps { get; set; }
        public IEnumerable<Branch> branches { get; set; }
        public IEnumerable<Services_> services { get; set; }
    }
}