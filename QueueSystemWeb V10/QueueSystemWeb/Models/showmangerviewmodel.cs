using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QueueSystemWeb.Models
{
    public class showmangerviewmodel
    {
        public int id { get; set; }
        public string name { get; set; }
        public string manger_id { get; set; }
        public IEnumerable<user> manger { get; set; }
        public organization organazation { get; set; }
    }
}