using System.Collections.Generic;

namespace LogUtils
{
    public class RemoteLoggerComparer : Comparer<BetaLogger>
    {
        public const int MAX_SCORE = 2;

        public static readonly RemoteLoggerComparer DefaultComparer = new RemoteLoggerComparer();

        /// <summary>
        /// Returns an integer representation of the better candidate for remote logging
        /// </summary>
        public override int Compare(BetaLogger logger, BetaLogger otherLogger)
        {
            // 1 means imperfect score
            // 2 means perfect score
            if (logger.AllowRemoteLogging == otherLogger.AllowRemoteLogging)
            {
                if (logger.AllowLogging == otherLogger.AllowLogging) //Same value
                    return 0;

                if (otherLogger.AllowLogging)
                    return logger.AllowRemoteLogging ? MAX_SCORE : 1;
            }
            else if (!logger.AllowRemoteLogging) //This means otherLogger allows it
            {
                if (otherLogger.AllowLogging)
                    return MAX_SCORE;

                return 1; //Situation always favors otherLogger
            }
            return -1;
        }
    }
}
