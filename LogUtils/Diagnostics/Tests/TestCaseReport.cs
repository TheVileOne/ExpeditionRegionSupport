using LogUtils.Helpers;
using System.Collections.Generic;
using System.Text;

namespace LogUtils.Diagnostics.Tests
{
    public partial class TestCase
    {
        protected const string RESULT_DIVIDER = "--------------------------------------------";

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
            ReportSectionHeader(report, "Showing test results");
        }

        protected virtual void EndReport(StringBuilder report)
        {
            ReportSectionHeader(report, "End of results");
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

        public enum ReportVerbosity
        {
            Compact,
            Standard,
            Verbose
        }
    }
}
