using System;
using System.Runtime.CompilerServices;
using System.Text;

namespace LogUtils.Formatting
{
    /// <summary>
    /// A struct that processes interpolated string data into a formatted string
    /// </summary>
    [InterpolatedStringHandler]
    public struct InterpolatedStringHandler : IFormattable
    {
        /// <summary>
        /// Stores the interpolate message string
        /// </summary>
        private readonly StringBuilder builder;

        private readonly ArgumentInfo[] arguments;

        private int argumentIndex = -1;

        public InterpolatedStringHandler(int literalLength, int formattedCount)
        {
            builder = new StringBuilder(literalLength);
            arguments = new ArgumentInfo[formattedCount];
        }

        /// <summary>
        /// Adds a string component for later formatting (used by compiled code)
        /// </summary>
        public readonly void AppendLiteral(string literal)
        {
            builder.Append(literal);
        }

        /// <summary>
        /// Adds an object component for later formatting (used by compiled code)
        /// </summary>
        /// <exception cref="InvalidOperationException">More than the amount of expected arguments were provided to the handler</exception>
        public void AppendFormatted<T>(T argument)
        {
            if (argumentIndex == arguments.Length)
                throw new InvalidOperationException("Handler does not accept additional arguments");

            argumentIndex++;
            arguments[argumentIndex] = new ArgumentInfo(argument, builder.Length);
        }

        /// <summary>
        /// Adds an object component for later formatting (used by compiled code)
        /// </summary>
        /// <exception cref="InvalidOperationException">More than the amount of expected arguments were provided to the handler</exception>
        public void AppendFormatted<T>(T argument, int alignment) //TODO: Implement alignment
        {
            if (argumentIndex == arguments.Length - 1)
                throw new InvalidOperationException("Handler does not accept additional arguments");

            argumentIndex++;
            arguments[argumentIndex] = new ArgumentInfo(argument, builder.Length);
        }

        internal readonly void SetBuildString(string value)
        {
            builder.Clear();
            builder.Capacity = value.Length;
            builder.Append(value);
        }

        /// <inheritdoc/>
        public override readonly string ToString()
        {
            return ToString(null, null);
        }

        /// <inheritdoc/>
        public readonly string ToString(string format, IFormatProvider formatProvider)
        {
            if (arguments.Length == 0)
                return builder.ToString();

            FormatProcessor processor = new FormatProcessor(format, formatProvider);

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

        private readonly struct ArgumentInfo(object argument, int index)
        {
            /// <summary>
            /// An object, or value to be inserted into the builder string
            /// </summary>
            public readonly object Argument = argument;

            /// <summary>
            /// The position in the builder string at the time of format
            /// </summary>
            public readonly int Index = index;
        }

        private readonly ref struct FormatProcessor
        {
            public readonly string Format;
            public readonly IFormatProvider Provider;
            public readonly ICustomFormatter Formatter;

            public FormatProcessor(string format, IFormatProvider provider)
            {
                Format = format;
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
                        object formatArgument = FormatData.ResolveArgument(info.Argument, Formatter, 0);
                        argument = Formatter.Format(Format, formatArgument, Provider);
                    }
                    else if (info.Argument is IFormattable formattable)
                    {
                        argument = formattable.ToString(Format, Provider);
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
