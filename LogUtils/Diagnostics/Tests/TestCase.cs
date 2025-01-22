using LogUtils.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace LogUtils.Diagnostics.Tests
{
    public class TestCase : IConditionHandler, IDisposable
    {
        private TestCaseGroup _group;

        /// <summary>
        /// The group that this test case belongs to
        /// </summary>
        public TestCaseGroup Group => _group;

        /// <summary>
        /// The result processor specific to this test case or its children. Null by default
        /// </summary>
        public IConditionHandler Handler;

        public virtual bool IsEnabled => Debug.AssertsEnabled;

        public string Name { get; }

        public List<Condition.Result> Results;

        public TestCase(string name) : this(null, name, null)
        {
        }

        public TestCase(string name, IConditionHandler handler) : this(null, name, handler)
        {
        }

        public TestCase(TestCaseGroup group, string name) : this(group, name, null)
        {
        }

        public TestCase(TestCaseGroup group, string name, IConditionHandler handler)
        {
            if (group != null)
                group.Add(this);

            Handler = handler ?? this;
            Name = name;
            Results = new List<Condition.Result>();
        }

        /// <summary>
        /// Creates a structure for asserting the state of a specified value
        /// </summary>
        /// <param name="value">Value to be used as an assert target</param>
        public Condition<T> AssertThat<T>(T value)
        {
            return Assert.That(value, Handler);
        }

        /// <summary>
        /// Creates a structure for asserting the state of a specified value
        /// </summary>
        /// <param name="value">Value to be used as an assert target</param>
        public Condition<T?> AssertThat<T>(T? value) where T : struct
        {
            return Assert.That(value, Handler);
        }

        public string CreateReport()
        {
            StringBuilder reportBuilder = new StringBuilder();
            
            CreateReport(reportBuilder);
            return reportBuilder.ToString();
        }

        public virtual void CreateReport(StringBuilder report)
        {
            //This header only needs to be displayed once
            if (report.Length == 0)
                report.AppendLine("Test Results");

            report.AppendLine("REPORT: " + Name);

            if (Results.Count == 0)
            {
                report.AppendLine("No results to show");
                return;
            }

            if (Debug.TestCasePolicy.AlwaysReportResultTotal)
                report.AppendLine($"Checking the results of {Results.Count} asserts");

            bool testCaseFailed = HasFailed();

            if (testCaseFailed)
            {
                var analyzer = Results.GetAnalyzer();

                analyzer.CountResults();

                int totalResults = analyzer.TotalResults,
                    totalPassedResults = analyzer.TotalPassedResults;

                report.AppendLine("Status: Failed")
                      .AppendLine()
                      .AppendLine($"{totalPassedResults} out of {totalResults} asserts passed")
                      .AppendLine();
                //.AppendLine("Failed asserts");

                foreach (var result in analyzer.GetFailedResults())
                {
                    report.AppendLine(result.ToString());
                }
            }
            else
            {
                report.AppendLine("All results passed")
                      .AppendLine();
            }
        }

        public void Dispose()
        {
            //Alert the case group that this case is finished handling cases, and the next test can take over
            if (Group != null)
                Group.NextCase();
        }

        protected virtual List<IConditionHandler> GetInheritedHandlers()
        {
            List<IConditionHandler> handlers = new List<IConditionHandler>();
            if (Group != null)
            {
                if (Group.Handler != null)
                    handlers.Add(Group.Handler);
                handlers.AddRange(Group.GetInheritedHandlers());
            }
            return handlers;
        }

        public virtual void Handle(in Condition.Result result)
        {
            Results.Add(result);
        }

        /// <summary>
        /// Checks that the test case has a failed outcome
        /// </summary>
        public virtual bool HasFailed()
        {
            bool includeExpectedOutcomes = Debug.TestCasePolicy.PreferExpectationsAsFailures;

            if (includeExpectedOutcomes)
                return Results.Exists(r => !r.PassedWithExpectations());
            return Results.Exists(r => !r.Passed);
        }

        public virtual bool HasReportDetails()
        {
            return Debug.TestCasePolicy.AlwaysReportResultTotal || HasFailed();
        }

        internal void SetGroupFromParent(TestCaseGroup group)
        {
            _group = group;
        }
    }
}
