using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using static LogUtils.Formatting.FormatDataAccess;

namespace LogUtils.Formatting
{
    /// <summary>
    /// A class that processes interpolated string data into a formatted string
    /// </summary>
    [InterpolatedStringHandler]
    public class InterpolatedStringHandler : FormattableString
    {
        private FormatProcessor processor;

        private readonly List<LiteralInfo> literals;
        private readonly List<ArgumentInfo> arguments;

        private int elementCount => literals.Count + arguments.Count;

        /// <inheritdoc/>
        public override string Format => BuildFormat();

        /// <inheritdoc/>
        public override int ArgumentCount => arguments.Count;

        public InterpolatedStringHandler()
        {
            literals = new List<LiteralInfo>();
            arguments = new List<ArgumentInfo>();
        }

        public InterpolatedStringHandler(int literalLength, int formattedCount)
        {
            literals = new List<LiteralInfo>(formattedCount + 1);
            arguments = new List<ArgumentInfo>(formattedCount);
        }

        /// <summary>
        /// Adds a string component for later formatting (used by compiled code)
        /// </summary>
        public void AppendLiteral(string literal)
        {
            literals.Add(new LiteralInfo(literal, elementCount));
        }

        /// <summary>
        /// Adds an object component for later formatting (used by compiled code)
        /// </summary>
        /// <param name="argument">An argument to be formatted</param>
        /// <exception cref="InvalidOperationException">More than the amount of expected arguments were provided to the handler</exception>
        public void AppendFormatted<T>(T argument)
        {
            arguments.Add(new ArgumentInfo(argument, elementCount));
        }

        /// <inheritdoc cref="AppendFormatted{T}(T)"/>
        /// <param name="argument">An argument to be formatted</param>
        /// <param name="alignment">Value affects padded space unless used with a <see cref="UnityEngine.Color"/> or <see cref="ConsoleColor"/>, of which it represents the number of formatted characters</param>
        public void AppendFormatted<T>(T argument, int alignment)
        {
            arguments.Add(new ArgumentInfo(argument, elementCount, alignment));
        }

        /// <inheritdoc cref="AppendFormatted{T}(T)"/>
        /// <param name="argument">An argument to be formatted</param>
        /// <param name="format">Format specification applicable to an argument</param>
        public void AppendFormatted<T>(T argument, string format)
        {
            arguments.Add(new ArgumentInfo(argument, elementCount, format: format));
        }

        /// <inheritdoc cref="AppendFormatted{T}(T)"/>
        /// <param name="argument">An argument to be formatted</param>
        /// <param name="format">Format specification applicable to an argument</param>
        /// <param name="alignment">Value affects padded space unless used with a <see cref="UnityEngine.Color"/> or <see cref="ConsoleColor"/>, of which it represents the number of formatted characters</param>
        public void AppendFormatted<T>(T argument, string format, int alignment)
        {
            arguments.Add(new ArgumentInfo(argument, elementCount, alignment, format));
        }

        /// <summary>
        /// Builds the format string out of appended string literals, and format arguments
        /// </summary>
        /// <returns></returns>
        internal string BuildFormat()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public override object[] GetArguments() => arguments.Select(argument => argument.Value).ToArray();

        /// <inheritdoc/>
        public override object GetArgument(int index) => arguments[index].Value;

        /// <inheritdoc/>
        public override string ToString()
        {
            return ToString(null);
        }

        /// <inheritdoc/>
        public override string ToString(IFormatProvider formatProvider)
        {
            StringBuilder builder = new StringBuilder();
            processor = new FormatProcessor(formatProvider);

            var formatData = processor.AccessData();

            //Set a new format entry for this builder - used for color tracking and is removed after format process is completed
            if (formatData != null)
                formatData.SetEntry(builder);

            try
            {
                if (arguments.Count == 0)
                {
                    for (int i = 0; i < literals.Count; i++)
                        builder.Append(literals[i].Value);
                }
                else if (literals.Count == 0)
                {
                    for (int i = 0; i < arguments.Count; i++)
                    {
                        if (formatData != null)
                            formatData.UpdateBuildLength();

                        string argumentString = processor.Process(arguments[i]);

                        if (!string.IsNullOrEmpty(argumentString))
                            builder.Append(argumentString);
                    }
                }
                else //At least one literal and one argument
                {
                    int literalsHandled = 0,
                        argumentsHandled = 0;

                    bool checkLiteralsFirst = true;
                    for (int i = 0; i < elementCount; i++)
                    {
                        if (formatData != null)
                            formatData.UpdateBuildLength();

                        if (checkLiteralsFirst)
                        {
                            if (!tryProcessLiteral())
                            {
                                tryProcessArgument(); //This should always succeed
                                checkLiteralsFirst = true; //After processing an argument, target a literal
                                continue;
                            }
                            checkLiteralsFirst = false; //It is improbable that we will be required to process consecuative literals - .NET merges such literals
                        }
                        else
                        {
                            if (!tryProcessArgument())
                            {
                                tryProcessLiteral(); //This should always succeed
                                checkLiteralsFirst = false;
                                continue;
                            }
                            checkLiteralsFirst = true;
                        }

                        bool tryProcessLiteral()
                        {
                            if (literalsHandled == literals.Count)
                                return false;

                            LiteralInfo literal = literals[literalsHandled];

                            if (literal.BuildPosition == i)
                            {
                                literalsHandled++;
                                builder.Append(literal.Value);
                                return true;
                            }
                            return false;
                        }

                        bool tryProcessArgument()
                        {
                            if (argumentsHandled == arguments.Count)
                                return false;

                            ArgumentInfo argument = arguments[argumentsHandled];

                            if (argument.BuildPosition == i)
                            {
                                argumentsHandled++;
                                string argumentString = processor.Process(argument);

                                if (!string.IsNullOrEmpty(argumentString))
                                    builder.Append(argumentString);
                                return true;
                            }
                            return false;
                        }
                    }
                }
            }
            finally
            {
                if (formatData != null)
                    formatData.EntryComplete((IColorFormatProvider)processor.Formatter);
            }
            return builder.ToString();
        }

        private readonly struct LiteralInfo(string value, int position)
        {
            /// <summary>
            /// The position in the builder string at the time of format
            /// </summary>
            public readonly int BuildPosition = position;

            /// <summary>
            /// The value of the literal
            /// </summary>
            public readonly string Value = value;
        }

        internal readonly struct ArgumentInfo(object argument, int position, [Optional]int range, [Optional]string format)
        {
            /// <summary>
            /// An object, or value to be inserted into the builder string
            /// </summary>
            public readonly object Value = argument;

            /// <summary>
            /// The format specifier code
            /// </summary>
            public readonly string Format = format;

            /// <summary>
            /// The position in the builder string at the time of format
            /// </summary>
            public readonly int BuildPosition = position;

            /// <summary>
            /// The number of characters to apply the format
            /// </summary>
            public readonly int Range = range;
        }

        private readonly struct FormatProcessor
        {
            public readonly IFormatProvider Provider;
            public readonly ICustomFormatter Formatter;

            public FormatProcessor(IFormatProvider provider)
            {
                Provider = provider;

                if (Provider != null)
                    Formatter = (ICustomFormatter)provider.GetFormat(typeof(ICustomFormatter));
            }

            public readonly string Process(in ArgumentInfo argument)
            {
                //Converts argument to a string
                string resultString;
                if (Formatter != null)
                {
                    IColorFormatProvider colorFormatter = Formatter as IColorFormatProvider;

                    if (colorFormatter != null)
                    {
                        FormatData.UpdateData(colorFormatter);
                    }

                    //Ensures that color data is stored as an argument, a requirement by color format providers
                    object formatArgument = FormatData.ResolveArgument(argument.Value, Formatter, argument.Range);
                    resultString = Formatter.Format(argument.Format, formatArgument, Provider);
                }
                else if (argument.Value is IFormattable formattable)
                {
                    resultString = formattable.ToString(argument.Format, Provider);
                }
                else
                {
                    resultString = argument.Value?.ToString();
                }
                return resultString;
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

            /// <summary>
            /// Accesses color related format data
            /// </summary>
            public readonly FormatDataAccess.Data AccessData()
            {
                if (Formatter is IColorFormatProvider colorFormatter)
                    return colorFormatter.GetData();
                return null;
            }
        }
    }
}
