using LogUtils.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogUtils.Diagnostics.Tests
{
    public class TestCaseGroup : TestCase, ICollection<TestCase>, ISelectable
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

        public List<TestCase> Cases = new List<TestCase>();

        public int Count => Cases.Count;

        public bool IsReadOnly => false;

        public TestCase SelectedCase;

        protected int SelectedIndex;

        public TestCaseGroup(string name) : this(null, name)
        {
        }

        public TestCaseGroup(TestCaseGroup group, string name) : base(group, name, null)
        {
        }

        public void Add(TestCase test)
        {
            if (test.Group == this) return;

            if (test.Group != null)
                test.Group.Remove(test);
            test.SetGroupFromParent(this); //Use a method here to avoid potential for inf loops
            Cases.Add(test);
        }

        public bool Remove(TestCase test)
        {
            bool caseRemoved = Cases.Remove(test);

            if (caseRemoved)
            {
                test.SetGroupFromParent(null); //Use a method here to avoid potential for inf loops

                //Was the selected case removed?
                if (SelectedCase == test)
                {
                    if (SelectedIndex < Cases.Count) //The next case at that index now becomes the new selected case
                    {
                        SelectedCase = Cases[SelectedIndex];
                    }
                    else if (Cases.Count > 0) //The selected case was at the end of the list
                    {
                        SelectPrev();
                    }
                }
                else
                {
                    //We don't know if this affected the index of the selected case - reassign the index
                    SelectedIndex = Cases.IndexOf(test);
                }
            }
            return caseRemoved;
        }

        public void Clear()
        {
            SelectedIndex = 0;
            SelectedCase = null;

            Cases.ForEach(c => c.SetGroupFromParent(null));
            Cases.Clear();
        }

        public bool Contains(TestCase test)
        {
            return Cases.Contains(test);
        }

        public void CopyTo(TestCase[] array, int arrayIndex)
        {
            Cases.CopyTo(array, arrayIndex);
        }

        public IEnumerator<TestCase> GetEnumerator()
        {
            return Cases.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Cases.GetEnumerator();
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
            SelectedIndex = Math.Max(0, Cases.Count - 1);
            SelectedCase = Cases.LastOrDefault();
        }

        public void SelectPrev()
        {
            SelectedIndex = Math.Max(0, SelectedIndex - 1);
            SelectedCase = Cases.ElementAtOrDefault(SelectedIndex);
        }

        public void SelectNext()
        {
            SelectedIndex = Math.Min(Cases.Count - 1, SelectedIndex + 1);
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

            //Hacky solution to ensure that all subgroups with reportable results display this header
            if (Group != null && Debug.TestCasePolicy.ReportVerbosity != ReportVerbosity.Compact)
                ReportSectionHeader(report, $"Showing test cases of {Name}");

            //We don't need to report on assert count if it 1:1 aligns with the case results
            if (totalAsserts != totalCases && totalPassedAsserts != totalPassedCases)
            {
                report.AppendLine($"- {totalPassedAsserts} out of {totalAsserts} asserts passed");
            }

            if (!allTestsPassed || Debug.TestCasePolicy.ReportVerbosity != ReportVerbosity.Compact)
            {
                if (base.HasFailed())
                {
                    var analyzer = Results.GetAnalyzer();
                    ReportResultEntries(report, analyzer.GetFailedResults());
                }

                var casesToReport = Debug.TestCasePolicy.ReportVerbosity != ReportVerbosity.Compact ? Cases : Cases.Where(c => c.HasReportDetails());

                TestCase lastTestProcessed = null;
                foreach (var testCase in casesToReport)
                {
                    if (testCase is TestCaseGroup)
                    {
                        ReportSectionHeader(report, $"Showing subgroup of {Name}");
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

            if (Group != null)
                ReportSectionHeader(report, $"Finished showing test group {Name}");
        }
    }
}
