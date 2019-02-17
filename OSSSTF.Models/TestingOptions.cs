using System;
using System.Collections.Generic;
using System.Text;

namespace OSWTF.Models
{
    public class TestingOptions
    {
        public string Prefix { get; set; }
        public List<Test> Tests { get; set; }        

        public class Test
        {
            public string Name { get; set; }
            public string Split { get; set; }
            public DateTime Begin { get; set; }
            public DateTime End { get; set; }
            public string[] Exclude { get; set; }
            public bool Active { get; set; }
        }
    }
}
