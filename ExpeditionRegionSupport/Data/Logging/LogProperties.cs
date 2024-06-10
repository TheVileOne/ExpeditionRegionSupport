using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ExpeditionRegionSupport.Data.Logging
{
    public class LogProperties
    {
        private LoggerID _logID;
        public LoggerID LogID
        {
            get
            {
                if (_logID == null)
                    _logID = new LoggerID(Filename, false);
                return _logID;
            }
        }

        /// <summary>
        /// The filename that will be used in the typical write path for the log file
        /// </summary>
        public string Filename;

        /// <summary>
        /// The filename that will be used if the write path is the LogManager Logs directory. May be null if same as Filename
        /// </summary>
        public string AltFilename;

        /// <summary>
        /// A list of filenames that should be considered equal to Filename/AltFilename
        /// </summary>
        public List<string> Aliases = new List<string>();

        protected List<LogRule> Rules = new List<LogRule>();

        public void AddRule(LogRule rule)
        {
            if (Rules.Exists(r => r.ID == rule.ID)) return;

            if (Rules.Count == 0)
            {
                Rules.Add(rule);
                return;
            }

            //The code below ensures that priority rules are last in the list. Rules that are applied last in the list are applied last to the log message
            switch (rule.ID)
            {
                case LogRule.Rule.ShowCategory:
                    if (Rules[Rules.Count - 1].ID == LogRule.Rule.ShowLineCount) //Line count should apply before category formatting
                        Rules.Insert(Rules.Count - 1, rule);
                    else
                        Rules.Add(rule); //Category formatting takes priority over every other rule
                    break;
                case LogRule.Rule.ShowLineCount:
                    Rules.Add(rule);
                    break;
                case LogRule.Rule.Unknown:
                    if (Rules[Rules.Count - 1].ID == LogRule.Rule.ShowCategory) //Insert before prioritized rules
                        Rules.Insert(Rules.Count - 1, rule);
                    else if (Rules[Rules.Count - 1].ID == LogRule.Rule.ShowLineCount) //ShowLineCount takes priority
                    {
                        //Inserts before one, or both prioritized rules
                        if (Rules.Count == 1 || Rules[Rules.Count - 2].ID != LogRule.Rule.ShowCategory)
                            Rules.Insert(Rules.Count - 1, rule);
                        else
                            Rules.Insert(Rules.Count - 2, rule);
                    }
                    else
                        Rules.Add(rule); //There are no prioritized rules if this triggers
                    break;
            }
        }

        public string Serialize()
        {
            return JsonConvert.SerializeObject(this);
        }

        public static LogProperties Deserialize(string propertyString)
        {
            return JsonConvert.DeserializeObject<LogProperties>(propertyString);
        }
    }

    public class LogRule
    {
        public Rule ID = Rule.Unknown; 

        public virtual string ApplyRule(string message)
        {
            return message;
        }

        public enum Rule
        {
            Unknown = -1,
            ShowCategory,
            ShowLineCount
        }
    }
}
