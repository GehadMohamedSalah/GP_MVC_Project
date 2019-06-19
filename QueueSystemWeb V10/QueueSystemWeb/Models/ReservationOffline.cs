using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QueueSystemWeb.Models
{
    public class ReservationOffline
    {
        public string name { get; set; }
        public string phone { get; set; }
        public IEnumerable<Services_> services { get; set; }
        public int? service_id { get; set; }
        public string date { get; set; }
        public IEnumerable<string> times { get; set; }
        public string time { get; set; }
    }
}