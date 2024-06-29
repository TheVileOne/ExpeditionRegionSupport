using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogUtils
{
    /// <summary>
    /// A class for storing log details until a logger is available to process the request
    /// </summary>
    public class LogRequest
    {
        /// <summary>
        /// General identifier string often used to provide additional user feedback 
        /// </summary>
        public string Category;

        /// <summary>
        /// A pending message waiting to be logged to file
        /// </summary>
        public string Message;
    }
}
