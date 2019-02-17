using System;
using System.Collections.Generic;
using System.Text;

namespace OSWTF.Models
{
    public class IndividualTestEligibility
    {
        public string TestName { get; set; }
        public string Variation { get; set; }
    }

    public class TestEligibility
    {
        public List<IndividualTestEligibility> IndividualTestEligibilities { get; set; }
        public List<string> Errors { get; set; }

        public TestEligibility()
        {
            IndividualTestEligibilities = new List<IndividualTestEligibility>();
            Errors = new List<string>();
        }
    }
}
