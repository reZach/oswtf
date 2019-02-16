using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using OSWTF.Models;
using System;

namespace OSWTF.Filters
{
    public class ServerSideFilter : IActionFilter
    {
        private string cookiePrefix = "oswtf_";
        private int[] trafficSplit;
        private bool isSuccessful = false;
        private readonly string _testName;
        private readonly IOptionsSnapshot<TestingOptions> _options;

        public ServerSideFilter(string testName, IOptionsSnapshot<TestingOptions> options)
        {
            _testName = testName;
            _options = options;

            ParseOptions();
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            if (!isSuccessful)
                return;

            int a = 234234;
            int b = a - 234;


        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            // Don't need to do anything here
        }

        private void ParseOptions()
        {
            TestingOptions.Test thisTest;

            // Short-circuit
            if (_options != null ||
                _options.Value.Tests.Count > 0)
            {
                thisTest = _options.Value.Tests.Find(t => t.Name == _testName);

                if (thisTest == null)
                    return;
            }
            else
                return;

            // Parse any meta options
            cookiePrefix = _options.Value.Prefix ?? cookiePrefix;



            isSuccessful = true;
        }
    }
}
