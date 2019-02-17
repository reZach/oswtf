using System;

namespace OSWTF.Models
{
    public class TestToBeRan
    {
        public string Name { get; set; }
        public int[] Split { get; set; }
        public DateTime Begin { get; set; }
        public DateTime End { get; set; }
        public string[] Exclude { get; set; }
        public bool Active { get; set; }
        public string ForceVariation { get; set; }
    }
}