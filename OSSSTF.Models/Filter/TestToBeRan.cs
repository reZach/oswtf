using System;
using System.Collections.Generic;
using System.Text;

namespace OSWTF.Models.Filter
{
    public class TestToBeRan
    {
        public string Name { get; set; }
        public int[] Split { get; set; }
        public DateTime Begin { get; set; }
        public DateTime End { get; set; }
        public string[] Exclude { get; set; }
        public bool Active { get; set; }
    }
}
