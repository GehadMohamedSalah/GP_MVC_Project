using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QueueSystemWeb.Models
{
    public class Periods
    {
        public List<string> periods { get; set; }
        public List<string> GetPeriods()
        {
            for(int i = 1; i <= 60; i++)
            {
                var x = "00:" + i + ":00";
                periods.Add(x);
            }
            return periods;
        }
    }

    public enum periods
    {
        
    }
}