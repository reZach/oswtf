using System.Collections.Generic;

namespace OSWTF.Server.Models
{
    public class IndividualResult
    {
        public string TestName { get; set; }
        public string Variation { get; set; }
    }

    public class Result
    {
        public Result()
        {
            IndividualResults = new List<IndividualResult>();
            Errors = new List<string>();
        }

        public List<IndividualResult> IndividualResults { get; set; }

        public string this[string key]
        {
            get
            {
                var result = IndividualResults.Find(
                    e => e.TestName == key);

                return result == null ? "" : result.Variation;
            }
        }

        public List<string> Errors { get; set; }
    }
}