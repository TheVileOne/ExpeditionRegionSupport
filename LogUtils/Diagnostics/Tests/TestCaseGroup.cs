using LogUtils.Helpers.Extensions;
using LogUtils.Policy;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ReportVerbosity = LogUtils.Enums.FormatEnums.FormatVerbosity;

namespace LogUtils.Diagnostics.Tests
{
    public class TestCaseGroup : TestCase, IReadOnlyCollection<TestCase>, ISelectable
    {
        /// <summary>
        /// Gets the test results for the test case and any children cases starting with the test group
        /// </summary>
        public IEnumerable<Condition.Result> AllResults
        {
            get
            {
                //Yield back test results for the current group
                foreach (var result in Results)
                    yield return result;

                //Select results from all child test cases, including any of their children
                var childResults = Cases.SelectMany(testCase =>
                {
                    TestCaseGroup testGroup = testCase as TestCaseGroup;

                    if (testGroup != null)
                        return testGroup.AllResults;
                    return testCase.Results;
                });

                //Yield back test results for its children
                foreach (var result in childResults)
                    yield return result;
                yield break;
            }
        }

        public IEnumerable<TestCase> AllCases
        {
            get
            {
                //Yield back test cases for the current group
                foreach (var testCase in Cases)
                    yield return testCase;

                //Select results from all child test cases, including any of their children
                var childCases = Cases.SelectMany(testCase =>
                {
                    TestCaseGroup testGroup = testCase as TestCaseGroup;

                    if (testGroup != null)
                        return testGroup.AllCases;
                    return new[] { testCase };
                });

                //Yield back test cases for its children
                foreach (var testCase in childCases)
                    yield return testCase;
                yield break;
            }
        }

        protected List<TestCase> Cases = new List<TestCase>();

        /// <inheritdoc/>
        public int Count => Cases.Count;

        public TestCase SelectedCase;

        protected int SelectedIndex;

        public TestCaseGroup(string name) : base(name)
        {
        }

        public TestCaseGroup(TestCaseGroup group, string name) : base(group, name)
        {
        }

        internal void Add(TestCase test)
        {
            if (test == null || test == this)
            {
                UtilityLogger.LogWarning("Test case argument invalid");
                return;
            }
            Cases.Add(test);
        }

        public bool Contains(TestCase test)
        {
            return Cases.Contains(test);
        }

        /// <inheritdoc/>
        public IEnumerator<TestCase> GetEnumerator()
        {
            return Cases.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Cases.GetEnumerator();
        }

        public override void Handle(Condition.Result result)
        {
            var firstHandler = result.Handlers[0];

            if (firstHandler.Equals(this))
                base.Handle(result);
        }

        /// <summary>
        /// Checks that any test cases under the group have failed outcomes, as well as its own asserts
        /// </summary>
        public override bool HasFailed()
        {
            return Cases.Exists(c => c.HasFailed()) || base.HasFailed();
        }

        public void PreviousCase()
        {
            SelectPrev();
        }

        public void NextCase()
        {
            SelectNext();
        }

        public void SelectFirst()
        {
            SelectedIndex = 0;
            SelectedCase = Cases.FirstOrDefault();
        }

        public void SelectLast()
        {
            SelectedIndex = Math.Max(0, Count - 1);
            SelectedCase = Cases.LastOrDefault();
        }

        public void SelectPrev()
        {
            SelectedIndex = Math.Max(0, SelectedIndex - 1);
            SelectedCase = Cases.ElementAtOrDefault(SelectedIndex);
        }

        public void SelectNext()
        {
            SelectedIndex = Math.Min(Count - 1, SelectedIndex + 1);
            SelectedCase = Cases.ElementAtOrDefault(SelectedIndex);
        }

        protected override void BuildReport(StringBuilder report)
        {
            int totalCases = Count, //Count includes only cases immediately managed by this instance
                totalPassedCases = 0,
                totalAsserts = 0, //Count includes all asserts from test cases belonging to this instance, and managed by children
                totalPassedAsserts = 0;

            //Check for results that do not belong to a child test case
            if (Results.Count > 0)
            {
                bool testCaseFailed = base.HasFailed();

                if (!testCaseFailed)
                    totalPassedCases++;
                totalCases++;

                //TODO: This code needs more thorough bug testing
                var analyzer = Results.GetAnalyzer();

                analyzer.CountResults();

                totalAsserts += analyzer.TotalResults;
                totalPassedAsserts += analyzer.TotalPassedResults;
            }

            //Check for results that belong to child test cases
            foreach (var testCase in Cases)
            {
                bool testCaseFailed = testCase.HasFailed();

                if (!testCaseFailed)
                    totalPassedCases++;

                TestCaseGroup testGroup = testCase as TestCaseGroup;

                IEnumerable<Condition.Result> testResults = testGroup != null ? testGroup.AllResults : testCase.Results;

                //Count the total amount of asserts for this test case, and how many were passed asserts
                if (testCaseFailed)
                {
                    var analyzer = testResults.GetAnalyzer();

                    analyzer.CountResults();

                    totalAsserts += analyzer.TotalResults;
                    totalPassedAsserts += analyzer.TotalPassedResults;
                }
                else
                {
                    int totalResults = testResults.Count();

                    //All tests passed for this case - include every result in the count
                    totalAsserts += totalResults;
                    totalPassedAsserts += totalResults;
                }
            }

            report.AppendLine($"REPORT - {Name}")
                  .AppendLine("INFO");

            if (Results.Count == 0 && Count == 0)
            {
                report.AppendLine("- No results to show");
                return;
            }

            bool allTestsPassed = totalPassedCases == totalCases;

            //Basic statistics on test failures
            report.AppendLine($"- {totalPassedCases} out of {totalCases} tests passed");

            if (Cases.Count > 0 && Group != null && TestCasePolicy.ReportVerbosity != ReportVerbosity.Compact)
                report.AppendHeader($"Showing test cases of {Name}");

            //We don't need to report on assert count if it 1:1 aligns with the case results
            if (totalAsserts > 0 && totalAsserts != totalCases && totalPassedAsserts != totalPassedCases)
            {
                report.AppendLine($"- {totalPassedAsserts} out of {totalAsserts} asserts passed");
            }

            if (!allTestsPassed || TestCasePolicy.ReportVerbosity != ReportVerbosity.Compact)
            {
                if (base.HasFailed())
                {
                    var analyzer = Results.GetAnalyzer();
                    ReportResultEntries(report, analyzer.GetFailedResults());
                }

                var casesToReport = TestCasePolicy.ReportVerbosity != ReportVerbosity.Compact ? Cases : Cases.Where(c => c.HasReportDetails());

                TestCase lastTestProcessed = null;
                foreach (var testCase in casesToReport)
                {
                    if (testCase is TestCaseGroup)
                    {
                        report.AppendHeader($"Showing subgroup of {Name}");
                    }
                    else if (lastTestProcessed != null) //This shouldn't apply to the first test case
                    {
                        report.AppendLine();
                    }

                    testCase.AppendReport(report);
                    lastTestProcessed = testCase;
                }
            }
            else
            {
                report.AppendLine("- All tests passed");
            }

            if (Cases.Count > 0 && Group != null)
                report.AppendHeader($"Finished showing test group {Name}");
        }
    }
}
