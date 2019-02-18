using System;
using System.Collections.Generic;
using System.Text;

namespace OSWTF.Models
{
    public class Template
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Content { get; set; }
        public string Selector { get; set; }
    }
}
