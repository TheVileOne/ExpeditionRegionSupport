﻿using System.Collections.Generic;
using System.Linq;

namespace LogUtils.Diagnostics
{
    public class ResultAnalyzer
    {
        private IEnumerable<Condition.Result> resultData;

        public int TotalResults;
        public int TotalPassedResults;
        public int TotalFailedResults;

        private Condition.Result _currentResult;

        public ResultAnalyzer(IEnumerable<Condition.Result> results)
        {
            resultData = results;
        }

        public void ClearResults()
        {
            _currentResult = default;

            TotalResults =
            TotalPassedResults =
            TotalFailedResults = 0;
        }

        public void CountResults()
        {
            ClearResults();

            var enumerator = resultData.GetEnumerator();

            while (enumerator.MoveNext())
            {
                _currentResult = enumerator.Current;

                TotalResults++;

                Condition.State resultState = analyzeResult();

                if (resultState == Condition.State.Pass)
                    TotalPassedResults++;
                else if (resultState == Condition.State.Fail)
                    TotalFailedResults++;
            }
        }

        public IEnumerable<Condition.Result> GetFailedResults()
        {
            return resultData.Where(result =>
            {
                _currentResult = result;

                Condition.State resultState = analyzeResult();

                return resultState == Condition.State.Fail;
            });
        }

        private Condition.State analyzeResult()
        {
            bool testResultFailed = Debug.TestCasePolicy.PreferExpectationsAsFailures
                            ? !_currentResult.PassedWithExpectations() || (Debug.TestCasePolicy.FailuresAreAlwaysReported && !_currentResult.Passed)
                            : !_currentResult.Passed;
            return testResultFailed ? Condition.State.Fail : Condition.State.Pass;
        }
    }
}
