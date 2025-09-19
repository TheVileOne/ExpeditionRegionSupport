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

        private bool formatted;

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

        /// <inheritdoc/>
        public override string ToString()
        {
            return ToString(null, null);
        }

        /// <inheritdoc/>
        public string ToString(string format, IFormatProvider formatProvider)
        {
            if (!formatted)
                processArguments();

            return builder.ToString();
        }

        private void processArguments()
        {
            int indexOffset = 0; //The change in the build string length as arguments are inserted into the string
            for (int i = 0; i < arguments.Length; i++)
            {
                ArgumentInfo info = arguments[i];

                string argument = info.Argument?.ToString();

                if (string.IsNullOrEmpty(argument)) //Inserting wont add any characters
                    continue;

                builder.Insert(info.Index + indexOffset, argument);
                indexOffset += argument.Length;
            }
            formatted = true;
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
    }
}
