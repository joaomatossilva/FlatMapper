using System;
using System.Collections.Generic;
using System.Text;

namespace ImportFootprint
{
    public class DataItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime Date { get; set; }
        public DayOfWeek? DayOfWeek { get; set; }
        public string Description { get; set; }
    }
}
