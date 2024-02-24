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
        public int FailedAttempts;

        public bool Success => Challenge != null;
        
        public Challenge Challenge;
    }
}
