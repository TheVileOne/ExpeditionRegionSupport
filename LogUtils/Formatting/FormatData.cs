using LogUtils.Console;
using LogUtils.Diagnostics;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;
using static LogUtils.Formatting.FormatDataAccess;

namespace LogUtils.Formatting
{
    /// <summary>
    /// A data class for storing format information
    /// </summary>
    public sealed class FormatData
    {
        /// <summary>
        /// The format argument
        /// </summary>
        public object Argument;

        /// <summary>
        /// Checks that Argument is a <see cref="Color"/> or a <see cref="ConsoleColor"/>
        /// </summary>
        public bool IsColorData => Argument is Color || Argument is ConsoleColor;

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

        /// <summary>
        /// Converts format argument into a <see cref="Color"/> value
        /// </summary>
        /// <exception cref="InvalidCastException">The format argument cannot be cast to a <see cref="Color"/> value</exception>
        public Color GetColor()
        {
            if (Argument is ConsoleColor color)
                return ConsoleColorMap.GetColor(color);
            return (Color)Argument;
        }

        /// <summary>
        /// Converts argument into a <see cref="FormatData"/> object when passed an implementation of <see cref="IColorFormatProvider"/>, and the argument is a supported Color type 
        /// </summary>
        /// <param name="formatArgument">An argument value to a formatted string</param>
        /// <param name="formatter">Handler responsible for formatting <paramref name="formatArgument"/></param>
        /// <param name="commaValue">Alignment value of <paramref name="formatArgument"/>. This value is borrowed, and used as a range length.</param>
        /// <returns>The replacement for <paramref name="formatArgument"/>, or the same instance when extra data is not required</returns>
        internal static object ResolveArgument(object formatArgument, ICustomFormatter formatter, int commaValue)
        {
            var colorProvider = formatter as IColorFormatProvider;

            if (colorProvider != null)
                return ResolveArgument(formatArgument, colorProvider, commaValue);

            return formatArgument;
        }

        /// <inheritdoc cref="ResolveArgument(object, ICustomFormatter, int)"/>
        internal static object ResolveArgument(object formatArgument, IColorFormatProvider provider, int commaValue)
        {
            var data = provider.GetData();

            LinkedListNode<NodeData> currentNode = data.Entries.Last;

            FormatData currentEntry = currentNode.Value.Current;

            currentEntry.Argument = formatArgument;

            if (currentEntry.IsColorData)
            {
                Assert.That(data.RangeCounter).IsZero();

                currentEntry.Range = commaValue;
                data.RangeCounter = Math.Max(currentEntry.Range, 0);
                data.BypassColorCancellation = true; //When ANSI color codes are applied for the first time, it will cancel the range check if we don't set this bypass flag
                return currentEntry;
            }
            return formatArgument;
        }

        internal static void UpdateData(IColorFormatProvider provider)
        {
            var data = provider.GetData();

            LinkedListNode<NodeData> currentNode = data.Entries.Last;
            NodeData currentBuildEntry = currentNode.Value;
            StringBuilder currentBuilder = currentBuildEntry.Builder;

            int positionOffset = currentNode.GetBuildOffset();
            if (data.UpdateBuildLength())
            {
                //Handle color reset
                currentBuildEntry.Current = new FormatData()
                {
                    BuildOffset = positionOffset,
                    LocalPosition = currentBuildEntry.LastCheckedBuildLength
                };
                provider.ResetColor(currentBuilder, currentBuildEntry.Current);
                data.UpdateBuildLength();
            }
            else
            {
                data.BypassColorCancellation = false;
            }

            //This will replace the last FormatData instance with the current one - this is by design
            currentBuildEntry.Current = new FormatData()
            {
                BuildOffset = positionOffset,
                LocalPosition = currentBuilder.Length
            };
        }
    }

    internal static class FormatDataAccess
    {
        private static readonly ConditionalWeakTable<IColorFormatProvider, Data> colorFormatCWT = new();

        internal static Data GetData(this IColorFormatProvider self) => colorFormatCWT.GetValue(self, _ => new());

        internal static int GetBuildOffset(this LinkedListNode<NodeData> node)
        {
            LinkedListNode<NodeData> previousNode = node.Previous;

            //Position in the string is the combined length of strings built up until this point. Since this value is cumulative, we only need to reference the
            //format data of the last build node for an accurate length
            return previousNode != null ? previousNode.Value.Current.Position : 0;
        }

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
            /// Allows color range check to not be reset when an ANSI color code is detected
            /// </summary>
            public bool BypassColorCancellation;

            /// <summary>
            /// Ansi color code escape sequence has been detected. Flag ensures that the code itself isn't considered as including colorable characters
            /// </summary>
            public bool ExpectAnsiCode;

            public void SetEntry(StringBuilder builder)
            {
                NodeData data = new NodeData()
                {
                    Builder = builder
                };
                Entries.AddLast(new LinkedListNode<NodeData>(data));
            }

            public void EntryComplete(IColorFormatProvider provider)
            {
                LinkedListNode<NodeData> currentNode = Entries.Last;
                NodeData currentBuildEntry = currentNode.Value;

                if (UpdateBuildLength())
                {
                    //Handle color reset
                    currentBuildEntry.Current = new FormatData()
                    {
                        BuildOffset = currentNode.GetBuildOffset(),
                        LocalPosition = currentBuildEntry.LastCheckedBuildLength
                    };
                    provider.ResetColor(currentBuildEntry.Builder, currentBuildEntry.Current);
                    UpdateBuildLength();
                }
                else
                {
                    BypassColorCancellation = false;
                }

                int lastBuildLength = currentBuildEntry.LastCheckedBuildLength = currentBuildEntry.Builder.Length;
                Entries.RemoveLast();

                //Check whether there are still active entries before clearing CWT state
                currentNode = Entries.Last;
                if (currentNode != null)
                {
                    currentNode.Value.LastCheckedBuildLength += lastBuildLength;
                    return;
                }
                Clear();
            }

            /// <summary>
            /// Set data fields back to default values
            /// </summary>
            public void Clear()
            {
                Entries.Clear();
                RangeCounter = 0;
                BypassColorCancellation = ExpectAnsiCode = false;
            }

            public bool UpdateBuildLength()
            {
                LinkedListNode<NodeData> currentNode = Entries.Last;

                NodeData currentBuildEntry = currentNode.Value;
                StringBuilder currentBuilder = currentBuildEntry.Builder;

                //UtilityLogger.DebugLog("Build position: " + currentBuildEntry.LastCheckedBuildLength);

                int numCharsSinceLastArgument = currentBuilder.Length - currentBuildEntry.LastCheckedBuildLength;

                //We are not expecting there to be format information in the string here
                if (!ExpectAnsiCode && (RangeCounter == 0 || numCharsSinceLastArgument == 0))
                {
                    //if (numCharsSinceLastArgument > 0)
                    //{
                    //    string unprocessedBuildString = currentBuilder.ToString().Substring(currentBuilder.Length - numCharsSinceLastArgument);

                    //    UtilityLogger.DebugLog($"'{unprocessedBuildString}' will remain at the last assigned color");
                    //}
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

                        if (!BypassColorCancellation)
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

                                //We have been signaled to skip over this ANSI code - any chars after the terminator may applied towards the range counter
                                if (BypassColorCancellation)
                                    ansiTerminatorDetected = ExpectAnsiCode = BypassColorCancellation = false;
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
                        currentBuildEntry.LastCheckedBuildLength = i + 1; //Adhere to an index + 1 standard
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
