using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using OSWTF.Server.Models;

namespace OSWTF.Server
{
    public class ServerSideTestFilter : IActionFilter
    {
        private bool isSuccessful;
        private readonly IOptions<TestingOptions> _options;
        private readonly string[] _testNames;
        private readonly string _cookiePrefix = "oswtf_";
        private readonly Result result = new Result();        
        private readonly List<string> preExecuteErrors = new List<string>();
        private readonly List<TestToBeRan> testsToBeRan = new List<TestToBeRan>();

        public ServerSideTestFilter(string[] testNames, IOptions<TestingOptions> options)
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
                result.Errors.AddRange(preExecuteErrors);
                AssignViewBagProperty(context);
                return;
            }


            for (var i = 0; i < testsToBeRan.Count; i++)
            {
                var cookieValue = string.Empty;
                var cookieName = _cookiePrefix + testsToBeRan[i].Name;
                var passesExclusionTest = true;
                
                // Only process logic if test is active
                if (!testsToBeRan[i].TurnOff)
                {
                    // If user is in any of the excluded test, don't run this test;
                    // don't run this logic if we are forcing a variation for a test
                    if (string.IsNullOrEmpty(testsToBeRan[i].ForceVariation))
                    {
                        for (var j = 0; j < testsToBeRan[i].Exclude.Length; j++)
                        {
                            if (!context.HttpContext.Request.Cookies.ContainsKey(
                                _cookiePrefix + testsToBeRan[i].Exclude[j])) continue;

                            passesExclusionTest = false;
                            break;
                        }
                    }


                    if (passesExclusionTest)
                    {
                        // If user does not have the test cookie
                        if (!context.HttpContext.Request.Cookies.ContainsKey(cookieName))
                        {
                            // Force a variation if possible
                            if (!string.IsNullOrEmpty(testsToBeRan[i].ForceVariation))
                            {
                                cookieValue = testsToBeRan[i].ForceVariation;
                            }
                            else
                            {
                                // Variation the user into a segment
                                var roller = new Random();
                                var roll = roller.Next(1, 101);

                                var sum = 0;
                                int bucket;
                                for (bucket = 0; bucket < testsToBeRan[i].Split.Length; bucket++)
                                {
                                    sum += testsToBeRan[i].Split[bucket];

                                    if (roll <= sum)
                                        break;
                                }
                                
                                cookieValue = Convert.ToString(bucket);
                            }

                            // Create the cookie on the user's response
                            context.HttpContext.Response.Cookies.Append(cookieName,
                                cookieValue, new CookieOptions
                                {
                                    Domain = context.HttpContext.Request.Host.Host,
                                    Path = "/",
                                    Expires = testsToBeRan[i].End,
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
                        cookieValue = "0";
                    }
                }
                else
                {
                    // Default to base variation
                    cookieValue = "0";
                }

                // Assign value
                result.IndividualResults.Add(new IndividualResult
                {
                    TestName = testsToBeRan[i].Name,
                    Variation = cookieValue
                });
            }

            AssignViewBagProperty(context);
        }

        private void ParseOptions()
        {            
            if (_options.Value.Tests != null &&
                _options.Value.Tests.Count > 0)
                for (var i = 0; i < _testNames.Length; i++)
                {
                    var individualTest = _options.Value.Tests.Find(
                        t => t.Name == _testNames[i]);

                    if (individualTest == null)
                    {
                        preExecuteErrors.Add(
                            $"Could not find the test with name '{_testNames[i]}' present in the options.");
                        break;
                    }


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

                    if (!string.IsNullOrEmpty(individualTest.ForceVariation))
                    {                        
                        if (!Int32.TryParse(individualTest.ForceVariation, out int forceVariation))
                        {
                            preExecuteErrors.Add(
                                $"The key 'ForceVariation' does not convert to a data type of 'int' for test '{individualTest.Name}' in options.");
                            return;
                        }

                        if (forceVariation >= individualTest.Split.Length)
                        {
                            preExecuteErrors.Add(
                                $"The key 'ForceVariation' does not map to the existing key 'Split' properly for test '{individualTest.Name}' in options.");
                            return;
                        }
                    }

                    try
                    {
                        // Parse values
                        testsToBeRan.Add(new TestToBeRan
                        {
                            Name = individualTest.Name,
                            Split = Array.ConvertAll(
                                individualTest.Split.Replace(
                                    " ", "").Split(','), Convert.ToInt32),
                            Begin = individualTest.Begin,
                            End = individualTest.End == default(DateTime)
                                ? new DateTime(3000, 1, 1)
                                : individualTest.End,
                            Exclude = individualTest.Exclude ?? new string[0],
                            TurnOff = individualTest.TurnOff,
                            ForceVariation = individualTest.ForceVariation
                        });

                        isSuccessful = true;
                    }
                    catch (Exception ex)
                    {
                        preExecuteErrors.Add(
                            $"Error occured while parsing options for test '{individualTest.Name}'. Error : '{ex.InnerException}'.");
                        return;
                    }
                }
            else
                preExecuteErrors.Add("Could not bind to any tests in options.");
        }

        private void AssignViewBagProperty(ActionExecutingContext context)
        {
            // If our controller is set up properly, set the value of our cookie            
            if (context.Controller is Controller controller) controller.ViewBag.ServerSideTests = result;
        }
    }
}