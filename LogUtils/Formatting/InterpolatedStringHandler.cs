using System;
using System.Runtime.CompilerServices;
using System.Text;

namespace LogUtils.Formatting
{
    /// <summary>
    /// A class that processes interpolated string data into a formatted string
    /// </summary>
    [InterpolatedStringHandler]
    public class InterpolatedStringHandler : FormattableString
    {
        /// <summary>
        /// Stores the interpolate message string
        /// </summary>
        private readonly StringBuilder builder;

        private readonly ArgumentInfo[] arguments;

        private int argumentIndex = -1;

        /// <inheritdoc/>
        public override string Format => BuildFormat();

        /// <inheritdoc/>
        public override int ArgumentCount => arguments.Length;

        public InterpolatedStringHandler(int literalLength, int formattedCount)
        {
            builder = new StringBuilder(literalLength);
            arguments = new ArgumentInfo[formattedCount];
        }

        /// <summary>
        /// Adds a string component for later formatting (used by compiled code)
        /// </summary>
        public void AppendLiteral(string literal)
        {
            builder.Append(literal);
        }

        /// <summary>
        /// Adds an object component for later formatting (used by compiled code)
        /// </summary>
        /// <param name="argument">An argument to be formatted</param>
        /// <exception cref="InvalidOperationException">More than the amount of expected arguments were provided to the handler</exception>
        public void AppendFormatted<T>(T argument)
        {
            if (argumentIndex == arguments.Length)
                throw new InvalidOperationException("Handler does not accept additional arguments");

            argumentIndex++;
            arguments[argumentIndex] = new ArgumentInfo(argument, builder.Length);
        }

        /// <inheritdoc cref="AppendFormatted{T}(T)"/>
        /// <param name="argument">An argument to be formatted</param>
        /// <param name="alignment">Value affects padded space unless used with a <see cref="UnityEngine.Color"/> or <see cref="ConsoleColor"/>, of which it represents the number of formatted characters</param>
        public void AppendFormatted<T>(T argument, int alignment)
        {
            if (argumentIndex == arguments.Length - 1)
                throw new InvalidOperationException("Handler does not accept additional arguments");

            argumentIndex++;
            arguments[argumentIndex] = new ArgumentInfo(argument, builder.Length, alignment);
        }

        /// <inheritdoc cref="AppendFormatted{T}(T)"/>
        /// <param name="argument">An argument to be formatted</param>
        /// <param name="format">Format specification applicable to an argument</param>
        public void AppendFormatted<T>(T argument, string format)
        {
            if (argumentIndex == arguments.Length - 1)
                throw new InvalidOperationException("Handler does not accept additional arguments");

            argumentIndex++;
            arguments[argumentIndex] = new ArgumentInfo(argument, builder.Length, format: format);
        }

        /// <inheritdoc cref="AppendFormatted{T}(T)"/>
        /// <param name="argument">An argument to be formatted</param>
        /// <param name="format">Format specification applicable to an argument</param>
        /// <param name="alignment">Value affects padded space unless used with a <see cref="UnityEngine.Color"/> or <see cref="ConsoleColor"/>, of which it represents the number of formatted characters</param>
        public void AppendFormatted<T>(T argument, string format, int alignment)
        {
            if (argumentIndex == arguments.Length - 1)
                throw new InvalidOperationException("Handler does not accept additional arguments");

            argumentIndex++;
            arguments[argumentIndex] = new ArgumentInfo(argument, builder.Length, alignment, format);
        }

        /// <summary>
        /// Builds the format string out of appended string literals, and format arguments
        /// </summary>
        /// <returns></returns>
        internal string BuildFormat()
        {
        }

        /// <inheritdoc/>
        public override object[] GetArguments() => arguments.Select(info => info.Argument).ToArray();

        /// <inheritdoc/>
        public override object GetArgument(int index) => arguments[index].Argument;

        internal void SetBuildString(string value)
        {
            builder.Clear();
            builder.Capacity = value.Length;
            builder.Append(value);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return ToString(null);
        }

        /// <inheritdoc/>
        public override string ToString(IFormatProvider formatProvider)
        {
            if (arguments.Length == 0)
                return builder.ToString();

            FormatProcessor processor = new FormatProcessor(formatProvider);

            var formatData = processor.AccessData();

            //Set a new format entry for this builder - used for color tracking and is removed after format process is completed
            if (formatData != null)
                formatData.SetEntry(builder);

            string initialBuildString = builder.ToString(); //Preserve the original string literals before including the formatted arguments

            string formattedString;
            try
            {
                processor.Process(builder, arguments);
                formattedString = builder.ToString();
            }
            finally
            {
                formatData?.EntryComplete((IColorFormatProvider)processor.Formatter);
                SetBuildString(initialBuildString);
            }
            return formattedString;
        }

        private readonly struct ArgumentInfo(object argument, int index, int range = 0, string format = "o")
        {
            /// <summary>
            /// An object, or value to be inserted into the builder string
            /// </summary>
            public readonly object Argument = argument;

            /// <summary>
            /// The format specifier code
            /// </summary>
            public readonly string Format = format;

            /// <summary>
            /// The position in the builder string at the time of format
            /// </summary>
            public readonly int Index = index;

            /// <summary>
            /// The number of characters to apply the format
            /// </summary>
            public readonly int Range = range;
        }

        private readonly ref struct FormatProcessor
        {
            public readonly IFormatProvider Provider;
            public readonly ICustomFormatter Formatter;

            public FormatProcessor(IFormatProvider provider)
            {
                Provider = provider;

                if (Provider != null)
                    Formatter = (ICustomFormatter)provider.GetFormat(typeof(ICustomFormatter));
            }

            public readonly void Process(StringBuilder builder, ArgumentInfo[] arguments)
            {
                int indexOffset = 0; //The change in the build string length as arguments are inserted into the string
                for (int i = 0; i < arguments.Length; i++)
                {
                    ArgumentInfo info = arguments[i];

                    //Converts argument to a string
                    string argument;
                    if (Formatter != null)
                    {
                        IColorFormatProvider colorFormatter = Formatter as IColorFormatProvider;

                        if (colorFormatter != null)
                            FormatData.UpdateData(colorFormatter);

                        //Ensures that color data is stored as an argument, a requirement by color format providers
                        object formatArgument = FormatData.ResolveArgument(info.Argument, Formatter, info.Range);
                        argument = Formatter.Format(info.Format, formatArgument, Provider);
                    }
                    else if (info.Argument is IFormattable formattable)
                    {
                        argument = formattable.ToString(info.Format, Provider);
                    }
                    else
                    {
                        argument = info.Argument?.ToString();
                    }

                    if (string.IsNullOrEmpty(argument)) //Inserting wont add any characters
                        continue;

                    builder.Insert(info.Index + indexOffset, argument);
                    indexOffset += argument.Length;
                }
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
