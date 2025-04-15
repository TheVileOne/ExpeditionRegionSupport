using LogUtils.Helpers;
using LogUtils.Helpers.Extensions;
using System.Collections.Generic;
using System.Text;
using ReportVerbosity = LogUtils.Enums.FormatEnums.FormatVerbosity;

namespace LogUtils.Diagnostics.Tests
{
    public partial class TestCase
    {
        public virtual bool HasReportDetails()
        {
            return HasFailed();
        }

        public string CreateReport()
        {
            StringBuilder report = new StringBuilder();

            BeginReport(report);
            BuildReport(report);
            EndReport(report);
            return report.ToString();
        }

        internal void AppendReport(StringBuilder report)
        {
            BuildReport(report);
        }

        protected virtual void BuildReport(StringBuilder report)
        {
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
            else if (Debug.TestCasePolicy.ReportVerbosity == ReportVerbosity.Verbose)
            {
                int totalResults = Results.Count;
                report.AppendLine($"- {totalResults} out of {totalResults} asserts passed");
            }
            else
            {
                report.AppendLine("- All results passed");
            }
        }

        protected virtual void BeginReport(StringBuilder report)
        {
            report.AppendLine()
                  .AppendLine("Test Results");
            FormatUtils.CreateHeader(report, "Showing test results");
        }

        protected virtual void EndReport(StringBuilder report)
        {
            FormatUtils.CreateHeader(report, "End of results");
        }

        protected void ReportResultEntries(StringBuilder report, IEnumerable<Condition.Result> results)
        {
            const string RESPONSE_FORMAT = "({0}){1}";

            foreach (var result in results)
            {
                string response = string.Format(RESPONSE_FORMAT, result.ID, Formatter.Format(result));

                if (result.HasEmptyMessage)
                    report.Append(result.Passed ? "Pass" : "Fail");

                report.AppendLine(response);
            }
        }
    }
}
