﻿using System;

namespace OSSSTF.Models
{
    public class UrlRule
    {
        public int Id { get; set; }
        public string Url { get; set; }

        public UrlRuleType UrlRuleType { get; set; }
    }
}
