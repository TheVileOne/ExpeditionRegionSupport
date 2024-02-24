using Expedition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpeditionRegionSupport.Filters
{
    public class ChallengeRequestInfo
    {
        public int Slot = -1;

        /// <summary>
        /// The number of attempts it took to generate this challenge
        /// </summary>
        public int TotalAttempts => FailedAttempts + (Success ? 1 : 0);
        public int FailedAttempts;

        public bool Success => Challenge != null;
        
        public Challenge Challenge;

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"[{Slot}]");
            sb.AppendLine("Challenge Type");

            if (Success)
            {
                sb.Append(Challenge.ChallengeName());

                if (FailedAttempts > 0)
                {
                    sb.AppendLine();
                    sb.Append("Process Attempts " + TotalAttempts);
                }
            }
            else
            {
                sb.Append("ABORTED");
            }

            return sb.ToString();
        }
    }
}
