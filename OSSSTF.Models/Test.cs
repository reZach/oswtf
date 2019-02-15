using System;
using System.Collections.Generic;
using System.Text;

namespace OSSSTF.Models
{
    public class Test
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Cookie { get; set; }
        public string CookieValue { get; set; }
        public string Group { get; set; }
        public string Content { get; set; }
        public string Selector { get; set; }
        public DateTime? Begin { get; set; }
        public DateTime? End { get; set; }

        public Template Template { get; set; }
        public List<UrlRule> UrlRules { get; set; }
    }
}
