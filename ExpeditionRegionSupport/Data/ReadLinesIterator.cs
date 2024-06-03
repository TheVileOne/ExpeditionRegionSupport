using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpeditionRegionSupport.Data
{
    public class ReadLinesIterator
    {
        protected StreamReader Stream { get; }

        /// <summary>
        /// Apply conditions that pass over text data that matches certain criteria
        /// </summary>
        public List<StringDelegates.Validate> SkipConditions = new List<StringDelegates.Validate>();

        public ReadLinesIterator(StreamReader stream)
        {
            Stream = stream;
        }

        public ReadLinesIterator(StreamReader stream, StringDelegates.Validate skipCondition) : this(stream)
        {
            if (skipCondition != null)
                SkipConditions.Add(skipCondition);
        }

        public virtual IEnumerable<string> GetEnumerable()
        {
            string line;
            do
            {
                line = Stream.ReadLine();

                if (line == null)
                    yield break;

                if (checkSkipConditions(line)) //Checks that line conforms with all given skip conditions
                    continue;

                yield return line;
            }
            while (line != null);
        }

        private bool checkSkipConditions(string line)
        {
            return SkipConditions.Exists(hasFailedCheck => hasFailedCheck(line));
        }
    }
}
