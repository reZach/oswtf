using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http;
using OSWTF.Models;
using System;

namespace OSWTF.Filters
{
    public class ServerSideFilter : IActionFilter
    {
        private string cookiePrefix = "oswtf_";
        private int[] trafficSplit;
        private DateTime? expires;
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

            string cookieName = cookiePrefix + _testName;
            
            if (!context.HttpContext.Request.Cookies.ContainsKey(cookieName))
            {
                // Bucket the user into a segment
                Random roller = new Random();
                int roll = roller.Next(1, 101);

                int sum = 0;
                int bucket;
                for (bucket = 0; bucket < trafficSplit.Length; bucket++)
                {
                    sum += trafficSplit[bucket];

                    if (roll <= sum)
                        break;
                }

                // Create the cookie on the user's response
                context.HttpContext.Response.Cookies.Append(cookieName, 
                    Convert.ToString((char)(65 + bucket)), new CookieOptions
                    {
                        Domain = context.HttpContext.Request.Host.Host,
                        Path = "/",
                        Expires = expires,
                        Secure = context.HttpContext.Request.IsHttps,
                        IsEssential = true
                    });
            }
           
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

            // Validate test is formatted correctly
            if (thisTest.Name == null ||
                thisTest.Split == null ||
                thisTest.Begin == null)
            {
                return;
            }

            try
            {
                // Parse values
                trafficSplit = Array.ConvertAll(
                    thisTest.Split.Replace(" ", "").Split(','), a => Convert.ToInt32(a));
                expires = thisTest.End != null ? thisTest.End : (DateTime?)null;
                cookiePrefix = _options.Value.Prefix ?? cookiePrefix;

                isSuccessful = true;
            }
            catch (Exception ex)
            {

            }
        }
    }
}
