using LogUtils.Helpers;
using System;
using System.Collections.Generic;
using System.Text;
using MessageFormatter = LogUtils.Diagnostics.AssertHandler.MessageFormatter;

namespace LogUtils.Diagnostics.Tests
{
    public class TestCase : IConditionHandler, IDisposable
    {
        public MessageFormatter Formatter;

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

            Formatter = new MessageFormatter();
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

        protected const string RESULT_DIVIDER = "--------------------------------------------";

        public string CreateReport()
        {
            StringBuilder report = new StringBuilder();

            BuildReport(report);
            return report.ToString();
        }

        public virtual void BuildReport(StringBuilder report)
        {
            //This header only needs to be displayed once
            if (report.Length == 0)
                BeginReport(report);

            bool testCaseFailed = HasFailed();

            report.AppendLine($"{(testCaseFailed ? "FAILED" : "PASSED")} - {Name}")
                  .AppendLine("INFO");

            if (Results.Count == 0)
            {
                report.AppendLine("- No results to show");
                return;
            }

            if (testCaseFailed)
            {
                var analyzer = Results.GetAnalyzer();

                analyzer.CountResults();

                int totalResults = analyzer.TotalResults,
                    totalPassedResults = analyzer.TotalPassedResults;

                report.AppendLine($"- {totalPassedResults} out of {totalResults} asserts passed")
                      .AppendLine();

                ReportResultEntries(report, analyzer.GetFailedResults());
            }
            else if (Debug.TestCasePolicy.AlwaysReportResultTotal)
            {
                int totalResults = Results.Count;
                report.AppendLine($"- {totalResults} out of {totalResults} asserts passed");
            }
            else
            {
                report.AppendLine("- All results passed");
            }
        }

        protected void BeginReport(StringBuilder report)
        {
            report.AppendLine()
                  .AppendLine("Test Results");
            ReportSectionHeader(report, "Showing test results");
        }

        protected void ReportSectionHeader(StringBuilder report, string sectionHeader)
        {
            report.AppendLine(RESULT_DIVIDER)
                  .AppendLine(sectionHeader)
                  .AppendLine(RESULT_DIVIDER);
        }

        protected void ReportResultEntries(StringBuilder report, IEnumerable<Condition.Result> results)
        {
            string response;
            foreach (var result in results)
            {
                response = Formatter.Format(result);

                if (result.HasEmptyMessage)
                    report.Append(result.Passed ? "Pass" : "Fail");

                report.AppendLine(response);
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
            var analyzer = Results.GetAnalyzer();
            return analyzer.HasFailedResults();
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
