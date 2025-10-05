using System;
using System.Collections.Generic;

namespace LogUtils.Helpers
{
    public static class ExceptionUtils
    {
        /// <summary>
        /// Takes an exception as input, and returns all InnerExceptions from it when it is an AggregateException, otherwise returns empty set
        /// </summary>
        public static ICollection<Exception> ExtractAggregate(Exception exception)
        {
            AggregateException aggregateException = exception as AggregateException;

            if (aggregateException != null)
                return aggregateException.InnerExceptions;
            return [];
        }
    }
}
