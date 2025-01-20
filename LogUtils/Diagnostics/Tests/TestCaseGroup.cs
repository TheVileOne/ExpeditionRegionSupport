using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogUtils.Diagnostics.Tests
{
    public class TestCaseGroup : TestCase, ICollection<TestCase>, ISelectable
    {
        public List<TestCase> Cases = new List<TestCase>();

        public override bool HasFailed
        {
            get
            {
                //Check if children have failed outcomes, as well as its own asserts
                return Cases.Exists(c => c.HasFailed) || base.HasFailed;
            }
        }

        public TestCase SelectedCase;

        protected int SelectedIndex;

        public TestCaseGroup(string name) : this(null, name)
        {
        }

        public TestCaseGroup(TestCaseGroup group, string name) : base(group, name, null)
        {
        }

        public int Count => Cases.Count;

        public bool IsReadOnly => false;

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

        public virtual string CreateReport()
        {
            int totalCases = Cases.Count;
            int totalPassedCases = Cases.Count(c => !c.HasFailed);

            bool allTestsPassed = totalPassedCases == Cases.Count;

            if (Results.Count > 0) //Include the group case as part of this count
            {
                allTestsPassed = !base.HasFailed;

                if (allTestsPassed)
                    totalPassedCases++;
                totalCases++;
            }

            StringBuilder reportBuilder = new StringBuilder();

            reportBuilder.AppendLine("Test Results")
                         .AppendLine(Name)
                         .AppendLine(allTestsPassed ? "All tests passed" : $"{totalPassedCases} out of {totalCases} tests passed");

            if (!allTestsPassed)
            {
                reportBuilder.AppendLine("Failed tests");
                foreach (var testCase in Cases.Where(c => c.HasFailed))
                {
                    if (testCase is TestCaseGroup group)
                    {
                        reportBuilder.AppendLine()
                                     .AppendLine(group.CreateReport());
                    }
                    else
                    {
                        reportBuilder.AppendLine(testCase.Name);
                        foreach (var failedAssert in testCase.Results.Where(r => !r.PassedWithExpectations()))
                        {
                            reportBuilder.AppendLine(failedAssert.ToString());
                        }
                    }
                }

                if (base.HasFailed)
                {
                    foreach (var failedAssert in Results.Where(r => !r.PassedWithExpectations()))
                    {
                        reportBuilder.AppendLine(failedAssert.ToString());
                    }
                }
            }

            return reportBuilder.ToString();
        }
    }
}
