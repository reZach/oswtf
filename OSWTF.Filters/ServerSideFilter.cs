using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http;
using OSWTF.Models;
using System;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace OSWTF.Filters
{
    public class ServerSideFilter : IActionFilter
    {
        private string cookiePrefix = "oswtf_";
        private int[] trafficSplit;
        private string[] excluded;
        private DateTime? expires;
        private bool isSuccessful = false;
        private List<string> preExecuteErrors = new List<string>();
        private TestEligibility eligibilities = new TestEligibility();
        private List<TestingOptions.Test> testsToBeRan = new List<TestingOptions.Test>();
        private readonly string[] _testNames;
        private readonly IOptions<TestingOptions> _options;

        public ServerSideFilter(string[] testNames, IOptions<TestingOptions> options)
        {
            _testNames = testNames;
            _options = options;

            ParseOptions();
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            // Don't need to do anything here
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (!isSuccessful)
            {
                eligibilities.Errors.AddRange(preExecuteErrors);
                AssignViewBagProperty(context);
                return;
            }
                

            
            string cookieValue = string.Empty;
            string cookieName = cookiePrefix + _testName;
            bool passesExclusionTest = true;

            // If user is in any of the excluded test,
            // don't run this test
            for (int i = 0; i < excluded.Length; i++)
            {
                if (!context.HttpContext.Request.Cookies.ContainsKey(cookiePrefix + excluded[i])) continue;

                passesExclusionTest = false;
                break;
            }


            if (!passesExclusionTest)
            {
                // If user does not have the test cookie
                if (!context.HttpContext.Request.Cookies.ContainsKey(cookieName))
                {
                    // Variation the user into a segment
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
                    cookieValue = Convert.ToString((1 + bucket));
                    context.HttpContext.Response.Cookies.Append(cookieName,
                        cookieValue, new CookieOptions
                        {
                            Domain = context.HttpContext.Request.Host.Host,
                            Path = "/",
                            Expires = expires,
                            Secure = context.HttpContext.Request.IsHttps,
                            IsEssential = true
                        });
                }
                else
                {
                    cookieValue = context.HttpContext.Request.Cookies[cookieName];
                }
            }
            else
            {
                // Default to base variation
                cookieValue = "1";
            }
            

            // Assign value
            eligibilities.IndividualTestEligibilities.Add(new IndividualTestEligibility
            {
                TestName = _testName,
                Variation = cookieValue
            });

            AssignViewBagProperty(context);
        }

        private void ParseOptions()
        {            
            // Short-circuit
            if (_options.Value.Tests != null &&
                _options.Value.Tests.Count > 0)
            {
                TestingOptions.Test individualTest;

                for (int i = 0; i < _testNames.Length; i++)
                {
                    individualTest = _options.Value.Tests.Find(
                        t => t.Name == _testNames[i]);

                    if (individualTest == null)
                    {
                        preExecuteErrors.Add(
                            $"Could not find the test with name '{_testNames[i]}' present in the options.");
                        break;
                    }

                    testsToBeRan.Add(individualTest);

                    // Validate each of the tests are formatted correctly
                    if (individualTest.Name == null)
                    {
                        preExecuteErrors.Add(
                            $"The key 'Name' does not exist for test '{individualTest.Name}' in options.");
                        return;
                    }

                    if (individualTest.Split == null)
                    {
                        preExecuteErrors.Add(
                            $"The key 'Split' does not exist for test '{individualTest.Name}' in options.");
                        return;
                    }

                    if (individualTest.Begin == default(DateTime))
                    {
                        preExecuteErrors.Add(
                            $"The key 'Begin' does not exist for test '{individualTest.Name}' in options.");
                        return;
                    }

                    try
                    {
                        // Parse values
                        trafficSplit = Array.ConvertAll(
                            thisTest.Split.Replace(" ", "").Split(','), Convert.ToInt32);
                        excluded = thisTest.Exclude ?? new string[0];
                        expires = thisTest.End != null ? thisTest.End : (DateTime?)null;
                        cookiePrefix = _options.Value.Prefix ?? cookiePrefix;

                        isSuccessful = true;
                    }
                    catch (Exception ex)
                    {
                        preExecuteErrors.Add(
                            $"Error occured while parsing options: '{ex.InnerException}'.");
                        return;
                    }
                }           
            }
            else
            {
                preExecuteErrors.Add("Could not bind to any tests in options.");
            }                
        }

        private void AssignViewBagProperty(ActionExecutingContext context)
        {
            // If our controller is set up properly,
            // set the value of our cookie
            if (context.Controller is Controller controller)
            {
                controller.ViewBag.Eligibilities = eligibilities;
            }
        }
    }
}
