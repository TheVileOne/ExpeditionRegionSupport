using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;

namespace LogUtils.Formatting
{
    /// <summary>
    /// A data class for storing format informations
    /// </summary>
    public sealed class FormatData
    {
        /// <summary>
        /// The format argument
        /// </summary>
        public object Argument;

        /// <summary>
        /// Checks that Argument is a UnityEngine.Color
        /// </summary>
        public bool IsColorData => Argument is Color;

        /// <summary>
        /// The index of the first character of the format placeholder in the format string
        /// </summary>
        public int Position;

        /// <summary>
        /// The number of valid chars to apply color formatting
        /// </summary>
        public int Range;
    }

    internal static class FormatDataAccess
    {
        private static readonly ConditionalWeakTable<IColorFormatProvider, Data> colorFormatCWT = new();

        internal static Data GetData(this IColorFormatProvider self) => colorFormatCWT.GetValue(self, _ => new());

        internal sealed class Data
        {
            /// <summary>
            /// Collection of active format data being processed
            /// </summary>
            public LinkedList<NodeData> Entries = new LinkedList<NodeData>();

            /// <summary>
            /// A check on how many characters are needed to satisfy an argument's range requirement
            /// </summary>
            public int RangeCounter;

            public void AddNodeEntry(StringBuilder builder)
            {
                NodeData data = new NodeData()
                {
                    Builder = builder
                };
                Entries.AddLast(new LinkedListNode<NodeData>(data));
            }

            public void RemoveLastNodeEntry()
            {
                Entries.RemoveLast();
            }

            public bool UpdateBuildLength()
            {
                UtilityLogger.DebugLog("Updating");

                LinkedListNode<NodeData> currentNode = Entries.Last;

                NodeData currentBuildEntry = currentNode.Value;
                StringBuilder currentBuilder = currentBuildEntry.Builder;

                //UtilityLogger.DebugLog("Build position: " + currentBuildEntry.LastCheckedBuildLength);

                int numCharsSinceLastArgument = currentBuilder.Length - currentBuildEntry.LastCheckedBuildLength;

                if (RangeCounter == 0 || numCharsSinceLastArgument == 0)
                    return false;

                for (int i = currentBuilder.Length - numCharsSinceLastArgument; i < currentBuilder.Length; i++)
                {
                    char buildChar = currentBuilder[i];

                    UtilityLogger.DebugLog(buildChar);

                    //Check that character is not a format-specific escape character, whitespace, or an ANSI color code character
                    bool canHaveColor = buildChar < '\a' || (buildChar > '\r' && buildChar != ' ' && buildChar != '\x1b');

                    if (canHaveColor)
                        RangeCounter--;
                    else if (buildChar == '\x1b') //ANSI escape sequence - we need to skip all chars up until the next 'm'
                        RangeCounter = 0;

                    if (RangeCounter == 0)
                    {
                        currentBuildEntry.LastCheckedBuildLength = i;
                        break;
                    }
                }

                try
                {
                    if (RangeCounter > 0)
                    {
                        currentBuildEntry.LastCheckedBuildLength = currentBuilder.Length;
                        return false;
                    }
                    return true;
                }
                finally
                {
                    UtilityLogger.DebugLog("Build position after: " + currentBuildEntry.LastCheckedBuildLength);

                    if (RangeCounter > 0)
                        UtilityLogger.DebugLog($"Expecting {RangeCounter} more characters");
                }
            }
        }

        internal class NodeData
        {
            /// <summary>
            /// The builder in control of building the formatted string
            /// </summary>
            public StringBuilder Builder;

            /// <summary>
            /// The format argument currently being processed
            /// </summary>
            public FormatData Current;

            public bool HasArguments => Current != null;

            public int LastCheckedBuildLength;
        }
    }
}
