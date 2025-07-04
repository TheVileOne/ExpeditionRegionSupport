using LogUtils.Console;
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
        /// The positional offset between the local build position, and the actual position in the formatted string
        /// </summary>
        public int BuildOffset;

        /// <summary>
        /// The index position in the local StringBuilder handling the format data
        /// </summary>
        public int LocalPosition;

        /// <summary>
        /// The index position of the format argument in the formatted string
        /// </summary>
        public int Position => LocalPosition + BuildOffset;

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

            /// <summary>
            /// Ansi color code escape sequence has been detected. Flag ensures that the code itself isn't considered as including colorable characters
            /// </summary>
            public bool ExpectAnsiCode;

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

                //We are not expecting there to be format information in the string here
                if (!ExpectAnsiCode && (RangeCounter == 0 || numCharsSinceLastArgument == 0))
                {
                    string unprocessedBuildString = currentBuilder.ToString().Substring(currentBuilder.Length - numCharsSinceLastArgument);

                    UtilityLogger.DebugLog($"'{unprocessedBuildString}' will remain at the last assigned color");
                    currentBuildEntry.LastCheckedBuildLength = currentBuilder.Length;
                    return false;
                }

                bool ansiTerminatorDetected = false;
                for (int i = currentBuilder.Length - numCharsSinceLastArgument; i < currentBuilder.Length; i++)
                {
                    char buildChar = currentBuilder[i];

                    if (buildChar == AnsiColorConverter.ANSI_ESCAPE_CHAR) //ANSI escape character
                    {
                        UtilityLogger.DebugLog("ANSI code encountered");
                        ansiTerminatorDetected = false; //Reset in case there are multiple codes in the build string
                        RangeCounter = 0;
                        ExpectAnsiCode = true;
                        continue;
                    }

                    if (ExpectAnsiCode)
                    {
                        if (!ansiTerminatorDetected)
                        {
                            //We don't want to reset ANSI flag inside the loop
                            if (buildChar == AnsiColorConverter.ANSI_TERMINATOR_CHAR)
                            {
                                UtilityLogger.DebugLog("ANSI code terminated");
                                ansiTerminatorDetected = true;
                            }
                        }
                        else //Show build characters that are after an ANSI code, but not the code itself
                        {
                            UtilityLogger.DebugLog(buildChar);
                        }
                        continue;
                    }

                    UtilityLogger.DebugLog(buildChar);

                    //Check that character is not a format-specific escape character, whitespace, or an ANSI color code character
                    bool canHaveColor = buildChar < '\a' || (buildChar > '\r' && buildChar != ' ');

                    if (canHaveColor)
                        RangeCounter--;

                    if (RangeCounter == 0)
                    {
                        currentBuildEntry.LastCheckedBuildLength = i;
                        break;
                    }
                }

                try
                {
                    if (ansiTerminatorDetected)
                    {
                        ExpectAnsiCode = false;
                        currentBuildEntry.LastCheckedBuildLength = currentBuilder.Length;
                        return false;
                    }

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
