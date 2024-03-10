using Expedition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpeditionRegionSupport.ExceptionHandling
{
    public class ChallengeFilterExceptionHandler
    {
        public void HandleException(Challenge source, Exception ex)
        {
            string exMessage = "An error occurred while generating the challenge type " + source.ChallengeName();

            Plugin.Logger.LogError(exMessage);

            //This exception has a distinctly known cause, and is worthy of being caught and ignored
            if (ex is IndexOutOfRangeException)
            {
                string exDetails = "Index was out of range. The most likely cause is an improperly handled challenge filter.";
                Plugin.Logger.LogError(exDetails);
                Plugin.Logger.LogError(ex);
                return;
            }
            throw ex;
        }
    }
}
