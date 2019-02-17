using System.Collections.Generic;

namespace OSWTF.Server.Models
{
    public class IndividualTestEligibility
    {
        public string TestName { get; set; }
        public string Variation { get; set; }
    }

    public class TestEligibility
    {
        public TestEligibility()
        {
            IndividualTestEligibilities = new List<IndividualTestEligibility>();
            Errors = new List<string>();
        }

        public List<IndividualTestEligibility> IndividualTestEligibilities { get; set; }
        public List<string> Errors { get; set; }
    }
}